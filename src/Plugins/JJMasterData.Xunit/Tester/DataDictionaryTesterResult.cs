using JJMasterData.Core.DataManager;

namespace JJMasterData.Xunit.Tester;

public class DataDictionaryTesterResult
{
    public Dictionary<string, DataDictionaryResult> Results { get; }
    public bool IsValid => Results.Values.All(r => r.IsValid);
    public DataDictionaryTesterResult(Dictionary<string, DataDictionaryResult> results)
    {
        Results = results;
    }
}