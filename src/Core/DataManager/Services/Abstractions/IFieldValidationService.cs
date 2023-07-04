using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager;

public interface IFieldValidationService
{
    string ValidateField(FormElementField field, string objname, string value, bool enableErrorLink = true);
}