using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager.Services;

public interface IFieldFormattingService
{
    Task<string> FormatGridValueAsync(FormElementField field, IDictionary<string, dynamic> values,
        IDictionary<string, dynamic> userValues);
    string FormatValue(FormElementField field, object value);
}