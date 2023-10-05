using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.ConsoleApp.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace JJMasterData.ConsoleApp.Services;

public class FormElementMigrationService
{
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private MetadataRepository MetadataRepository { get; }
    private DataAccess DataAccess { get; }
    private string TableName => Options.DataDictionaryTableName;
    private JJMasterDataCoreOptions Options { get; }
    public FormElementMigrationService(
        IEntityRepository entityRepository, 
        IDataDictionaryRepository dataDictionaryRepository, 
        MetadataRepository metadataRepository,
        IOptions<JJMasterDataCoreOptions> options
        )
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        MetadataRepository = metadataRepository;
        DataAccess = new DataAccess(options.Value.ConnectionString, DataAccessProvider.SqlServer);
        Options = options.Value;
    }
    
    public void Migrate()
    {
        var start = DateTime.Now;
        var databaseDictionaries = MetadataRepository.GetMetadataList();
        
        DataAccess.SetCommand($"DROP TABLE {TableName}");
        DataAccess.SetCommand($"DROP PROCEDURE {Options.GetReadProcedureName(TableName)}");
        DataAccess.SetCommand($"DROP PROCEDURE {Options.GetWriteProcedureName(TableName)}");
        
        DataDictionaryRepository.CreateStructureIfNotExistsAsync().GetAwaiter().GetResult();
        
        Console.WriteLine($@"✅ Re-created {TableName} and all related stored procedures.");
        
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
            Console.WriteLine($@"✅ {formElement.Name}");
        }

        DataAccess.SetCommand($"delete from {TableName} where type <> 'F'");
        
        DataAccess.SetCommand($$"""
                              UPDATE {{TableName}}
                              SET [json] = REPLACE([json],
                                  '{search_id}', 
                                  '{SearchId}') 
                              WHERE [json] LIKE '%{search_id}%';
                              """);
        
        Console.WriteLine($@"✅ Replaced {{search_id}} to {{SearchId}} in all elements.");
        
        DataAccess.SetCommand($$"""
                                UPDATE {{TableName}}
                                SET [json] = REPLACE([json],
                                    '{search_text}',
                                    '{SearchText}')
                                WHERE [json] LIKE '%{search_text}%';
                                """);
        
        Console.WriteLine($@"✅ Replaced {{search_text}} to {{SearchText}} in all elements.");
        
        DataAccess.SetCommand($$"""
                                UPDATE {{TableName}}
                                SET [json] = REPLACE([json],
                                    '{objname}',
                                    '{ComponentName}')
                                WHERE [json] LIKE '%{objid}%';
                                """);
        
        Console.WriteLine($@"✅ Replaced {{objname}} to {{ComponentName}} in all elements.");
        
        Console.WriteLine($@"Process started: {start}");
        Console.WriteLine($@"Process finished: {DateTime.Now}");
        Console.ReadLine();
    }
}