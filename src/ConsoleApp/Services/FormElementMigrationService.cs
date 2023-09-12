using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Data;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Exceptions;
using JJMasterData.ConsoleApp.Repository;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Configuration;

namespace JJMasterData.ConsoleApp.Services;

public class FormElementMigrationService
{
    private IEntityRepository EntityRepository { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private MetadataRepository MetadataRepository { get; }
    private DataAccess DataAccess { get; }
    private string TableName { get; }
    
    public FormElementMigrationService(
        IEntityRepository entityRepository, 
        IDataDictionaryRepository dataDictionaryRepository, 
        MetadataRepository metadataRepository,
        IConfiguration configuration
        )
    {
        string? strConn = configuration.GetConnectionString("ConnectionString");
        if (strConn == null)
            throw new JJMasterDataException("ConnectionString not especified");
        
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        MetadataRepository = metadataRepository;
        DataAccess = new DataAccess(strConn, DataAccessProvider.SqlServer);
        TableName = configuration.GetJJMasterData().GetValue<string>("DataDictionaryTableName")!;
    }
    
    public void Migrate()
    {
        var start = DateTime.Now;
        var databaseDictionaries = MetadataRepository.GetMetadataList();
        
        foreach (var metadata in databaseDictionaries)
        {
            var formElement = metadata.GetFormElement();
            DataDictionaryRepository.InsertOrReplaceAsync(formElement).GetAwaiter().GetResult();
            Console.WriteLine($@"✅ {formElement.Name}");
        }

        DataAccess.SetCommand($"delete from {TableName} where type <> 'F'");

        Console.WriteLine($@"Process started: {start}");
        Console.WriteLine($@"Process finished: {DateTime.Now}");
        Console.ReadLine();
        //TODO: Recriar a proc e table
    }
}