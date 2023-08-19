using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager.Services;

public interface IFieldFormattingService
{
    Task<string> FormatGridValueAsync(FormElementField field, IDictionary<string, object> values,
        IDictionary<string, object> userValues);
    string FormatValue(FormElementField field, object value);
}