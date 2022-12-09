using JJMasterData.Core.DataManager;

namespace JJMasterData.Xunit.Tester;

public class DataDictionaryTesterResult
{
    public Dictionary<string, FormLetter> Results { get; }
    public bool IsValid => Results.Values.All(r => r.IsValid);
    public DataDictionaryTesterResult(Dictionary<string, FormLetter> results)
    {
        Results = results;
    }
}