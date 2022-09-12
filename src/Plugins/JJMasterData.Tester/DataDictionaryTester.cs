using System.Collections;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Tester;

public class DataDictionaryTester : IDataDictionaryTester
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
    /// Run all DataDictionary operations with default values. Only use this if you have a simple dictionary
    /// without foreign keys.
    /// </summary>
    /// <returns>A Dictionary with the name of the operation and the result.</returns>
    public DataDictionaryTesterResult Test() => Test(GetExample());
    
    /// <summary>
    /// Test all DataDictionary operations with the given values.
    /// </summary>
    /// <param name="values"></param>
    /// <returns>A Dictionary with the name of the operation and the result.</returns>
    public DataDictionaryTesterResult Test(Hashtable values)
    {
        var insertResult = Insert(values);
        var updateResult = Update(values);
        var readResult = Read();
        var deleteResult = Delete(values);
        
        return new DataDictionaryTesterResult(new ()
        {
            { nameof(Insert), insertResult},
            { nameof(Update), updateResult},
            { nameof(Read), readResult},
            { nameof(Delete), deleteResult}
        });
    }

    public DataDictionaryResult Insert(Hashtable values) => _dataDictionaryManager.Insert(this, values);
    public DataDictionaryResult Update(Hashtable values) => _dataDictionaryManager.Update(this, values);
    public DataDictionaryResult Read(Hashtable? filters = null) => _dataDictionaryManager.GetDataTable(filters);
    public DataDictionaryResult Delete(Hashtable values) => _dataDictionaryManager.Delete(this, values);
    private Hashtable GetExample()
    {
        var example = new Hashtable();

        foreach (var field in _dataDictionaryManager.FormElement.Fields)
        {
            switch (field.DataType)
            {
                case FieldType.Date:
                case FieldType.DateTime:
                    example[field.Name] = DateTime.Now;
                    break;
                case FieldType.Float:
                    example[field.Name] = 1.0;
                    break;
                case FieldType.Int:
                    example[field.Name] = 1;
                    break;
                case FieldType.NText:
                case FieldType.NVarchar:
                case FieldType.Text:
                case FieldType.Varchar:
                    example[field.Name] = "string";
                    break;
            }
        }

        return example;
    }
}