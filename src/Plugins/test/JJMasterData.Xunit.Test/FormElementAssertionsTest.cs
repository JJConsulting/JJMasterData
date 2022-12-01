using AutoFixture;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using JJMasterData.Xunit.Assertions;
using System.Collections;

namespace JJMasterData.Xunit.Test;

public class AssertTest
{
    [Fact]
    public void AssertDataDictionaryWithDefaultValuesTest()
    {
        var formElement = new DictionaryDao().GetDictionary("AssertDataDictionary").GetFormElement();

        formElement.AssertAllOperations();
    }
    
    [Fact]
    public void AssertDataDictionaryWithConfiguredValuesTest()
    {
        var formElement = new DictionaryDao().GetDictionary("AssertDataDictionary").GetFormElement();

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