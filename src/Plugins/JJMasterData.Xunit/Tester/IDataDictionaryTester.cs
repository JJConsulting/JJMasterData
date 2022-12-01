using System.Collections;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Xunit.Tester;

internal interface IDataDictionaryTester
{
    DataDictionaryTesterResult AllOperations(Hashtable? values = null);
    DataDictionaryResult Insert(Hashtable values);
    DataDictionaryResult Update(Hashtable values);
    DataDictionaryResult Read(Hashtable? filters = null);
    DataDictionaryResult Delete(Hashtable values);
}