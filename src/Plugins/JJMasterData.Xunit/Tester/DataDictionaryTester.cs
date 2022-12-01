using System.Collections;
using AutoFixture;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Xunit.Tester;

internal class DataDictionaryTester : IDataDictionaryTester
{
    private readonly DataDictionaryManager _dataDictionaryManager;
    public DataDictionaryTester(string dictionaryName)
    {
        _dataDictionaryManager = new DataDictionaryManager(dictionaryName);
    }

    public DataDictionaryTester(FormElement formElement)
    {
        _dataDictionaryManager = new DataDictionaryManager(formElement);
    }
    
    /// <summary>
    /// Test all DataDictionary operations with the given values.
    /// </summary>
    /// <param name="values"></param>
    /// <returns>A Dictionary with the name of the operation and the result.</returns>
    public DataDictionaryTesterResult AllOperations(Hashtable? values = null)
    {
        values ??= GetFixtureValues();

        var insertResult = Insert(values);
        var updateResult = Update(values);
        var readResult = Read(values);
        var deleteResult = Delete(values);

        return new DataDictionaryTesterResult(new()
        {
            { nameof(Insert), insertResult},
            { nameof(Update), updateResult},
            { nameof(Read), readResult},
            { nameof(Delete), deleteResult}
        });
    }
    
    /// <summary>
    /// Test all DataDictionary operations with the given values.
    /// </summary>
    /// <param name="configure"></param>
    /// <returns>A Dictionary with the name of the operation and the result.</returns>
    public DataDictionaryTesterResult AllOperations(Action<DataDictionaryTesterValues> configure)
    {

        var values = new DataDictionaryTesterValues();

        configure(values);
        
        var insertResult = Insert(values.InsertValues);
        var updateResult = Update(values.UpdateValues);
        var readResult = Read(values.ReadValues);
        var deleteResult = Delete(values.DeleteValues);
        
        return new DataDictionaryTesterResult(new Dictionary<string, DataDictionaryResult>
        {
            { nameof(Insert), insertResult},
            { nameof(Update), updateResult},
            { nameof(Read), readResult},
            { nameof(Delete), deleteResult}
        });
    }

    public DataDictionaryResult Insert(Hashtable? values = null)
    {
        values ??= GetFixtureValues();
        
        return _dataDictionaryManager.Insert(this, values);
    }

    public DataDictionaryResult Update(Hashtable values) => _dataDictionaryManager.Update(this, values);
    

    public DataDictionaryResult Read(Hashtable? filters = null) => _dataDictionaryManager.GetDataTable(filters);
    public DataDictionaryResult Delete(Hashtable values) => _dataDictionaryManager.Delete(this, values);
    public Hashtable GetFixtureValues()
    {
        var fixture = new Fixture();
        var example = new Hashtable();

        foreach (var field in _dataDictionaryManager.FormElement.Fields)
        {
            switch (field.DataType)
            {
                case FieldType.Date:
                case FieldType.DateTime:
                    example[field.Name] = fixture.Create<DateTime>();
                    break;
                case FieldType.Float:
                    example[field.Name] = fixture.Create<float>();
                    break;
                case FieldType.Int:
                    example[field.Name] = fixture.Create<int>();
                    break;
                case FieldType.NText:
                case FieldType.NVarchar:
                case FieldType.Text:
                case FieldType.Varchar:
                    example[field.Name] = fixture.Create<string>();
                    break;
            }
        }

        return example;
    }
}