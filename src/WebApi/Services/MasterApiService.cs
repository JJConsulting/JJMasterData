using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.DataManager;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.WebApi.Models;
using Microsoft.Extensions.Localization;
using System.Diagnostics;
using System.Net;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Data.Entity.Repository;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.WebApi.Services;

public class MasterApiService(ExpressionsService expressionsService,
    IHttpContextAccessor httpContextAccessor,
    IHttpContext httpContext,
    DataItemService dataItemService,
    FieldValuesService fieldValuesService,
    FormService formService,
    IEntityRepository entityRepository,
    IDataDictionaryRepository dataDictionaryRepository,

    IStringLocalizer<MasterDataResources> stringLocalizer)
{
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext!;

    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    public async Task<string> GetListFieldAsTextAsync(string elementName, int pag, int regporpag, string? orderby)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);
        if (!formElement.ApiOptions.EnableGetAll)
            throw new UnauthorizedAccessException();

        var filters = GetDefaultFilter(formElement, true);
        var showLogInfo = Debugger.IsAttached;
        string text = await entityRepository.GetListFieldsAsTextAsync(formElement, new EntityParameters()
        {
            Filters = filters!,
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

        var dictionary = await dataDictionaryRepository.GetFormElementAsync(elementName);
        if (!dictionary.ApiOptions.EnableGetAll)
            throw new UnauthorizedAccessException();

        var filters = GetDefaultFilter(dictionary, true);
        var element = dictionary;
        var result = await entityRepository.GetDictionaryListResultAsync(element, new EntityParameters
        {
            Filters = filters!,
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
        var formElement = await dataDictionaryRepository.GetFormElementAsync(elementName);
        if (!formElement.ApiOptions.EnableGetDetail)
            throw new UnauthorizedAccessException();
        
        var primaryKeys = DataHelper.GetPkValues(formElement, id, ',');
        var filters = ParseFilter(formElement, primaryKeys);
        var fields = await entityRepository.GetFieldsAsync(formElement, filters);

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

    public async IAsyncEnumerable<ResponseLetter> SetFieldsAsync(IEnumerable<Dictionary<string, object?>> paramsList,
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

    public async IAsyncEnumerable<ResponseLetter> UpdateFieldsAsync(IEnumerable<Dictionary<string, object?>> paramsList,
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

    public async IAsyncEnumerable<ResponseLetter> UpdatePartAsync(IEnumerable<Dictionary<string, object?>> paramsList,
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

    private async Task<ResponseLetter> Insert(FormElement formElement, Dictionary<string, object?> apiValues,
        FormElementApiOptions metadataApiOptions)
    {
        ResponseLetter ret;
        try
        {
            var values = await fieldValuesService.MergeWithExpressionValuesAsync(formElement, new FormStateData(apiValues, PageState.Insert), true);
            var formResult = await formService.InsertAsync(formElement, values, GetDataContext());
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

    private async Task<ResponseLetter> Update(FormElement formElement, Dictionary<string, object?> apiValues)
    {
        ResponseLetter ret;
        try
        {
            var values = await fieldValuesService.MergeWithExpressionValuesAsync(formElement, new FormStateData(apiValues, PageState.Update), true);
            var formResult = await formService.UpdateAsync(formElement, values, GetDataContext());
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

    private async Task<ResponseLetter> InsertOrReplace(FormElement formElement, Dictionary<string, object?> apiValues,
        FormElementApiOptions metadataApiOptions)
    {
        ResponseLetter ret;
        try
        {
            var values = await fieldValuesService.MergeWithExpressionValuesAsync(formElement,  new FormStateData(apiValues, PageState.Import), true);
            var formResult =await formService.InsertOrReplaceAsync(formElement, values, GetDataContext());
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

    private async Task<ResponseLetter> Patch(FormElement formElement, Dictionary<string, object?> values)
    {
        ResponseLetter ret;
        try
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException(@"Invalid parameter or not found", nameof(values));

            var parsedValues = DataHelper.ParseOriginalName(formElement, values);
            var pkValues = DataHelper.GetPkValues(formElement, parsedValues!);
            var currentValues = await entityRepository.GetFieldsAsync(formElement, pkValues);
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
        var values = await fieldValuesService.MergeWithExpressionValuesAsync(formElement, new FormStateData(primaryKeys!, PageState.Delete), true);
        var formResult = await formService.DeleteAsync(formElement, values, GetDataContext());

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
        string elementName, Dictionary<string, object>? paramValues, 
        PageState pageState, 
        string objname = "")
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        var dictionary = await dataDictionaryRepository.GetFormElementAsync(elementName);

        if (!dictionary.ApiOptions.EnableAdd & !dictionary.ApiOptions.EnableUpdate)
            throw new UnauthorizedAccessException();

        var values = ParseFilter(dictionary, paramValues);
        var userValues = new Dictionary<string, object?>
        {
            { "componentName", objname }
        };

        var newValues = await fieldValuesService.MergeWithExpressionValuesAsync(dictionary,  new FormStateData(values!,pageState), false);
        var formData = new FormStateData(newValues, userValues, pageState);
        var listFormValues = new Dictionary<string, FormValues>();
        foreach (var field in dictionary.Fields)
        {
            var formValues = new FormValues
            {
                Enable = expressionsService.GetBoolValue(field.EnableExpression, formData),
                Visible = expressionsService.GetBoolValue(field.VisibleExpression, formData)
            };

            if (newValues != null && newValues.TryGetValue(field.Name, out var newvalue))
                formValues.Value = newvalue!;

            if (!field.Name.Equals(objname, StringComparison.OrdinalIgnoreCase))
            {
                if (field.Component is FormComponent.ComboBox or FormComponent.Search)
                {
                    var dataQuery = new DataQuery(formData, dictionary.ConnectionId);
                    formValues.DataItems = await dataItemService.GetValuesAsync(field.DataItem!, dataQuery);
                }
            }

            listFormValues.Add(field.Name.ToLower(), formValues);
        }

        return listFormValues;
    }

    /// <summary>
    /// Preserves the original field name and validates if the field exists
    /// </summary>
    private Dictionary<string, object> ParseFilter(FormElement metadata, Dictionary<string, object>? paramValues)
    {
        var filters = GetDefaultFilter(metadata);
        if (paramValues == null)
            return filters;

        foreach (var entry in paramValues)
        {
            var field = metadata.Fields[entry.Key];
            if (!filters.ContainsKey(entry.Key))
                filters.Add(field.Name, entry.Value.ToString()!);
        }

        return filters;
    }

    private Dictionary<string, object> GetDefaultFilter(FormElement formElement, bool loadQueryString = false)
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
                filters.Add(formElement.Fields[key].Name, value!);
            }
        }

        if (string.IsNullOrEmpty(formElement.ApiOptions.ApplyUserIdOn))
            return filters;

        string userId = GetUserId();
        if (!filters.TryGetValue(formElement.ApiOptions.ApplyUserIdOn, out var filter))
        {
            filters.Add(formElement.ApiOptions.ApplyUserIdOn, userId);
        }
        else
        {
            if (!userId.Equals(filter.ToString()))
            {
                throw new UnauthorizedAccessException(
                    "Access denied to change user filter on {formElement.Name}");
            }
        }

        return filters;
    }

    private string GetUserId()
    {
        return DataHelper.GetCurrentUserId(httpContext, null)!;
    }

    private DataContext GetDataContext()
    {
        var userId = GetUserId();
        return new DataContext(httpContext.Request, DataContextSource.Api, userId);
    }

    private ValueTask<FormElement> GetDataDictionary(string elementName)
    {
        if (string.IsNullOrEmpty(elementName))
            throw new ArgumentNullException(nameof(elementName));

        return dataDictionaryRepository.GetFormElementAsync(elementName);
    }

    /// <summary>
    /// Compares the values of the fields received with those sent to the bank, returning different records
    /// </summary>
    /// <remarks>
    /// This happens due to triggers or values
    /// returned in set methods (id autoNum) for example
    /// </remarks>
    private static Dictionary<string, object?>? GetDiff(
        Dictionary<string, object?> original,
        Dictionary<string, object?> result, 
        FormElementApiOptions apiOptions)
    {
        var newValues = new Dictionary<string, object?>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var entry in result)
        {
            var entryVal = entry.Value;
            if (entryVal == null)
            {
                continue;
            }
            string fieldName = apiOptions.GetFieldNameParsed(entry.Key);
            
            if (!original.ContainsKey(entry.Key))
            {
                newValues.Add(fieldName, entryVal);
                continue;
            }

            var originalEntry = original[entry.Key];
            if (originalEntry == null)
            {
                newValues.Add(fieldName, entry.Value);
                continue;
            }

            if (!((string)originalEntry).Equals(entryVal.ToString()))
            {
                newValues.Add(fieldName, entry.Value);
            }
        }

        return newValues.Count > 0 ? newValues : null;
    }

    private ResponseLetter CreateErrorResponseLetter(Dictionary<string, string>? errors,
        FormElementApiOptions apiOptions)
    {
        var letter = new ResponseLetter
        {
            Status = 400,
            Message = StringLocalizer["Invalid data"],
            ValidationList = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
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