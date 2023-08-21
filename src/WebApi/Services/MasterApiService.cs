using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.WebApi.Models;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System.Collections;
using System.Diagnostics;
using System.Net;

namespace JJMasterData.WebApi.Services;

public class MasterApiService
{
    private readonly HttpContext _httpContext;
    private AccountService AccountService { get; }
    private IExpressionsService ExpressionsService { get; }
    private IHttpContext HttpContext { get; }
    private IDataItemService DataItemService { get; }
    private IFormService FormService { get; }
    private IFieldsService FieldsService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private readonly IEntityRepository _entityRepository;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;

    public MasterApiService(
        AccountService accountService,
        IExpressionsService expressionsService,
        IHttpContextAccessor httpContextAccessor,
        IHttpContext httpContext,
        IDataItemService dataItemService,
        IFormService formService,
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IFieldsService fieldsService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer
        )
    {
        _httpContext = httpContextAccessor.HttpContext!;
        AccountService = accountService;
        ExpressionsService = expressionsService;
        HttpContext = httpContext;
        DataItemService = dataItemService;
        FormService = formService;
        FieldsService = fieldsService;
        StringLocalizer = stringLocalizer;
        _entityRepository = entityRepository;
        _dataDictionaryRepository = dataDictionaryRepository;
    }

    public async Task<string> GetListFieldAsTextAsync(string elementName, int pag, int regporpag, string? orderby)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var formElement = await _dataDictionaryRepository.GetMetadataAsync(elementName);
        if (!formElement.ApiOptions.EnableGetAll)
            throw new UnauthorizedAccessException();

        var filters = GetDefaultFilter(formElement, true);
        var showLogInfo = Debugger.IsAttached;
        string text = await _entityRepository.GetListFieldsAsTextAsync(formElement, new EntityParameters()
        {
            Parameters = filters,
            CurrentPage = pag,
            RecordsPerPage = regporpag,
            OrderBy = OrderByData.FromString(orderby)
        }, showLogInfo);
        if (string.IsNullOrEmpty(text))
            throw new KeyNotFoundException("No records found");

