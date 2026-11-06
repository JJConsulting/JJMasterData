using System.Text;
using JJMasterData.Commons.Data.Csv;
using JJMasterData.Commons.Data.Entity.Providers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.WebApi.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

BenchmarkSwitcher.FromAssembly(typeof(CsvHttpBenchmark).Assembly).Run(args);

[MemoryDiagnoser]
public class CsvHttpBenchmark
{
    public IEnumerable<int> RowCounts => Environment.GetEnvironmentVariable("CSV_BENCH_ROWS") is { } value
        ? value.Split(',').Select(int.Parse)
        : new[] { 5, 1000, 10000 };
    [ParamsSource(nameof(RowCounts))] public int Rows { get; set; }
    private MsSqlContainer container = null!;
    private WebApplication app = null!;
    private HttpClient client = null!;
    private readonly FormElement element = new() { Name = "CsvBench", TableName = "CsvBench", UseReadProcedure = true, ReadProcedureName = "CsvBenchGet" };

    [GlobalSetup]
    public async Task Setup()
    {
        container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04").Build();
        await container.StartAsync();
        await using (var connection = new SqlConnection(container.GetConnectionString()))
        {
            await connection.OpenAsync();
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE CsvBench (id int NOT NULL PRIMARY KEY, name nvarchar(100), date datetime2, amount decimal(18,2));
                INSERT CsvBench SELECT value, CONCAT(N'Produto ',value), CAST('2026-11-03T14:57:00' AS datetime2), 1234.56 FROM GENERATE_SERIES(1,10000);
                """;
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = """
                CREATE PROCEDURE CsvBenchGet @orderby nvarchar(100)=NULL, @regporpag int=5, @pag int=1, @qtdtotal int=-1 OUTPUT AS
                BEGIN
                    SET NOCOUNT ON;
                    IF @qtdtotal <> -1 SELECT @qtdtotal=COUNT(*) FROM CsvBench;
                    SELECT id,name,date,amount FROM CsvBench ORDER BY id OFFSET ((@pag-1)*@regporpag) ROWS FETCH NEXT @regporpag ROWS ONLY;
                END
                """;
            await cmd.ExecuteNonQueryAsync();
        }
        foreach (var (name,type) in new[]{("id",FieldType.Int),("name",FieldType.NVarchar),("date",FieldType.DateTime),("amount",FieldType.Decimal)})
            element.Fields.Add(new FormElementField { Name=name, DataType=type });
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddMemoryCache();
        builder.Services.AddJJMasterDataCommons(o => o.ConnectionString=container.GetConnectionString());
        app = builder.Build();
        app.MapGet("/old", async (int rows, IEntityRepository repository) => {
#pragma warning disable CS0612
            var legacy = new CsvHttpBenchmarks.LegacyPlainTextReader(
                app.Services.GetRequiredService<IEntityProvider>(),
                app.Services.GetRequiredService<IConnectionRepository>(),
                app.Services.GetRequiredService<ILogger<CsvHttpBenchmarks.LegacyPlainTextReader>>());
            var text = await legacy.GetFieldsListAsTextAsync(element, new EntityParameters { CurrentPage=1, RecordsPerPage=rows });
#pragma warning restore CS0612
            return Results.Text(text,"text/csv");
        });
        app.MapGet("/new", async (int rows, IEntityRepository repository) => {
            var result=await repository.GetDictionaryListResultAsync(element,new EntityParameters { CurrentPage=1, RecordsPerPage=rows },true);
            var response=new CsvHttpBenchmarks.OriginalListResponse { TotalOfRecords=result.TotalOfRecords };
            response.SetData(element,result.Data);
            return Results.Text(response.ToCsv(),"text/csv");
        });
        app.MapGet("/optimized", (int rows, IEntityRepository repository, HttpContext context) =>
            Results.Stream(async stream =>
            {
                // This writer owns no stream. On failure, discard its managed buffer instead of
                    // flushing it during disposal and starting an otherwise untouched HTTP response.
                    var writer = new StreamWriter(stream, new UTF8Encoding(false), leaveOpen: true);
                await repository.WriteCsvAsync(element, writer,
                    new EntityParameters { CurrentPage=1, RecordsPerPage=rows },
                    new CsvWriteOptions { NewLine=Environment.NewLine }, context.RequestAborted);
                await writer.FlushAsync(context.RequestAborted);
                    await writer.DisposeAsync();
            }, "text/csv; charset=utf-8"));
        await app.StartAsync();
        client=new HttpClient { BaseAddress=new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
        client.DefaultRequestHeaders.Accept.ParseAdd("text/csv");
        var oldText=await Fetch("old"); var newText=await Fetch("new");
        if(oldText != newText || newText != await Fetch("optimized") || oldText.Split('\n',StringSplitOptions.RemoveEmptyEntries).Length != Rows)
            throw new InvalidOperationException("CSV output/row count mismatch");
        await using (var connection = new SqlConnection(container.GetConnectionString()))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "UPDATE CsvBench SET name=@text, amount=NULL WHERE id=1";
            command.Parameters.AddWithValue("@text", " A|B\r\nC\"  ");
            await command.ExecuteNonQueryAsync();
            if (await Fetch("new") != await Fetch("optimized"))
                throw new InvalidOperationException("Special values, dates or nulls differ in streamed CSV");
            command.CommandText = "UPDATE CsvBench SET name=N'Produto 1',amount=1234.56 WHERE id=1";
            await command.ExecuteNonQueryAsync();
        }
        for(int i=0;i<10;i++){await Fetch("old");await Fetch("new");await Fetch("optimized");}
    }
    private async Task<string> Fetch(string path)
    {
        using var response=await client.GetAsync($"/{path}?rows={Rows}");
        response.EnsureSuccessStatusCode();
        if(response.Content.Headers.ContentType?.MediaType != "text/csv") throw new InvalidOperationException("Wrong content type");
        return await response.Content.ReadAsStringAsync();
    }
    [Benchmark(Baseline=true)] public Task<string> Old()=>Fetch("old");
    [Benchmark] public Task<string> New()=>Fetch("new");
    [Benchmark] public Task<string> Optimized()=>Fetch("optimized");
    [GlobalCleanup] public async Task Cleanup(){client?.Dispose();if(app is not null)await app.DisposeAsync();if(container is not null)await container.DisposeAsync();}
}
