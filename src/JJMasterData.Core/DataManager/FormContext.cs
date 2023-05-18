using System.Collections;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager;

internal record FormContext(IDictionary Values, IDictionary Errors, PageState PageState)
{
    public IDictionary Values { get; } = Values;
    public IDictionary Errors { get; } = Errors;
    public PageState PageState { get; } = PageState;
}