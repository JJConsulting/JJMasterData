using System.Collections;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity.Abstractions;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.AuditLog;
using JJMasterData.Core.Facades;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Options;
using JJMasterData.Xunit.Tester;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace JJMasterData.Xunit.Validators;

public class MetadataValidator
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptions<JJMasterDataCoreOptions> _options;
    private readonly AuditLogService _auditLogService;

    public IEntityRepository Repository { get; }
    public IFormEventResolver FormEventResolver { get; }
    public IOptions<JJMasterDataCoreOptions> Options { get; }

    public IHttpContext HttpContext { get; }

    public MetadataValidator(IEntityRepository repository,
        IHttpContext httpContext, ILoggerFactory loggerFactory, IOptions<JJMasterDataCoreOptions> options,
        AuditLogService auditLogService, IFormEventResolver formEventResolver)
    {
        Repository = repository;
        FormEventResolver = formEventResolver;
        HttpContext = httpContext;
        _loggerFactory = loggerFactory;
        _options = options;
        _auditLogService = auditLogService;
        Options = options;
    }

    public Metadata AssertAllOperations(Metadata metadata, Hashtable? values = null)
    {
        var tester = new DataDictionaryTester(metadata, Repository, HttpContext, FormEventResolver, _loggerFactory,
            _options, _auditLogService);
        var result = tester.AllOperations(values);
        Assert.True(result.IsValid);
        return metadata;
    }

    public Metadata AssertAllOperations(Metadata metadata, Action<DataDictionaryTesterValues> configure)
    {
        var tester = new DataDictionaryTester(metadata, Repository, HttpContext, FormEventResolver, _loggerFactory,
            _options, _auditLogService);
        var result = tester.AllOperations(configure);
        Assert.True(result.IsValid);
        return metadata;
    }

    public Metadata AssertInsert(Metadata metadata, Hashtable? values = null)
    {
        var tester = new DataDictionaryTester(metadata, Repository, HttpContext, FormEventResolver, _loggerFactory,
            _options, _auditLogService);
        var result = tester.Insert(values);
        Assert.True(result.IsValid);
        return metadata;
    }

    public Metadata AssertUpdate(Metadata metadata, Hashtable values)
    {
        var tester = new DataDictionaryTester(metadata, Repository, HttpContext, FormEventResolver, _loggerFactory,
            _options, _auditLogService);
        var result = tester.Update(values);
        Assert.True(result.IsValid);
        return metadata;
    }

    public Metadata AssertRead(Metadata metadata, Hashtable? values = null)
    {
        var tester = new DataDictionaryTester(metadata, Repository, HttpContext, FormEventResolver, _loggerFactory,
            _options, _auditLogService);
        var result = tester.Read(values);
        Assert.True(result.IsValid);
        return metadata;
    }

    public Metadata AssertDelete(Metadata metadata, Hashtable values)
    {
        var tester = new DataDictionaryTester(metadata, Repository, HttpContext, FormEventResolver, _loggerFactory,
            _options, _auditLogService);
        var result = tester.Delete(values);
        Assert.True(result.IsValid);
        return metadata;
    }
}