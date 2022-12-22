using AutoFixture;
using System.Collections;
using JJMasterData.Commons.Dao;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.Options;
using JJMasterData.Xunit.Validators;
using Microsoft.Extensions.Options;

namespace JJMasterData.Xunit.Test;

public class AssertTest
{
    private readonly IDataDictionaryRepository _dictionaryRepository;

    private readonly MetadataValidator _validator;

    public AssertTest(IDataDictionaryRepository dictionaryRepository, IEntityRepository entityRepository,
        IFormEventResolver formEventResolver, IOptions<JJMasterDataCoreOptions> options)
    {
        _dictionaryRepository = dictionaryRepository;
        _validator = new MetadataValidator(entityRepository, formEventResolver, options);
    }

    [Fact]
    public void AssertDataDictionaryWithDefaultValuesTest()
    {
        var metadata = _dictionaryRepository.GetMetadata("AssertDataDictionary");
        _validator.AssertAllOperations(metadata);
    }

    [Fact]
    public void AssertDataDictionaryWithConfiguredValuesTest()
    {
        var metadata = _dictionaryRepository.GetMetadata("AssertDataDictionary");
        var id = new Fixture().Create<int>();

        _validator.AssertAllOperations(metadata, options =>
        {
            options.InsertValues = new Hashtable
            {
                ["Id"] = id,
                ["Name"] = "Gustavo",
                ["Count"] = 21,
                ["BirthDate"] = DateTime.Now,
            };
            options.UpdateValues = new Hashtable
            {
                ["Id"] = id,
                ["Name"] = "Gustavo 2",
                ["Count"] = 22,
                ["BirthDate"] = DateTime.Now.AddDays(1),
            };
            options.DeleteValues = new Hashtable
            {
                ["Id"] = id
            };
        });
    }
}