using AutoFixture;
using JJMasterData.Xunit.Assertions;
using System.Collections;
using JJMasterData.Core.DataDictionary.Repository;

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
        var formElement = metadata.GetFormElement();
        formElement.AssertAllOperations();
    }
    
    [Fact]
    public void AssertDataDictionaryWithConfiguredValuesTest()
    {
        var metadata = _dictionaryRepository.GetMetadata("AssertDataDictionary");
        var formElement = metadata.GetFormElement();
        var id = new Fixture().Create<int>();

        formElement.AssertAllOperations(options =>
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