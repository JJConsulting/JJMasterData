using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.ConsoleApp.Repository;
using JJMasterData.Core.Configuration.Options;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.ConsoleApp.Services;

public class FormElementMigrationService
{
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private MetadataRepository MetadataRepository { get; }
    private ExpressionsMigrationService ExpressionsMigrationService { get; }
    private ILogger<FormElementMigrationService> Logger { get; }
    private DataAccess DataAccess { get; }
    private string TableName => Options.DataDictionaryTableName;
    private JJMasterDataCoreOptions Options { get; }
    public FormElementMigrationService(
        IDataDictionaryRepository dataDictionaryRepository, 
        MetadataRepository metadataRepository,
        IOptions<JJMasterDataCoreOptions> options,
        ExpressionsMigrationService expressionsMigrationService,
        ILogger<FormElementMigrationService> logger)
    {
        DataDictionaryRepository = dataDictionaryRepository;
        MetadataRepository = metadataRepository;
        ExpressionsMigrationService = expressionsMigrationService;
        Logger = logger;
        DataAccess = new DataAccess(options.Value.ConnectionString, DataAccessProvider.SqlServer);
        Options = options.Value;
    }
    
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
        DataAccess.SetCommand($"DROP PROCEDURE {Options.GetReadProcedureName(TableName)}");
        DataAccess.SetCommand($"DROP PROCEDURE {Options.GetWriteProcedureName(TableName)}");
        
        DataDictionaryRepository.CreateStructureIfNotExistsAsync().GetAwaiter().GetResult();
        
        Logger.LogInformation("\u2705 Re-created {TableName} and all related stored procedures", TableName);
        
        foreach (var metadata in databaseDictionaries)
        {
            var formElement = metadata.GetFormElement();
            
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