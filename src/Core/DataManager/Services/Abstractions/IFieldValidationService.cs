using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager;

public interface IFieldValidationService
{
    string ValidateField(FormElementField field, string fieldId, string value, bool enableErrorLink = true);
}