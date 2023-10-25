using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class IndexesService : BaseService
{
    public IndexesService(
        IValidationDictionary validationDictionary,
        IDataDictionaryRepository dataDictionaryRepository,
        IStringLocalizer<MasterDataResources> stringLocalizer)
        : base(validationDictionary, dataDictionaryRepository,stringLocalizer)
    {
    }

    public async Task<bool> SaveAsync(string id, string index, ElementIndex elementIndex)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(id);

        
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
            await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
        }

        return IsValid;

    }

    public bool Validate(ElementIndex elementIndext)
    {
        if (elementIndext.Columns == null || elementIndext.Columns.Count == 0)
            AddError("", StringLocalizer["Select a field to compose the index"]);

        return IsValid;
    }

    public async Task DeleteAsync(string elementName, string index)
    {
        var dictionary = await DataDictionaryRepository.GetFormElementAsync(elementName);
        var elementIndex = dictionary.Indexes[int.Parse(index)];
        dictionary.Indexes.Remove(elementIndex);
        await DataDictionaryRepository.InsertOrReplaceAsync(dictionary);
    }

    public async Task MoveDownAsync(string elementName, string index)
    {
        var dictionary = await DataDictionaryRepository.GetFormElementAsync(elementName);
        var indexes = dictionary.Indexes;
        int indexToMoveDown = int.Parse(index);
        if (indexToMoveDown >= 0 && indexToMoveDown < indexes.Count - 1)
        {
            (indexes[indexToMoveDown + 1], indexes[indexToMoveDown]) = (indexes[indexToMoveDown], indexes[indexToMoveDown + 1]);
            await DataDictionaryRepository.InsertOrReplaceAsync(dictionary);
        }
    }

    public async Task MoveUpAsync(string elementName, string index)
    {
        var dictionary = await DataDictionaryRepository.GetFormElementAsync(elementName);
        var indexes = dictionary.Indexes;
        int indexToMoveUp = int.Parse(index);
        if (indexToMoveUp > 0)
        {
            (indexes[indexToMoveUp - 1], indexes[indexToMoveUp]) = (indexes[indexToMoveUp], indexes[indexToMoveUp - 1]);
            await DataDictionaryRepository.InsertOrReplaceAsync(dictionary);
        }
    }

}