using System;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Exceptions;

internal sealed class FormValuesException(FormElementField field, string value, Exception innerException)
    : JJMasterDataException("Error at FormValuesService.", innerException)
{
    public FormElementField Field { get; set; } = field;
    public object Value { get; set; } = value;
}