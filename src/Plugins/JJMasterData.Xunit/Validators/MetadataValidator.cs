using System.Collections;
using JJMasterData.Commons.Dao;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Xunit.Tester;
using Xunit;

namespace JJMasterData.Xunit.Validators;

public class MetadataValidator
{
    public IEntityRepository Repository { get; }
    public IFormEventResolver FormEventResolver { get; }

    public MetadataValidator(IEntityRepository repository, IFormEventResolver formEventResolver)
    {
        Repository = repository;
        FormEventResolver = formEventResolver;
    }
    
    public Metadata AssertAllOperations(Metadata metadata, Hashtable? values = null)
    {
        var tester = new DataDictionaryTester(metadata,Repository,FormEventResolver);
        var result = tester.AllOperations(values);
        Assert.True(result.IsValid);
        return metadata;
    }
    
    public Metadata AssertAllOperations(Metadata metadata, Action<DataDictionaryTesterValues> configure)
    {
        var tester = new DataDictionaryTester(metadata,Repository,FormEventResolver);
        var result = tester.AllOperations(configure);
        Assert.True(result.IsValid);
        return metadata;
    }
    
    public Metadata AssertInsert(Metadata metadata, Hashtable? values = null)
    {
        var tester = new DataDictionaryTester(metadata,Repository,FormEventResolver);
        var result = tester.Insert(values);
        Assert.True(result.IsValid);
        return metadata;
    }
    
    public Metadata AssertUpdate(Metadata metadata, Hashtable values)
    {
        var tester =  new DataDictionaryTester(metadata,Repository,FormEventResolver);
        var result = tester.Update(values);
        Assert.True(result.IsValid);
        return metadata;
    }
    
    public Metadata AssertRead(Metadata metadata, Hashtable? values = null)
    {
        var tester = new DataDictionaryTester(metadata,Repository,FormEventResolver);
        var result = tester.Read(values);
        Assert.True(result.IsValid);
        return metadata;
    }
    
    public Metadata AssertDelete(Metadata metadata, Hashtable values)
    {
        var tester =  new DataDictionaryTester(metadata,Repository,FormEventResolver);
        var result = tester.Delete(values);
        Assert.True(result.IsValid);
        return metadata;
    }
}
