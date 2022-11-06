using JJMasterData.Api.Models;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using JJMasterData.Core.DataManager;
using System.Collections;
using System.Diagnostics;
using System.Net;

namespace JJMasterData.Api.Services;

public class MasterApiService
{
    private DictionaryDao DictionaryDao { get; }

    private Factory Factory => DictionaryDao.Factory;

    private readonly HttpContext? _httpContext;

    private readonly AccountService _accountService;

    public MasterApiService(IHttpContextAccessor httpContextAccessor, AccountService accountService)
    {
        _httpContext = httpContextAccessor?.HttpContext;
        DictionaryDao = new DictionaryDao();
        _accountService = accountService;
    }

    public string GetListFieldAsText(string elementName, int pag, int regporpag, string orderby)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var dictionary = DictionaryDao.GetDictionary(elementName);
        if (!dictionary.Api.EnableGetAll)
            throw new UnauthorizedAccessException();

        var filters = GetDefaultFilter(dictionary, true);
        var element = dictionary.Table;

        var showLogInfo = Debugger.IsAttached;
        string text = Factory.GetListFieldsAsText(element, filters, orderby, regporpag, pag, showLogInfo);
        if (string.IsNullOrEmpty(text))
            throw new KeyNotFoundException(Translate.Key("No records found"));

