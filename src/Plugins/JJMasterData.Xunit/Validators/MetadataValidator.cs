using System.Collections;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Options;
using JJMasterData.Xunit.Tester;
using Microsoft.Extensions.Options;
using Xunit;

namespace JJMasterData.Xunit.Validators;

public class MetadataValidator
{
    private readonly CoreServicesFacade _coreServicesFacade;
    public IEntityRepository Repository { get; }
    public IFormEventResolver FormEventResolver { get; }
    public IOptions<JJMasterDataCoreOptions> Options { get; }

    public IHttpContext HttpContext { get; }

    public MetadataValidator(
        IEntityRepository repository,
        IHttpContext httpContext,
        CoreServicesFacade coreServicesFacade)
    {
        Repository = repository;
        FormEventResolver = coreServicesFacade.FormEventResolver;
        HttpContext = httpContext;
        Options = coreServicesFacade.Options;
        
        _coreServicesFacade = coreServicesFacade;
    }

    public Metadata AssertAllOperations(Metadata metadata, Hashtable? values = null)
    {
        var tester = new DataDictionaryTester(metadata, Repository, HttpContext, _coreServicesFacade);
        var result = tester.AllOperations(values);
        Assert.True(result.IsValid);
        return metadata;
    }

    public Metadata AssertAllOperations(Metadata metadata, Action<DataDictionaryTesterValues> configure)
    {
        var tester = new DataDictionaryTester(metadata, Repository, HttpContext, _coreServicesFacade);
        var result = tester.AllOperations(configure);
        Assert.True(result.IsValid);
        return metadata;
    }

    public Metadata AssertInsert(Metadata metadata, Hashtable? values = null)
    {
        var tester = new DataDictionaryTester(metadata, Repository, HttpContext, _coreServicesFacade);
        var result = tester.Insert(values);
        Assert.True(result.IsValid);
        return metadata;
    }

    public Metadata AssertUpdate(Metadata metadata, Hashtable values)
    {
        var tester = new DataDictionaryTester(metadata, Repository, HttpContext, _coreServicesFacade);
        var result = tester.Update(values);
        Assert.True(result.IsValid);
        return metadata;
    }

    public Metadata AssertRead(Metadata metadata, Hashtable? values = null)
    {
        var tester = new DataDictionaryTester(metadata, Repository, HttpContext, _coreServicesFacade);
        var result = tester.Read(values);
        Assert.True(result.IsValid);
        return metadata;
    }

    public Metadata AssertDelete(Metadata metadata, Hashtable values)
    {
        var tester = new DataDictionaryTester(metadata, Repository, HttpContext, _coreServicesFacade);
        var result = tester.Delete(values);
        Assert.True(result.IsValid);
        return metadata;
    }
}