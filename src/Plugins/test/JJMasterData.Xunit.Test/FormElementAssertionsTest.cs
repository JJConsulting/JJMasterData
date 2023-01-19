using AutoFixture;
using JJMasterData.Xunit.Assertions;
using System.Collections;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;

namespace JJMasterData.Xunit.Test;

public class AssertTest
{
    private readonly IDataDictionaryRepository _dictionaryRepository;

    public AssertTest(IDataDictionaryRepository dictionaryRepository)
    {
        _dictionaryRepository = dictionaryRepository;
    }

    [Fact]
    public void AssertDataDictionaryWithDefaultValuesTest()
    {
        var metadata = _dictionaryRepository.GetMetadata("AssertDataDictionary");
        metadata.AssertAllOperations();
    }
    
    [Fact]
    public void AssertDataDictionaryWithConfiguredValuesTest()
    {
        var metadata = _dictionaryRepository.GetMetadata("AssertDataDictionary");
        var id = new Fixture().Create<int>();

        metadata.AssertAllOperations(options =>
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