        return text;
    }

    public MasterApiListResponse GetListFields(string elementName, int pag, int regporpag, string orderby, int total = 0)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var dictionary = DictionaryDao.GetDictionary(elementName);
        if (!dictionary.Api.EnableGetAll)
            throw new UnauthorizedAccessException();

        var filters = GetDefaultFilter(dictionary, true);
        var element = dictionary.Table;
        var dt = Factory.GetDataTable(element, filters, orderby, regporpag, pag, ref total);

        if (dt == null || dt.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("No records found"));

        var ret = new MasterApiListResponse();
        ret.Tot = total;
        ret.SetDataTableValues(dictionary, dt);

        return ret;
    }

    public Dictionary<string, object> GetFields(string elementName, string id)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var dictionary = DictionaryDao.GetDictionary(elementName);
        if (!dictionary.Api.EnableGetDetail)
            throw new UnauthorizedAccessException();

        var element = dictionary.Table;
        var pks = element.Fields.ToList().FindAll(x => x.IsPk);
        var ids = id.Split(',');

        if (ids.Length != pks.Count)
            throw new DataDictionaryException(Translate.Key("Invalid primary key"));

        var paramValues = new Hashtable();
        for (int i = 0; i < pks.Count; i++)
        {
            paramValues.Add(pks[i].Name, ids[i]);
        }

        var filters = ParseFilter(dictionary, paramValues);
        var fields = Factory.GetFields(element, filters);

        if (fields == null || fields.Count == 0)
            throw new KeyNotFoundException(Translate.Key("No records found"));

        //We transform to dictionary to preserve the order of fields in parse
        var listRet = new Dictionary<string, object>();
        foreach (ElementField field in element.Fields)
        {
            string fieldName = dictionary.Api.GetFieldNameParsed(field.Name);
            if (fields.ContainsKey(field.Name))
                listRet.Add(fieldName, fields![field.Name]!);
        }

        return listRet;
    }

    public List<ResponseLetter> SetFields(Hashtable[] listParam, string elementName, bool replace = false)
    {
        if (listParam == null)
            throw new ArgumentNullException(nameof(listParam));

        var dictionary = GetDataDictionary(elementName);
        if (!dictionary.Api.EnableAdd | !dictionary.Api.EnableUpdate)
            throw new UnauthorizedAccessException();

        var formService = GetFormService(dictionary);
        var listRet = new List<ResponseLetter>();
        foreach (Hashtable values in listParam)
        {
            ResponseLetter ret = replace ?
                InsertOrReplace(formService, values, dictionary.Api) :
                Insert(formService, values, dictionary.Api);

            listRet.Add(ret);
        }
        return listRet;
    }

    public List<ResponseLetter> UpdateFields(Hashtable[] listParam, string elementName)
    {
        if (listParam == null)
            throw new DataDictionaryException(Translate.Key("Invalid parameter or not a list"));

        var dictionary = GetDataDictionary(elementName);
        if (!dictionary.Api.EnableUpdate)
            throw new UnauthorizedAccessException();

        var formService = GetFormService(dictionary);
        var listRet = new List<ResponseLetter>();
        foreach (Hashtable values in listParam)
        {
            var ret = Update(formService, values, dictionary.Api);
            listRet.Add(ret);
        }

        return listRet;
    }

    public List<ResponseLetter> UpdatePart(Hashtable[] listParam, string elementName)
    {
        if (listParam == null)
            throw new ArgumentNullException(nameof(listParam));

        var dictionary = GetDataDictionary(elementName);
        if (!dictionary.Api.EnableUpdatePart)
            throw new UnauthorizedAccessException();

        if (listParam == null)
            throw new DataDictionaryException(Translate.Key("Invalid parameter or not a list"));

        var formService = GetFormService(dictionary);
        var listRet = new List<ResponseLetter>();
        foreach (Hashtable values in listParam)
        {
            var ret = Patch(formService, values, dictionary.Api);
            listRet.Add(ret);
        }

        return listRet;
    }

    private ResponseLetter Insert(FormService formService, Hashtable values, DicApiSettings api)
    {
        ResponseLetter ret;
        try
        {
            var formResult = formService.Insert(values);
            if (formResult.IsValid)
            {
                ret = new ResponseLetter
                {
                    Status = (int)HttpStatusCode.Created,
                    Message = Translate.Key("Record added successfully"),
                    Data = DataHelper.GetDiff(values, formResult.Values, api)
                };
            }
            else
            {
                ret = CreateErrorResponseLetter(formResult.Errors, api);
            }
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }
        return ret;
    }

    private ResponseLetter Update(FormService formService, Hashtable values, DicApiSettings api)
    {
        ResponseLetter ret;
        try
        {
            var formResult = formService.Update(values);
            if (formResult.IsValid)
            {
                if (formResult.NumberOfRowsAffected == 0)
                    throw new KeyNotFoundException(Translate.Key("No records found"));

                ret = new ResponseLetter();
                ret.Status = (int)HttpStatusCode.OK;
                ret.Message = Translate.Key("Record updated successfully");
                ret.Data = DataHelper.GetDiff(values, formResult.Values, api);
            }
            else
            {
                ret = CreateErrorResponseLetter(formResult.Errors, api);
            }
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }
        return ret;
    }

    private ResponseLetter InsertOrReplace(FormService formService, Hashtable values, DicApiSettings api)
    {
        ResponseLetter ret;
        try
        {
            var formResult = formService.InsertOrReplace(values);
            if (formResult.IsValid)
            {
                ret = new ResponseLetter();
                if (formResult.Result == CommandType.Insert)
                {
                    ret.Status = (int)HttpStatusCode.Created;
                    ret.Message = Translate.Key("Record added successfully");
                }
                else
                {
                    ret.Status = (int)HttpStatusCode.OK;
                    ret.Message = Translate.Key("Record updated successfully");
                }
                ret.Data = DataHelper.GetDiff(values, formResult.Values, api);
            }
            else
            {
                ret = CreateErrorResponseLetter(formResult.Errors, api);
            }
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }
        return ret;
    }

    private ResponseLetter Patch(FormService formService, Hashtable values, DicApiSettings api)
    {
        ResponseLetter ret;
        try
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException(Translate.Key("Invalid parameter or not found"), nameof(values));

            var formManager = formService.FormManager;
            var parsedValues = DataHelper.ParseOriginalName(formManager.FormElement, values);
            var pkValues = DataHelper.GetPkValues(formManager.FormElement, parsedValues);
            var currentValues = Factory.GetFields(formManager.FormElement, pkValues);
            if (currentValues == null)
                throw new KeyNotFoundException(Translate.Key("No records found"));

            foreach (DictionaryEntry entry in parsedValues)
            {
                if (currentValues.ContainsKey(entry.Key))
                    currentValues[entry.Key] = entry.Value;
                else
                    currentValues.Add(entry.Key, entry.Value);
            }

            ret = Update(formService, currentValues, api);
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }
        return ret;
    }

    public ResponseLetter Delete(string elementName, string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        var ids = id.Split(',');
        if (ids == null || ids.Length == 0)
            throw new ArgumentException(Translate.Key("Invalid parameter or not found"), nameof(id));

        var dictionary = GetDataDictionary(elementName);
        if (!dictionary.Api.EnableDel)
            throw new UnauthorizedAccessException();

        var formService = GetFormService(dictionary);
        var formElement = dictionary.GetFormElement();
        var listRet = new List<ResponseLetter>();
        var pks = formElement.Fields.ToList().FindAll(x => x.IsPk);

        if (ids.Length != pks.Count)
            throw new DataDictionaryException(Translate.Key("Invalid primary key"));

        var filters = new Hashtable();
        for (int i = 0; i < pks.Count; i++)
        {
            filters.Add(pks[i].Name, ids[i]);
        }

        var formResult = formService.Delete(filters);

        if (formResult.IsValid)
        {
            if (formResult.NumberOfRowsAffected == 0)
                throw new KeyNotFoundException(Translate.Key("No records found"));

            return new ResponseLetter
            {
                Status = (int)HttpStatusCode.NoContent,
                Message = Translate.Key("Record successfully deleted")
            };
        }

        return CreateErrorResponseLetter(formResult.Errors, dictionary.Api);
    }


    /// <summary>
    /// Disparado ao realizar o gatilho no formulário
    /// </summary>
    /// <param name="elementName">Nome do Dicionário</param>
    /// <param name="paramValues">Valores do campos no formulário</param>
    /// <param name="pageState">Tipo de operação</param>
    /// <param name="objname">Nome do campo que disparou o gatilho</param>
    /// <returns></returns>
    public Dictionary<string, FormValues> PostTrigger(string elementName, Hashtable paramValues, PageState pageState,
        string objname = "")
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var dictionary = DictionaryDao.GetDictionary(elementName);

        if (!dictionary.Api.EnableAdd & !dictionary.Api.EnableUpdate)
            throw new UnauthorizedAccessException();

        var element = dictionary.GetFormElement();
        var values = ParseFilter(dictionary, paramValues);
        var userValues = new Hashtable
        {
            { "objname", objname }
        };

        var formManager = new FormManager(dictionary.GetFormElement());
        formManager.UserValues = userValues;
        formManager.Factory = Factory;

        var newvalues = formManager.MergeWithExpressionValues(values, pageState, false);
        var listFormValues = new Dictionary<string, FormValues>();
        foreach (FormElementField f in element.Fields)
        {
            var formValues = new FormValues();
            formValues.Enable = formManager.Expression.GetBoolValue(f.EnableExpression, f.Name, pageState, newvalues);
            formValues.Visible = formManager.Expression.GetBoolValue(f.VisibleExpression, f.Name, pageState, newvalues);

            if (newvalues != null && newvalues.Contains(f.Name))
                formValues.Value = newvalues[f.Name];

            if (!f.Name.ToLower().Equals(objname.ToLower()))
            {
                if (f.Component == FormComponent.ComboBox || f.Component == FormComponent.Search)
                {
                    formValues.DataItems = formManager.GetDataItemValues(f.DataItem, newvalues, pageState);
                }
            }

            listFormValues.Add(f.Name.ToLower(), formValues);
        }

        return listFormValues;
    }

    /// <summary>
    /// Preserva o nome original do campo e valida se o campo existe
    /// </summary>
    private Hashtable ParseFilter(DicParser dic, Hashtable paramValues)
    {
        var filters = GetDefaultFilter(dic);
        if (paramValues == null)
            return filters;

        foreach (DictionaryEntry entry in paramValues)
        {
            var field = dic.Table.Fields[entry.Key.ToString()];
            if (!filters.ContainsKey(entry.Key.ToString()))
                filters.Add(field.Name, StringManager.ClearText(entry.Value.ToString()));
        }

        return filters;
    }

    /// <summary>
    /// Retorna o filtro com usuário logado
    /// </summary>
    /// <returns></returns>
    private Hashtable GetDefaultFilter(DicParser dic, bool loadQueryString = false)
    {
        if (_httpContext == null)
            throw new NullReferenceException(nameof(_httpContext));

        var filters = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        if (loadQueryString)
        {
            var qnvp = _httpContext.Request.Query.Keys;
            foreach (string key in qnvp)
            {
                if (!dic.Table.Fields.ContainsKey(key))
                    continue;

                string value = _httpContext.Request.Query[key];
                filters.Add(dic.Table.Fields[key].Name, StringManager.ClearText(value));
            }
        }

        if (string.IsNullOrEmpty(dic.Api.ApplyUserIdOn))
            return filters;

        string userId = GetUserId();
        if (!filters.ContainsKey(dic.Api.ApplyUserIdOn))
        {
            filters.Add(dic.Api.ApplyUserIdOn, userId);
        }
        else
        {
            if (!userId.Equals(filters![dic.Api.ApplyUserIdOn]!.ToString()))
            {
                throw new UnauthorizedAccessException(
                    Translate.Key("Access denied to change user filter on {0}", dic.Table.Name));
            }
        }

        return filters;
    }

    private string GetUserId()
    {
        var tokenInfo = _accountService.GetTokenInfo(_httpContext?.User?.Claims?.FirstOrDefault()?.Value);
        if (tokenInfo == null)
            throw new UnauthorizedAccessException("Invalid Token");

        string userId = tokenInfo.UserId;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Invalid User");

        return userId;
    }

    private FormService GetFormService(DicParser dictionary)
    {
        bool logActionIsVisible = dictionary.UIOptions.ToolBarActions.LogAction.IsVisible;
        string userId = GetUserId();

        var formElement = dictionary.GetFormElement();
        var dataContext = new DataContext(DataContextSource.Api, userId);
        var userValues = GetDefaultFilter(dictionary);

        if (!userValues.ContainsKey("USERID"))
            userValues.Add("USERID", GetUserId());

        var service = new FormService(formElement, dataContext)
        {
            FormRepository = Factory,
            UserValues = userValues,
            EnableHistoryLog = logActionIsVisible
        };
        service.AddFormEvent();

        return service;
    }

    private DicParser GetDataDictionary(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        return DictionaryDao.GetDictionary(elementName);
    }


    private ResponseLetter CreateErrorResponseLetter(Hashtable erros, DicApiSettings api)
    {
        var letter = new ResponseLetter
        {
            Status = 400,
            Message = Translate.Key("Invalid data"),
            ValidationList = new Hashtable(StringComparer.InvariantCultureIgnoreCase)
        };

        if (erros == null)
            return letter;

        foreach (DictionaryEntry entry in erros)
        {
            string fieldName = api.GetFieldNameParsed(entry.Key.ToString());
            letter.ValidationList.Add(fieldName, entry.Value);
        }

        return letter;
    }
}