using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager;
using System.Collections;
using System.Diagnostics;
using System.Net;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.WebApi.Models;
using JJMasterData.Core.FormEvents.Abstractions;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.WebApi.Services;

public class MasterApiService
{
    private readonly HttpContext? _httpContext;
    private readonly IEntityRepository _entityRepository;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;
    private readonly IFormEventResolver? _formEventResolver;
    public MasterApiService(IHttpContextAccessor httpContextAccessor, 
                            IEntityRepository entityRepository, 
                            IDataDictionaryRepository dataDictionaryRepository, IFormEventResolver? formEventResolver)
    {
        _httpContext = httpContextAccessor.HttpContext;
        _entityRepository = entityRepository;
        _dataDictionaryRepository = dataDictionaryRepository;
        _formEventResolver = formEventResolver;
    }

    public string GetListFieldAsText(string elementName, int pag, int regporpag, string? orderby)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var formElement = _dataDictionaryRepository.GetMetadata(elementName);
        if (!formElement.ApiOptions.EnableGetAll)
            throw new UnauthorizedAccessException();

        var filters = GetDefaultFilter(formElement, true);
        var showLogInfo = Debugger.IsAttached;
        string text = _entityRepository.GetListFieldsAsText(formElement, filters, orderby, regporpag, pag, showLogInfo);
        if (string.IsNullOrEmpty(text))
            throw new KeyNotFoundException(Translate.Key("No records found"));

