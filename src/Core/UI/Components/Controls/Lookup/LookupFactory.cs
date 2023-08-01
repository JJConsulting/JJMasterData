using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Factories;

internal class LookupFactory : IControlFactory<JJLookup>
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

    public JJLookup Create()
    {
        return new JJLookup(
            HttpContext,
            LookupService,
            LoggerFactory.CreateLogger<JJLookup>());
    }

    public JJLookup Create(FormElement formElement,FormElementField field, ControlContext controlContext)
    {
        var lookup = Create();
        lookup.SetAttr(field.Attributes);
        lookup.Name = field.Name;
        lookup.SelectedValue = controlContext.Value?.ToString();
        lookup.Visible = true;
        lookup.DataItem = field.DataItem;
        lookup.AutoReloadFormFields = false;
        lookup.Attributes.Add("pnlname", controlContext.ParentName);
        lookup.FormValues = controlContext.FormStateData.FormValues;
        lookup.PageState = controlContext.FormStateData.PageState;
        lookup.UserValues = controlContext.FormStateData.UserValues;

        if (field.DataType is FieldType.Int)
        {
            lookup.OnlyNumbers = true;
            lookup.MaxLength = 11;
        }
        else
        {
            lookup.MaxLength = field.Size;
        }

        return lookup;
    }
    
}