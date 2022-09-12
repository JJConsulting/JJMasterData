using System.Collections.Generic;
using System.Linq;
using System.Text;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.DataDictionary.Services.Abstractions;

public abstract class BaseService
{
    private readonly IValidationDictionary _validationDictionary;
    private DictionaryDao _dictionaryDao;

    public DictionaryDao DicDao => _dictionaryDao ??= new DictionaryDao();
    
    protected BaseService(IValidationDictionary validationDictionary)
    {
        _validationDictionary = validationDictionary;
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
        var dicParser = DicDao.GetDictionary(dictionaryName);
        return dicParser.GetFormElement();
    }

    public bool ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            AddError(nameof(name), Translate.Key("Required [Name] field"));
            return false;
        }

        if (name.ToLower().Equals("pag") |
            name.ToLower().Equals("regporpag") |
            name.ToLower().Equals("orderby") |
            name.ToLower().Equals("tot") |
            name.ToLower().Equals("pagestate"))
        {
            AddError(nameof(name), "[Name] field contains a reserved word used in the api");
        }

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
            foreach (var f in formElement.Fields)
                hints.AppendFormat(",'{0}'", f.Name);
        }
        hints.Append("]");
        return hints.ToString();
    }

    public Dictionary<string, string> GetElementList()
    {
        var dicElement = new Dictionary<string, string>();
        dicElement.Add(string.Empty, Translate.Key("--Select--"));

        string[] list = DicDao.GetListDictionaryName();
        foreach (string name in list)
        {
            dicElement.Add(name, name);
        }

        return dicElement;
    }

}