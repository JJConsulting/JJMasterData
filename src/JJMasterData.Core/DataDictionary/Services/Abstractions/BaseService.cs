using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.DataDictionary.Services.Abstractions;

public abstract class BaseService
{
    private readonly IValidationDictionary _validationDictionary;

    public IDictionaryRepository DictionaryRepository { get; }

    protected BaseService(IValidationDictionary validationDictionary, IDictionaryRepository dictionaryRepository)
    {
        _validationDictionary = validationDictionary;
        DictionaryRepository = dictionaryRepository;
    }

    protected void AddError(string field, string message)
    {
        _validationDictionary?.AddError(field, message);
    }

    public JJValidationSummary GetValidationSummary()
    {
        return new JJValidationSummary(_validationDictionary.Errors.ToList());
    }

    public bool IsValid => _validationDictionary.IsValid;

    public FormElement GetFormElement(string dictionaryName)
    {
        var dicParser = DictionaryRepository.GetMetadata(dictionaryName);
        return dicParser.GetFormElement();
    }

    public bool ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            AddError(nameof(name), Translate.Key("Required [Name] field"));
            return false;
        }

        if (Validate.IsMasterDataKeyword(name))
            AddError(nameof(name), Translate.Key("The [Name] field contains a reserved word used in the api"));

        if (name.Contains(" "))
            AddError(nameof(name), Translate.Key("The [Name] field cannot contain blank spaces."));

        if (name.Length > 64)
            AddError(nameof(name), Translate.Key("The [Name] field cannot contain more than 64 characters."));

        string[] chars = { "&", "?", "=", ",", "'", "[", "]", "/", "\\", "+", "!", " " };
        foreach (string c in chars)
        {
            if (name.Contains(c))
                AddError(nameof(name), Translate.Key($"The [Name] field contains an invalid character({c})"));
        }

        string nameNoAccents = StringManager.NoAccents(name);
        if (!nameNoAccents.Equals(name))
            AddError(nameof(name), Translate.Key("The [Name] field cannot contain accents."));

        if (Validate.IsDatabaseKeyword(name))
            AddError(nameof(name), Translate.Key("The [Name] field contains a reserved word used in the database."));


        return _validationDictionary.IsValid;
    }

    public bool ValidateExpression(string value, params string[] args)
    {
        return args.Any(value.StartsWith);
    }

    public string GetHintDictionary(FormElement formElement)
    {
        var hints = new StringBuilder();
        hints.Append("[");
        hints.AppendFormat("'pagestate'");
        hints.AppendFormat(",'objname'");
        hints.AppendFormat(",'search_id'");
        hints.AppendFormat(",'search_text'");
        hints.AppendFormat(",'USERID'");
        if (formElement != null)
        {
            foreach (var f in formElement.FormFields)
                hints.AppendFormat(",'{0}'", f.Name);
        }
        hints.Append("]");
        return hints.ToString();
    }

    public Dictionary<string, string> GetElementList()
    {
        var dicElement = new Dictionary<string, string>();
        dicElement.Add(string.Empty, Translate.Key("--Select--"));

        IEnumerable<string> list = DictionaryRepository.GetNameList();
        foreach (string name in list)
        {
            dicElement.Add(name, name);
        }

        return dicElement;
    }

}