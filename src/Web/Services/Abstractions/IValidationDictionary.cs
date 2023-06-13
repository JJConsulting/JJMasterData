using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Services.Abstractions;

public interface IValidationDictionary
{
    public IEnumerable<string> Errors { get;}

    void AddError(string key, string errorMessage);

    bool IsValid { get; }

}