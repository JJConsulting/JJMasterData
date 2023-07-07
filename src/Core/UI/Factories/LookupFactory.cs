using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Factories;

public class LookupFactory
{
    private IHttpContext HttpContext { get; }
    private ILookupService LookupService { get; }
    private ILoggerFactory LoggerFactory { get; }

    public LookupFactory(       
        IHttpContext httpContext,
        ILookupService lookupService,
        ILoggerFactory loggerFactory)
    {
        HttpContext = httpContext;
        LookupService = lookupService;
        LoggerFactory = loggerFactory;
    }

    internal JJLookup CreateLookup(FormElementField field, ExpressionOptions expOptions, object value, string panelName)
    {
        var search = new JJLookup(
            HttpContext,
            LookupService,
            LoggerFactory.CreateLogger<JJLookup>());
        search.SetAttr(field.Attributes);
        search.Name = field.Name;
        search.SelectedValue = value?.ToString();
        search.Visible = true;
        search.DataItem = field.DataItem;
        search.AutoReloadFormFields = false;
        search.Attributes.Add("pnlname", panelName);
        search.FormValues = expOptions.FormValues;
        search.PageState = expOptions.PageState;
        search.UserValues = expOptions.UserValues;

        if (field.DataType is FieldType.Int)
        {
            search.OnlyNumbers = true;
            search.MaxLength = 11;
        }
        else
        {
            search.MaxLength = field.Size;
        }

        return search;
    }
}