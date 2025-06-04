using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Services;

public interface IValidationDictionary
{
    public IEnumerable<string> Errors { get;}

    void AddError(string key, string errorMessage);
    
    bool IsValid { get; }

    void RemoveError(string field);
}