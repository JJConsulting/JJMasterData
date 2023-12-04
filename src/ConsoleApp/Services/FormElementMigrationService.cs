using JJMasterData.Commons.Data;
using JJMasterData.ConsoleApp.Repository;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.ConsoleApp.Services;

public class FormElementMigrationService(IDataDictionaryRepository dataDictionaryRepository, 
    MetadataRepository metadataRepository,
    IOptions<MasterDataCoreOptions> options,
    ExpressionsMigrationService expressionsMigrationService,
    ILogger<FormElementMigrationService> logger)
{
    private IDataDictionaryRepository DataDictionaryRepository { get; } = dataDictionaryRepository;
    private MetadataRepository MetadataRepository { get; } = metadataRepository;
    private ExpressionsMigrationService ExpressionsMigrationService { get; } = expressionsMigrationService;
    private ILogger<FormElementMigrationService> Logger { get; } = logger;
    private DataAccess DataAccess { get; } = new(options.Value.ConnectionString, DataAccessProvider.SqlServer);
    private string TableName => Options.DataDictionaryTableName;
    private MasterDataCoreOptions Options { get; } = options.Value;

    public void Migrate()
    {
        var start = DateTime.Now;
        
        var containsLegacyType = DataAccess.GetResult($"SELECT [type] from {TableName} where [type] <> 'F'");

        if (containsLegacyType is null)
        {
            Logger.LogInformation("✅ DataDictionary is already migrated");
            return;
        }
        
        var databaseDictionaries = MetadataRepository.GetMetadataList();
        
        DataAccess.SetCommand($"DROP TABLE {TableName}");
        // DataAccess.SetCommand($"DROP PROCEDURE {Options.GetReadProcedureName(TableName)}");
        // DataAccess.SetCommand($"DROP PROCEDURE {Options.GetWriteProcedureName(TableName)}");
        
        DataDictionaryRepository.CreateStructureIfNotExistsAsync().GetAwaiter().GetResult();
        
        Logger.LogInformation("\u2705 Re-created {TableName} and all related stored procedures", TableName);
        
        foreach (var metadata in databaseDictionaries)
        {
            var formElement = metadata.GetFormElement();

            formElement.UseReadProcedure = true;
            formElement.UseWriteProcedure = true;
            
            foreach (var field in formElement.Fields)
            {
                if (field.DataFile is not null)
                {
                    field.DataFile.MaxFileSize /= 1000000;
                }
            }
            
            DataDictionaryRepository.InsertOrReplaceAsync(formElement).GetAwaiter().GetResult();
            Logger.LogInformation("\u2705 {FormElementName}", formElement.Name);
        }

        DataAccess.SetCommand($"delete from {TableName} where type <> 'F'");
        
        DataAccess.SetCommand($$"""
                              UPDATE {{TableName}}
                              SET [json] = REPLACE([json],
                                  '{search_id}', 
                                  '{SearchId}') 
                              WHERE [json] LIKE '%{search_id}%';
                              """);
        
        Logger.LogInformation("✅ Replaced {{search_id}} to {{SearchId}} in all elements");
        
        DataAccess.SetCommand($$"""
                                UPDATE {{TableName}}
                                SET [json] = REPLACE([json],
                                    '{search_text}',
                                    '{SearchText}')
                                WHERE [json] LIKE '%{search_text}%';
                                """);
        
        Logger.LogInformation(@"✅ Replaced {{search_text}} to {{SearchText}} in all elements");
        
        DataAccess.SetCommand($$"""
                                UPDATE {{TableName}}
                                SET [json] = REPLACE([json],
                                    '{objname}',
                                    '{FieldName}')
                                WHERE [json] LIKE '%{objname}%';
                                """);
        
        Console.WriteLine(@"✅ Replaced {{objname}} to {{FieldName}} in all elements");
        
        Console.WriteLine($@"Process started: {start}");
        Console.WriteLine($@"Process finished: {DateTime.Now}");
        
        ExpressionsMigrationService.Migrate();
    }
}