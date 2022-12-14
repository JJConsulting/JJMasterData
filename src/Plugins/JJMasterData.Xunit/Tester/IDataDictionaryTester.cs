using System.Collections;
using System.Data;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Xunit.Tester;

internal interface IDataDictionaryTester
{
    DataDictionaryTesterResult AllOperations(Hashtable? values = null);
    FormLetter Insert(Hashtable values);
    FormLetter Update(Hashtable values);
    FormLetter<DataTable> Read(Hashtable? filters = null);
    FormLetter Delete(Hashtable values);
}