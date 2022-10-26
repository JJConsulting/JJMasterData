using System.Collections;
using System.Diagnostics;
using System.Net;
using JJMasterData.Api.Models;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.AuditLog;
using JJMasterData.Core.DataDictionary.DictionaryDAL;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Api.Services;

public class MasterApiService
{
    private DictionaryDao DictionaryDao { get; }
    private Factory Factory => DictionaryDao.Factory;

    private readonly HttpContext? _httpContext;

    private readonly AccountService _accountService;

    private readonly AuditLogService _auditLogService;

    public MasterApiService(IHttpContextAccessor httpContextAccessor, AccountService accountService)
    {
        _httpContext = httpContextAccessor?.HttpContext;
        DictionaryDao = new DictionaryDao();
        _auditLogService = new AuditLogService(AuditLogSource.Api);
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

    public MasterApiListResponse GetListFields(string elementName, int pag, int regporpag, string orderby,
        int total = 0)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var dictionary = DictionaryDao.GetDictionary(elementName);
        if (!dictionary.Api.EnableGetAll)
            throw new UnauthorizedAccessException();

        var filters = GetDefaultFilter(dictionary, true);

        var manager = new FormService(elementName);
        var result = manager.GetDataTable(filters, orderby, regporpag, pag);

        if (result == null || result.Total.Equals(0))
            throw new KeyNotFoundException(Translate.Key("No records found"));

        var ret = new MasterApiListResponse
        {
            Tot = result.Total
        };

        ret.SetDataTableValues(dictionary, result.Result);

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

        var dataDictionaryManager = new FormService(elementName);
        
        var result = dataDictionaryManager.GetHashtable(filters);

        if (result.Result == null || result.Total == 0)
            throw new KeyNotFoundException(Translate.Key("No records found"));

        //We transform to dictionary to preserve the order of fields in parse
        var listRet = new Dictionary<string, object>();
        foreach (ElementField field in element.Fields)
        {
            string fieldName = dictionary.Api.GetFieldNameParsed(field.Name);
            if (result.Result.ContainsKey(field.Name))
                listRet.Add(fieldName, result.Result![field.Name]!);
        }

        return listRet;
    }

    public List<ResponseLetter> SetFields(Hashtable[] listParam, string elementName, bool replace = false)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        if (listParam == null)
            throw new ArgumentNullException(nameof(listParam));

        var dictionary = DictionaryDao.GetDictionary(elementName);
        if (!dictionary.Api.EnableAdd)
            throw new UnauthorizedAccessException();

        var formManager = new FormManager(dictionary.GetFormElement())
        {
            UserValues = GetDefaultFilter(dictionary),
            Factory = Factory
        };

