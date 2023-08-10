using JJMasterData.Commons.Data;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Extensions;
using JJMasterData.Web.Extensions;
using JJMasterData.Pdf;
using JJMasterData.WebExample.Authorization;
using Newtonsoft.Json;

namespace JJMasterData.WebExample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var root = Path.GetFullPath(Path.Join(builder.Environment.ContentRootPath, "..", ".."));
        var settingsPath = Path.Combine(root, "appsettings.json");

        //Without this IConfiguration don't work. If we add inside AddJJMasterDataWeb, I don't know why performance goes to trash.
        builder.Configuration.AddJsonFile(settingsPath, optional: false, reloadOnChange: true);
        builder.Services.AddRazorPages().AddViewLocalization();
        builder.Services.AddJJMasterDataWeb(settingsPath)
            .WithPdfExportation();
            // .WithEntityRepository(
            //     "data source=localhost,1433;initial catalog=JJMasterData;user=sa;password=Test@123456",
            //     DataAccessProviderType.SqlServer);
        //.WithDataAccess(_=>new DataAccess(builder.Configuration.GetConnectionString("ConnectionString"),"System.Data.SqlClient"));
            //.WithMongoDbDataDictionary();
            //.WithPythonFormEventResolver(options => options.ScriptsPath = "../../example/JJMasterData.WebExample/FormEvents/Python");
        
        //You can also:
        // builder.Services.AddJJMasterDataWeb(options =>
        // {
        //     options.JJMasterDataCore.BootstrapVersion = 5;
        // });
        
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("MasterDataPolicy", policy =>
            {
                policy.AddRequirements(new AllowAnonymousAuthorizationRequirement());
            });
        });
        
        var app = builder.Build();
        if (app.Environment.IsProduction())
            app.UseHsts();

        app.UseExceptionHandler("/Error");
        app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapRazorPages();
        app.UseAuthorization();
        app.UseJJMasterDataWeb();
        app.MapJJMasterData()
            .RequireAuthorization("MasterDataPolicy");
        
        app.Run();
    }
}