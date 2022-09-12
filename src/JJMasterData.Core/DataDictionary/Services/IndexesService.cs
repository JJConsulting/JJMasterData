using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class IndexesService : BaseService
{
    public IndexesService(IValidationDictionary validationDictionary) : base(validationDictionary)
    {
    }

    public bool Save(string id, string index, ElementIndex elementIndex)
    {
        var formElement = DicDao.GetFormElement(id);
        
        if (!string.IsNullOrEmpty(index))
        {
            formElement.Indexes[int.Parse(index)] = elementIndex;
        }
        else
        {
            formElement.Indexes.Add(elementIndex);
        }

        if (Validate(elementIndex))
        {
            DicDao.SetFormElement(formElement);
        }

        return IsValid;

    }

    public bool Validate(ElementIndex elementIndext)
    {
        if (elementIndext.Columns == null || elementIndext.Columns.Count == 0)
            AddError("", Translate.Key("Select a field to compose the index"));

        return IsValid;
    }
}