        return listParam.Select(values => replace 
            ? Set(formManager, values, dictionary.Api) 
            : Insert(formManager, values, dictionary.Api)).ToList();
    }

    public List<ResponseLetter> UpdateFields(Hashtable[] listParam, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        if (listParam == null)
            throw new ArgumentNullException(nameof(listParam));

        var dictionary = DictionaryDao.GetDictionary(elementName);

        if (!dictionary.Api.EnableUpdate)
            throw new UnauthorizedAccessException();

        if (listParam == null)
            throw new DataDictionaryException(Translate.Key("Invalid parameter or not a list"));

        var formManager = new FormManager(dictionary.GetFormElement())
        {
            UserValues = GetDefaultFilter(dictionary),
            Factory = Factory
        };

        var listRet = new List<ResponseLetter>();
        foreach (Hashtable values in listParam)
        {
            ResponseLetter ret = Update(formManager, values, dictionary.Api);
            listRet.Add(ret);
        }

        return listRet;
    }

    private ResponseLetter Insert(FormManager formManager, Hashtable values, DicApiSettings api)
    {
        var ret = new ResponseLetter();

        try
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException(Translate.Key("Invalid parameter or not found"), nameof(values));

            var parsedValues = formManager.ParseOriginalName(values);
            var newvalues = formManager.GetTriggerValues(parsedValues, PageState.Insert, true);

            var dictionary = DictionaryDao.GetDictionary(formManager.FormElement.Name);

            bool logActionIsVisible = dictionary.UIOptions.ToolBarActions.LogAction.IsVisible;

            var dictionaryManager = new FormService(dictionary.GetFormElement(),
                logActionIsVisible ? _auditLogService : null);

            var result = dictionaryManager.Insert(this, newvalues, () =>
                formManager.ValidateFields(newvalues, PageState.Insert, false)
            );

            if (result.IsValid)
            {
                ret.Status = (int)HttpStatusCode.Created;
                ret.Message = Translate.Key("Record added successfully");
                ret.Data = formManager.GetDiff(parsedValues, newvalues, api);
            }
            else
            {
                ret = CreateErrorResponseLetter(result.Errors, api);
            }
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }

        return ret;
    }

    private ResponseLetter Update(FormManager formManager, Hashtable values, DicApiSettings api)
    {
        var ret = new ResponseLetter();
        try
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException(Translate.Key("Invalid parameter or not found"), nameof(values));

            var parsedValues = formManager.ParseOriginalName(values);
            var newvalues = formManager.GetTriggerValues(parsedValues, PageState.Update, true);
            
            formManager.GetPkValues(newvalues);

            var dictionary = DictionaryDao.GetDictionary(formManager.FormElement.Name);

            bool logActionIsVisible = dictionary.UIOptions.ToolBarActions.LogAction.IsVisible;

            var dictionaryManager = new FormService(dictionary.GetFormElement(),
                logActionIsVisible ? _auditLogService : null);

            var result = dictionaryManager.Update(this, newvalues, () =>
                formManager.ValidateFields(newvalues, PageState.Update, false)
            );
            
            if (result.IsValid)
            {
                ret.Status = (int)HttpStatusCode.OK;
                ret.Message = Translate.Key("Record updated successfully");
                ret.Data = formManager.GetDiff(parsedValues, newvalues, api);
            }
            else
            {
                ret = CreateErrorResponseLetter(result.Errors, api);
            }
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }

        return ret;
    }

    public ResponseLetter Set(FormManager formManager, Hashtable values, DicApiSettings api)
    {
        var ret = new ResponseLetter();

        try
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException(Translate.Key("Invalid parameter or not found"), nameof(values));

            var parsedValues = formManager.ParseOriginalName(values);
            var newvalues = formManager.GetTriggerValues(parsedValues, PageState.Import, true);
            var erros = formManager.ValidateFields(newvalues, PageState.Import, false);

            var formEvent = FormEventManager.GetFormEvent(formManager.FormElement.Name);

            if (erros.Count == 0)
            {
                var cmd = Factory.SetValues(formManager.FormElement, newvalues);
                if (cmd == CommandType.Insert)
                {
                    ret.Status = (int)HttpStatusCode.Created;
                    ret.Message = Translate.Key("Record added successfully");
                    formEvent?.OnAfterInsert(this, new FormAfterActionEventArgs(newvalues));
                }
                else
                {
                    ret.Status = (int)HttpStatusCode.OK;
                    ret.Message = Translate.Key("Record updated successfully");
                    formEvent?.OnAfterUpdate(this, new FormAfterActionEventArgs(newvalues));
                }

                ret.Data = formManager.GetDiff(parsedValues, newvalues, api);
            }
            else
            {
                ret = CreateErrorResponseLetter(erros, api);
            }
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }

        return ret;
    }

    private ResponseLetter Patch(FormManager formManager, Hashtable values, DicApiSettings api)
    {
        var ret = new ResponseLetter();
        try
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException(Translate.Key("Invalid parameter or not found"), nameof(values));

            var parsedValues = formManager.ParseOriginalName(values);

            //Validates if the Pk were filled
            var pkValues = formManager.GetPkValues(parsedValues);

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

            var newvalues = formManager.GetTriggerValues(currentValues, PageState.Update, true);
            
            var dictionary = DictionaryDao.GetDictionary(formManager.FormElement.Name);

            bool logActionIsVisible = dictionary.UIOptions.ToolBarActions.LogAction.IsVisible;

            var dictionaryManager = new FormService(dictionary.Table.Name,
                logActionIsVisible ? _auditLogService : null);

            var result = dictionaryManager.Update(this, newvalues, () =>
                formManager.ValidateFields(newvalues, PageState.Update, false)
            );
            
            if (result.IsValid)
            {
                ret.Status = (int)HttpStatusCode.OK;
                ret.Message = Translate.Key("Record updated successfully");

                ret.Data = formManager.GetDiff(parsedValues, newvalues, api);
            }
            else
            {
                ret = CreateErrorResponseLetter(result.Errors, api);
            }
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }

        return ret;
    }

    public List<ResponseLetter> UpdatePart(Hashtable[] listParam, string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        if (listParam == null)
            throw new ArgumentNullException(nameof(listParam));

        var dictionary = DictionaryDao.GetDictionary(elementName);

        if (!dictionary.Api.EnableUpdatePart)
            throw new UnauthorizedAccessException();

        if (listParam == null)
            throw new DataDictionaryException(Translate.Key("Invalid parameter or not a list"));

        var formManager = new FormManager(dictionary.GetFormElement())
        {
            UserValues = GetDefaultFilter(dictionary),
            Factory = Factory,
        };

        return listParam.Select(values => Patch(formManager, values, dictionary.Api)).ToList();
    }

    public ResponseLetter Delete(string elementName, string id)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentException(nameof(elementName));

        if (string.IsNullOrEmpty(id))
            throw new ArgumentException(nameof(id));

        var ids = id.Split(',');
        if (ids == null || ids.Length == 0)
            throw new ArgumentException(Translate.Key("Invalid parameter or not found"), nameof(id));

        var dictionary = DictionaryDao.GetDictionary(elementName);
        bool logActionIsVisible = dictionary.UIOptions.ToolBarActions.LogAction.IsVisible;
        
        var formElement = dictionary.GetFormElement();

        var dataDictionaryManager = new FormService(formElement, logActionIsVisible ? _auditLogService : null);

        if (!dictionary.Api.EnableDel)
            throw new UnauthorizedAccessException();

        var pks = formElement.Fields.ToList().FindAll(x => x.IsPk);

        if (ids.Length != pks.Count)
            throw new DataDictionaryException(Translate.Key("Invalid primary key"));

        var filters = new Hashtable();
        for (int i = 0; i < pks.Count; i++)
        {
            filters.Add(pks[i].Name, ids[i]);
        }

        var result = dataDictionaryManager.Delete(this, filters);

        if (result.Total == 0)
            throw new KeyNotFoundException(Translate.Key("No records found"));

        return new ResponseLetter
        {
            Message = Translate.Key("Record successfully deleted"),
            Status = (int)HttpStatusCode.NoContent
        };
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

        var newvalues = formManager.GetTriggerValues(values, pageState, false);
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
        if (paramValues == null)
            return null;
        var filters = GetDefaultFilter(dic);
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

        if (!string.IsNullOrEmpty(dic.Api.ApplyUserIdOn))
        {
            var userid = _accountService.GetTokenInfo(_httpContext?.User?.Claims?.FirstOrDefault()?.Value);
            if (!filters.ContainsKey(dic.Api.ApplyUserIdOn))
            {
                filters.Add(dic.Api.ApplyUserIdOn, userid.UserId);
            }
            else
            {
                if (!filters[dic.Api.ApplyUserIdOn].ToString().Equals(userid))
                    throw new UnauthorizedAccessException(Translate.Key("Access denied to change user filter on {0}",
                        dic.Table.Name));
            }
        }

        return filters;
    }

    private ResponseLetter CreateErrorResponseLetter(Hashtable erros, DicApiSettings api)
    {
        var letter = new ResponseLetter();
        letter.Status = 400;
        letter.Message = Translate.Key("Invalid data");
        letter.ValidationList = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

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