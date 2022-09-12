using System.Collections;
using JJMasterData.Core.DataManager;

namespace JJMasterData.Tester;

public interface IDataDictionaryTester
{
    DataDictionaryTesterResult Test();
    DataDictionaryTesterResult Test(Hashtable values);
    DataDictionaryResult Insert(Hashtable values);
    DataDictionaryResult Update(Hashtable values);
    DataDictionaryResult Read(Hashtable? filters = null);
    DataDictionaryResult Delete(Hashtable values);
}