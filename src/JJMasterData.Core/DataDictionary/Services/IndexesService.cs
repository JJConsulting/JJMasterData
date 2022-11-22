using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class IndexesService : BaseService
{
    public IndexesService(IValidationDictionary validationDictionary, IDictionaryRepository dictionaryRepository)
        : base(validationDictionary, dictionaryRepository)
    {
    }

    public bool Save(string id, string index, ElementIndex elementIndex)
    {
        var dictionary = DictionaryRepository.GetDictionary(id);
        var formElement = dictionary.GetFormElement();
        
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
            dictionary.SetFormElement(formElement);
            DictionaryRepository.SetDictionary(dictionary);
        }

        return IsValid;

    }

    public bool Validate(ElementIndex elementIndext)
    {
        if (elementIndext.Columns == null || elementIndext.Columns.Count == 0)
            AddError("", Translate.Key("Select a field to compose the index"));

        return IsValid;
    }

    public void Delete(string dictionaryName, string index)
    {
        var dictionary = DictionaryRepository.GetDictionary(dictionaryName);
        var elementIndex = dictionary.Table.Indexes[int.Parse(index)];
        dictionary.Table.Indexes.Remove(elementIndex);
        DictionaryRepository.SetDictionary(dictionary);
    }

    public void MoveDown(string dictionaryName, string index)
    {
        var dictionary = DictionaryRepository.GetDictionary(dictionaryName);
        var indexes = dictionary.Table.Indexes;
        int indexToMoveDown = int.Parse(index);
        if (indexToMoveDown >= 0 && indexToMoveDown < indexes.Count - 1)
        {
            ElementIndex elementIndex = indexes[indexToMoveDown + 1];
            indexes[indexToMoveDown + 1] = indexes[indexToMoveDown];
            indexes[indexToMoveDown] = elementIndex;
            DictionaryRepository.SetDictionary(dictionary);
        }
    }

    public void MoveUp(string dictionaryName, string index)
    {
        var dictionary = DictionaryRepository.GetDictionary(dictionaryName);
        var indexes = dictionary.Table.Indexes;
        int indexToMoveUp = int.Parse(index);
        if (indexToMoveUp > 0)
        {
            ElementIndex elementIndex = indexes[indexToMoveUp - 1];
            indexes[indexToMoveUp - 1] = indexes[indexToMoveUp];
            indexes[indexToMoveUp] = elementIndex;
            DictionaryRepository.SetDictionary(dictionary);
        }
    }

}