        return text;
    }

    public async Task<MasterApiListResponse> GetListFieldsAsync(string elementName, int pag, int regporpag, string? orderby,
        int total = 0)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var dictionary = await _dataDictionaryRepository.GetMetadataAsync(elementName);
        if (!dictionary.ApiOptions.EnableGetAll)
            throw new UnauthorizedAccessException();

        var filters = GetDefaultFilter(dictionary, true);
        var element = dictionary;
        var result = await _entityRepository.GetDictionaryListAsync(element, new EntityParameters
        {
            Parameters = filters,
            CurrentPage = pag,
            RecordsPerPage = regporpag,
            OrderBy = OrderByData.FromString(orderby)
        });

        if (result == null || result.Data.Count == 0)
            throw new KeyNotFoundException("No records found");

        var ret = new MasterApiListResponse
        {
            TotalOfRecords = total
        };
        ret.SetData(dictionary, result.Data);

        return ret;
    }

    public async Task<Dictionary<string, object>> GetFieldsAsync(string elementName, string id)
    {
        var formElement = await _dataDictionaryRepository.GetMetadataAsync(elementName);
        if (!formElement.ApiOptions.EnableGetDetail)
            throw new UnauthorizedAccessException();
        
        var primaryKeys = DataHelper.GetPkValues(formElement, id, ',');
        var filters = ParseFilter(formElement, primaryKeys);
        var fields = await _entityRepository.GetDictionaryAsync(formElement, filters);

        if (fields == null || fields.Count == 0)
            throw new KeyNotFoundException("No records found");

        //We transform to dictionary to preserve the order of fields in parse
        var listRet = new Dictionary<string, object>();
        foreach (var field in formElement.Fields)
        {
            string fieldName = formElement.ApiOptions.GetFieldNameParsed(field.Name);
            if (fields.TryGetValue(field.Name, out var field1))
                listRet.Add(fieldName, field1!);
        }

        return listRet;
    }

    public async IAsyncEnumerable<ResponseLetter> SetFieldsAsync(IEnumerable<IDictionary<string, object>> paramsList,
        string elementName, bool replace = false)
    {
        if (paramsList == null)
            throw new ArgumentNullException(nameof(paramsList));

        var formElement = await GetDataDictionary(elementName);
        if (!formElement.ApiOptions.EnableAdd | !formElement.ApiOptions.EnableUpdate)
            throw new UnauthorizedAccessException();

        foreach (var values in paramsList)
        {
            yield return replace
                ? await InsertOrReplace(formElement, values, formElement.ApiOptions)
                : await Insert(formElement, values, formElement.ApiOptions);
        }
    }

    public async IAsyncEnumerable<ResponseLetter> UpdateFieldsAsync(IEnumerable<IDictionary<string, object>> paramsList,
        string elementName)
    {
        if (paramsList == null)
            throw new JJMasterDataException("Invalid parameter or not a list");

        var dictionary = await GetDataDictionary(elementName);
        if (!dictionary.ApiOptions.EnableUpdate)
            throw new UnauthorizedAccessException();

        foreach (var values in paramsList)
        {
            yield return await Update(dictionary, values);
        }
    }

    public async IAsyncEnumerable<ResponseLetter> UpdatePartAsync(IEnumerable<IDictionary<string, object>> paramsList,
        string elementName)
    {
        if (paramsList == null)
            throw new ArgumentNullException(nameof(paramsList));

        var formElement = await GetDataDictionary(elementName);
        if (!formElement.ApiOptions.EnableUpdatePart)
            throw new UnauthorizedAccessException();

        if (paramsList == null)
            throw new JJMasterDataException("Invalid parameter or not a list");


        foreach (var values in paramsList)
        {
            yield return await Patch(formElement, values);
        }
    }

    private async Task<ResponseLetter> Insert(FormElement formElement, IDictionary<string, object> apiValues,
        FormElementApiOptions metadataApiOptions)
    {
        ResponseLetter ret;
        try
        {
            var values = await FieldsService.MergeWithExpressionValuesAsync(formElement, apiValues, PageState.Insert, true);
            var formResult = await FormService.InsertAsync(formElement, values, GetDataContext());
            if (formResult.IsValid)
            {
                ret = new ResponseLetter
                {
                    Status = (int)HttpStatusCode.Created,
                    Message = StringLocalizer["Record added successfully"],
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

    private async Task<ResponseLetter> Update(FormElement formElement, IDictionary<string, object> apiValues)
    {
        ResponseLetter ret;
        try
        {
            var values = await FieldsService.MergeWithExpressionValuesAsync(formElement, apiValues, PageState.Update, true);
            var formResult = await FormService.UpdateAsync(formElement, values, GetDataContext());
            if (formResult.IsValid)
            {
                if (formResult.NumberOfRowsAffected == 0)
                    throw new KeyNotFoundException("No records found");

                ret = new ResponseLetter
                {
                    Status = (int)HttpStatusCode.OK,
                    Message = StringLocalizer["Record updated successfully"],
                    Data = GetDiff(apiValues, values, formElement.ApiOptions)
                };
            }
            else
            {
                ret = CreateErrorResponseLetter(formResult.Errors, formElement.ApiOptions);
            }
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }

        return ret;
    }

    private async Task<ResponseLetter> InsertOrReplace(FormElement formElement, IDictionary<string, object> apiValues,
        FormElementApiOptions metadataApiOptions)
    {
        ResponseLetter ret;
        try
        {
            var values = await FieldsService.MergeWithExpressionValuesAsync(formElement, apiValues, PageState.Import, true);
            var formResult =await FormService.InsertOrReplaceAsync(formElement, values, GetDataContext());
            if (formResult.IsValid)
            {
                ret = new ResponseLetter();
                if (formResult.Result == CommandOperation.Insert)
                {
                    ret.Status = (int)HttpStatusCode.Created;
                    ret.Message = StringLocalizer["Record added successfully"];
                }
                else
                {
                    ret.Status = (int)HttpStatusCode.OK;
                    ret.Message = StringLocalizer["Record updated successfully"];
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

    private async Task<ResponseLetter> Patch(FormElement formElement, IDictionary<string, object> values)
    {
        ResponseLetter ret;
        try
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException(@"Invalid parameter or not found", nameof(values));

            var parsedValues = DataHelper.ParseOriginalName(formElement, values);
            var pkValues = DataHelper.GetPkValues(formElement, parsedValues!);
            var currentValues = await _entityRepository.GetDictionaryAsync(formElement, pkValues);
            if (currentValues == null)
                throw new KeyNotFoundException("No records found");

            DataHelper.CopyIntoDictionary( currentValues, parsedValues, true);
            ret = await Update(formElement, currentValues);
        }
        catch (Exception ex)
        {
            ret = ExceptionManager.GetResponse(ex);
        }

        return ret;
    }

    public async Task<ResponseLetter> DeleteAsync(string elementName, string id)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        var dictionary = await GetDataDictionary(elementName);
        if (!dictionary.ApiOptions.EnableDel)
            throw new UnauthorizedAccessException();

        var formElement = dictionary;
        var primaryKeys = DataHelper.GetPkValues(formElement, id, ',');
        var values = await FieldsService.MergeWithExpressionValuesAsync(formElement, primaryKeys, PageState.Delete, true);
        var formResult = await FormService.DeleteAsync(formElement, values, GetDataContext());

        if (formResult.IsValid)
        {
            if (formResult.NumberOfRowsAffected == 0)
                throw new KeyNotFoundException("No records found");

            return new ResponseLetter
            {
                Status = (int)HttpStatusCode.NoContent,
                Message = StringLocalizer["Record successfully deleted"]
            };
        }

        return CreateErrorResponseLetter(formResult.Errors, dictionary.ApiOptions);
    }

    /// <summary>
    /// Fired when triggering the form
    /// </summary>
    public async Task<Dictionary<string, FormValues>> PostTriggerAsync(
        string elementName, IDictionary<string, object>? paramValues, 
        PageState pageState, 
        string objname = "")
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var dictionary = await _dataDictionaryRepository.GetMetadataAsync(elementName);

        if (!dictionary.ApiOptions.EnableAdd & !dictionary.ApiOptions.EnableUpdate)
            throw new UnauthorizedAccessException();

        var values = ParseFilter(dictionary, paramValues);
        var userValues = new Dictionary<string,object>
        {
            { "objname", objname }
        };

        var newValues = await FieldsService.MergeWithExpressionValuesAsync(dictionary, values, pageState, false);
        var formData = new FormStateData(newValues, userValues, pageState);
        var listFormValues = new Dictionary<string, FormValues>();
        foreach (var field in dictionary.Fields)
        {
            var formValues = new FormValues
            {
                Enable = await ExpressionsService.GetBoolValueAsync(field.EnableExpression, formData),
                Visible = await ExpressionsService.GetBoolValueAsync(field.VisibleExpression, formData)
            };

            if (newValues != null && newValues.TryGetValue(field.Name, out var newvalue))
                formValues.Value = newvalue!;

            if (!field.Name.ToLower().Equals(objname.ToLower()))
            {
                if (field.Component is FormComponent.ComboBox or FormComponent.Search)
                {
                    formValues.DataItems = await DataItemService
                        .GetValuesAsync(field.DataItem, formData, null,null )
                        .ToListAsync();
                }
            }

            listFormValues.Add(field.Name.ToLower(), formValues);
        }

        return listFormValues;
    }

    /// <summary>
    /// Preserves the original field name and validates if the field exists
    /// </summary>
    private IDictionary<string, object> ParseFilter(FormElement metadata, IDictionary<string, object>? paramValues)
    {
        var filters = GetDefaultFilter(metadata);
        if (paramValues == null)
            return filters;

        foreach (var entry in paramValues)
        {
            var field = metadata.Fields[entry.Key];
            if (!filters.ContainsKey(entry.Key))
                filters.Add(field.Name, StringManager.ClearText(entry.Value.ToString()!));
        }

        return filters;
    }

    private IDictionary<string, object> GetDefaultFilter(FormElement formElement, bool loadQueryString = false)
    {
        if (_httpContext == null)
            throw new NullReferenceException(nameof(_httpContext));

        var filters = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
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
            if (!userId.Equals(filters[formElement.ApiOptions.ApplyUserIdOn].ToString()))
            {
                throw new UnauthorizedAccessException(
                    "Access denied to change user filter on {formElement.Name}");
            }
        }

        return filters;
    }

    private string GetUserId()
    {
        var tokenInfo = AccountService.GetTokenInfo(_httpContext.User.Claims.FirstOrDefault()?.Value);
        if (tokenInfo == null)
            throw new UnauthorizedAccessException("Invalid Token");

        string? userId = tokenInfo.UserId;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Invalid User");

        return userId;
    }


    private DataContext GetDataContext()
    {
        var userId = GetUserId();
        return new DataContext(HttpContext, DataContextSource.Api, userId);
    }

    private async Task<FormElement> GetDataDictionary(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        return await _dataDictionaryRepository.GetMetadataAsync(elementName);
    }

    /// <summary>
    /// Compares the values of the fields received with those sent to the bank, returning different records
    /// </summary>
    /// <remarks>
    /// This happens due to triggers or values
    /// returned in set methods (id autoNum) for example
    /// </remarks>
    private IDictionary<string, object>? GetDiff(IDictionary<string, object> original,
        IDictionary<string, object> result, FormElementApiOptions apiOptions)
    {
        var newValues = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var entry in result)
        {
            if (entry.Value == null)
                continue;

            string fieldName = apiOptions.GetFieldNameParsed(entry.Key);
            if (original.ContainsKey(entry.Key))
            {
                if (original[entry.Key] == null && entry.Value != null ||
                    !original[entry.Key].Equals(entry.Value))
                    newValues.Add(fieldName, entry.Value);
            }
            else
                newValues.Add(fieldName, entry.Value);
        }

        return newValues.Count > 0 ? newValues : null;
    }

    private ResponseLetter CreateErrorResponseLetter(IDictionary<string, object>? errors,
        FormElementApiOptions apiOptions)
    {
        var letter = new ResponseLetter
        {
            Status = 400,
            Message = StringLocalizer["Invalid data"],
            ValidationList = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase)
        };

        if (errors == null)
            return letter;

        foreach (var entry in errors)
        {
            string fieldName = apiOptions.GetFieldNameParsed(entry.Key);
            letter.ValidationList.Add(fieldName, entry.Value);
        }

        return letter;
    }
}