        return text;
    }
    public MasterApiListResponse GetListFields(string elementName, int pag, int regporpag, string? orderby, int total = 0)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var dictionary = _dataDictionaryRepository.GetMetadata(elementName);
        if (!dictionary.ApiOptions.EnableGetAll)
            throw new UnauthorizedAccessException();

        var filters = GetDefaultFilter(dictionary, true);
        var element = dictionary;
        var dt = _entityRepository.GetDataTable(element, filters, orderby, regporpag, pag, ref total);

        if (dt == null || dt.Rows.Count == 0)
            throw new KeyNotFoundException(Translate.Key("No records found"));

        var ret = new MasterApiListResponse
        {
            Tot = total
        };
        ret.SetDataTableValues(dictionary, dt);

        return ret;
    }
    public Dictionary<string, object> GetFields(string elementName, string id)
    {
        var dictionary = _dataDictionaryRepository.GetMetadata(elementName);
        if (!dictionary.ApiOptions.EnableGetDetail)
            throw new UnauthorizedAccessException();

        var element = dictionary;
        var primaryKeys = DataHelper.GetPkValues(element, id, ',');
        var filters = ParseFilter(dictionary, primaryKeys);
        var fields = _entityRepository.GetFields(element, filters);

        if (fields == null || fields.Count == 0)
            throw new KeyNotFoundException(Translate.Key("No records found"));

        //We transform to dictionary to preserve the order of fields in parse
        var listRet = new Dictionary<string, object>();
        foreach (ElementField field in element.Fields)
        {
            string fieldName = dictionary.ApiOptions.GetFieldNameParsed(field.Name);
            if (fields.ContainsKey(field.Name))
                listRet.Add(fieldName, fields[field.Name]!);
        }

        return listRet;
    }
    public IEnumerable<ResponseLetter> SetFields(IEnumerable<Hashtable> paramsList, string elementName, bool replace = false)
    {
        if (paramsList == null)
            throw new ArgumentNullException(nameof(paramsList));

        var dictionary = GetDataDictionary(elementName);
        if (!dictionary.ApiOptions.EnableAdd | !dictionary.ApiOptions.EnableUpdate)
            throw new UnauthorizedAccessException();

        var formService = GetFormService(dictionary);

        foreach (var values in paramsList)
        {
            yield return replace ?
                InsertOrReplace(formService, values, dictionary.ApiOptions) :
                Insert(formService, values, dictionary.ApiOptions);
        }
    }
    public IEnumerable<ResponseLetter> UpdateFields(IEnumerable<Hashtable> paramsList, string elementName)
    {
        if (paramsList == null)
            throw new JJMasterDataException(Translate.Key("Invalid parameter or not a list"));

        var dictionary = GetDataDictionary(elementName);
        if (!dictionary.ApiOptions.EnableUpdate)
            throw new UnauthorizedAccessException();

        var formService = GetFormService(dictionary);
        
        foreach (var values in paramsList)
        {
            yield return Update(formService, values, dictionary.ApiOptions);
        }
    }
    public IEnumerable<ResponseLetter> UpdatePart(IEnumerable<Hashtable> paramsList, string elementName)
    {
        if (paramsList == null)
            throw new ArgumentNullException(nameof(paramsList));

        var dictionary = GetDataDictionary(elementName);
        if (!dictionary.ApiOptions.EnableUpdatePart)
            throw new UnauthorizedAccessException();

        if (paramsList == null)
            throw new JJMasterDataException(Translate.Key("Invalid parameter or not a list"));

        var formService = GetFormService(dictionary);

        foreach (var values in paramsList)
        {
            yield return Patch(formService, values, dictionary.ApiOptions);
        }
    }
    private ResponseLetter Insert(FormService formService, IDictionary<string,dynamic> apiValues, FormElementApiOptions metadataApiOptions)
    {
        ResponseLetter ret;
        try
        {
            var values = formService.FormFieldsService.MergeWithExpressionValues(apiValues, PageState.Insert, true);
            var formResult = formService.Insert(values);
            if (formResult.IsValid)
            {
                ret = new ResponseLetter
                {
                    Status = (int)HttpStatusCode.Created,
                    Message = Translate.Key("Record added successfully"),
                    Data = GetDiff(apiValues, values, metadataApiOptions)
                };
            }
            else
            {
                ret = CreateErrorResponseLetter(formResult.Errors, metadataApiOptions);
            }
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }
        return ret;
    }
    private ResponseLetter Update(FormService formService, IDictionary apiValues, FormElementApiOptions metadataApiOptions)
    {
        ResponseLetter ret;
        try
        {
            var values = formService.FormFieldsService.MergeWithExpressionValues(apiValues, PageState.Update, true);
            var formResult = formService.Update(values);
            if (formResult.IsValid)
            {
                if (formResult.NumberOfRowsAffected == 0)
                    throw new KeyNotFoundException(Translate.Key("No records found"));

                ret = new ResponseLetter
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = Translate.Key("Record updated successfully"),
                    Data = GetDiff(apiValues, values, metadataApiOptions)
                };
            }
            else
            {
                ret = CreateErrorResponseLetter(formResult.Errors, metadataApiOptions);
            }
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }
        return ret;
    }
    private ResponseLetter InsertOrReplace(FormService formService, Hashtable apiValues, FormElementApiOptions metadataApiOptions)
    {
        ResponseLetter ret;
        try
        {
            var values = formService.FormFieldsService.MergeWithExpressionValues(apiValues, PageState.Import, true);
            var formResult = formService.InsertOrReplace(values);
            if (formResult.IsValid)
            {
                ret = new ResponseLetter();
                if (formResult.Result == CommandOperation.Insert)
                {
                    ret.Status = (int)HttpStatusCode.Created;
                    ret.Message = Translate.Key("Record added successfully");
                }
                else
                {
                    ret.Status = (int)HttpStatusCode.OK;
                    ret.Message = Translate.Key("Record updated successfully");
                }
                ret.Data = GetDiff(apiValues, values, metadataApiOptions);
            }
            else
            {
                ret = CreateErrorResponseLetter(formResult.Errors, metadataApiOptions);
            }
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }
        return ret;
    }

    private ResponseLetter Patch(FormService formService, Hashtable values, FormElementApiOptions metadataApiOptions)
    {
        ResponseLetter ret;
        try
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException(Translate.Key("Invalid parameter or not found"), nameof(values));

            var formManager = formService.FormFieldsService;
            var parsedValues = DataHelper.ParseOriginalName(formManager.FormElement, values);
            var pkValues = DataHelper.GetPkValues(formManager.FormElement, parsedValues!);
            IDictionary currentValues = _entityRepository.GetFields(formManager.FormElement, pkValues);
            if (currentValues == null)
                throw new KeyNotFoundException(Translate.Key("No records found"));

            DataHelper.CopyIntoDictionary(ref currentValues, parsedValues, true);
            ret = Update(formService, currentValues, metadataApiOptions);
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

        var dictionary = GetDataDictionary(elementName);
        if (!dictionary.ApiOptions.EnableDel)
            throw new UnauthorizedAccessException();

        var formService = GetFormService(dictionary);
        var formElement = dictionary;
        var primaryKeys = DataHelper.GetPkValues(formElement, id, ',');
        var values = formService.FormFieldsService.MergeWithExpressionValues(primaryKeys, PageState.Delete, true);
        var formResult = formService.Delete(values);

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

        return CreateErrorResponseLetter(formResult.Errors, dictionary.ApiOptions);
    }

    /// <summary>
    /// Fired when triggering the form
    /// </summary>
    public Dictionary<string, FormValues> PostTrigger(
        string elementName, Hashtable? paramValues, PageState pageState, string objname = "")
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var dictionary = _dataDictionaryRepository.GetMetadata(elementName);

        if (!dictionary.ApiOptions.EnableAdd & !dictionary.ApiOptions.EnableUpdate)
            throw new UnauthorizedAccessException();

        var element = dictionary;
        var values = ParseFilter(dictionary, paramValues);
        var userValues = new Hashtable
        {
            { "objname", objname }
        };

        var expManager = new ExpressionManager(userValues, _entityRepository);
        var formManager = new FieldValuesService(dictionary, expManager);
        IDictionary newvalues = formManager.MergeWithExpressionValues(values, pageState, false);
        var listFormValues = new Dictionary<string, FormValues>();
        foreach (FormElementField f in element.Fields)
        {
            var formValues = new FormValues
            {
                Enable = formManager.ExpressionsService.GetBoolValue(f.EnableExpression, f.Name, pageState, newvalues),
                Visible = formManager.ExpressionsService.GetBoolValue(f.VisibleExpression, f.Name, pageState, newvalues)
            };

            if (newvalues != null && newvalues.Contains(f.Name))
                formValues.Value = newvalues[f.Name]!;

            if (!f.Name.ToLower().Equals(objname.ToLower()))
            {
                if (f.Component is FormComponent.ComboBox or FormComponent.Search)
                {
                    formValues.DataItems = formManager.GetDataItemValues(f.DataItem, newvalues, pageState);
                }
            }

            listFormValues.Add(f.Name.ToLower(), formValues);
        }

        return listFormValues;
    }

    /// <summary>
    /// Preserves the original field name and validates if the field exists
    /// </summary>
    private Hashtable ParseFilter(FormElement metadata, IDictionary? paramValues)
    {
        var filters = GetDefaultFilter(metadata);
        if (paramValues == null)
            return filters;

        foreach (DictionaryEntry entry in paramValues)
        {
            //if field not exists, generate a exception
            var field = metadata.Fields[entry.Key.ToString()];
            if (!filters.ContainsKey(entry.Key.ToString() ?? string.Empty))
                filters.Add(field.Name, StringManager.ClearText(entry.Value?.ToString()!));
        }

        return filters;
    }
    private Hashtable GetDefaultFilter(FormElement formElement, bool loadQueryString = false)
    {
        if (_httpContext == null)
            throw new NullReferenceException(nameof(_httpContext));

        var filters = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        if (loadQueryString)
        {
            var qnvp = _httpContext.Request.Query.Keys;
            foreach (string key in qnvp)
            {
                if (!formElement.Fields.Contains(key))
                    continue;

                string? value = _httpContext.Request.Query[key];
                filters.Add(formElement.Fields[key].Name, StringManager.ClearText(value));
            }
        }

        if (string.IsNullOrEmpty(formElement.ApiOptions.ApplyUserIdOn))
            return filters;

        string userId = GetUserId();
        if (!filters.ContainsKey(formElement.ApiOptions.ApplyUserIdOn))
        {
            filters.Add(formElement.ApiOptions.ApplyUserIdOn, userId);
        }
        else
        {
            if (!userId.Equals(filters[formElement.ApiOptions.ApplyUserIdOn]!.ToString()))
            {
                throw new UnauthorizedAccessException(
                    Translate.Key("Access denied to change user filter on {0}", formElement.Name));
            }
        }

        return filters;
    }
    private string GetUserId()
    {
        var tokenInfo = AccountService.GetTokenInfo(_httpContext?.User.Claims.FirstOrDefault()?.Value);
        if (tokenInfo == null)
            throw new UnauthorizedAccessException("Invalid Token");

        string? userId = tokenInfo.UserId;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Invalid User");

        return userId;
    }
    private FormService GetFormService(FormElement formElement)
    {
        bool logActionIsVisible = formElement.Options.GridToolbarActions.LogAction.IsVisible;
        string userId = GetUserId();
        var userValues = new Hashtable
        {
            { "USERID", GetUserId() }
        };
        
        var dataContext = new DataContext(DataContextSource.Api, userId);
        var formEvent = _formEventResolver?.GetFormEvent(formElement.Name);
        formEvent?.OnFormElementLoad(dataContext,new FormElementLoadEventArgs(formElement));
        
        var expManager = new ExpressionManager(userValues, _entityRepository);
        var formManager = new FieldValuesService(formElement, expManager);
        var service = new FormService(formManager, dataContext)
        {
            EnableAuditLog = logActionIsVisible
        };

        service.AddFormEvent(formEvent);
        
        return service;
    }
    private FormElement GetDataDictionary(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        return _dataDictionaryRepository.GetMetadata(elementName);
    }
    private FormElement GetFormElement(string elementName) => GetDataDictionary(elementName);
    
    /// <summary>
    /// Compares the values of the fields received with those sent to the bank, returning different records
    /// </summary>
    /// <remarks>
    /// This happens due to triggers or values
    /// returned in set methods (id autoNum) for example
    /// </remarks>
    private Hashtable? GetDiff(IDictionary<string,dynamic> original, IDictionary<string,dynamic> result, FormElementApiOptions apiOptions)
    {
        var newValues = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        foreach (var entry in result)
        {
            if (entry.Value == null)
                continue;

            string fieldName = apiOptions.GetFieldNameParsed(entry.Key);
            if (original.ContainsKey(entry.Key))
            {
                if (original[entry.Key] == null && entry.Value != null ||
                    !original[entry.Key]!.Equals(entry.Value))
                    newValues.Add(fieldName, entry.Value);
            }
            else
                newValues.Add(fieldName, entry.Value);
        }

        return newValues.Count > 0 ? newValues : null;
    }
    private ResponseLetter CreateErrorResponseLetter(IDictionary? errors, FormElementApiOptions apiOptions)
    {
        var letter = new ResponseLetter
        {
            Status = 400,
            Message = Translate.Key("Invalid data"),
            ValidationList = new Dictionary<string,dynamic>(StringComparer.InvariantCultureIgnoreCase)
        };

        if (errors == null)
            return letter;

        foreach (DictionaryEntry entry in errors)
        {
            string fieldName = apiOptions.GetFieldNameParsed(entry.Key!.ToString()!);
            letter.ValidationList.Add(fieldName, entry.Value);
        }

        return letter;
    }
}