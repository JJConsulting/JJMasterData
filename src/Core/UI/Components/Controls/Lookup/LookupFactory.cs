using System;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Factories;

internal class LookupFactory : IControlFactory<JJLookup>
{
    private IHttpContext HttpContext { get; }
    private ILookupService LookupService { get; }
    private IEncryptionService EncryptionService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private ILoggerFactory LoggerFactory { get; }

    public LookupFactory(       
        IHttpContext httpContext,
        ILookupService lookupService,
        IEncryptionService encryptionService,
        JJMasterDataUrlHelper urlHelper,
        ILoggerFactory loggerFactory)
    {
        HttpContext = httpContext;
        LookupService = lookupService;
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
        LoggerFactory = loggerFactory;
    }

    public JJLookup Create()
    {
        throw new InvalidOperationException("JJLookup must be instantiated with a FormElement and FormElementField.");
    }

    public JJLookup Create(FormElement formElement, FormElementField field, ControlContext controlContext)
    {
        var lookup = new JJLookup(
            formElement,
            field,
            HttpContext,
            LookupService,
            EncryptionService,
            UrlHelper,
            LoggerFactory.CreateLogger<JJLookup>());
        lookup.SetAttr(field.Attributes);
        lookup.Name = field.Name;
        lookup.SelectedValue = controlContext.Value?.ToString();
        lookup.Visible = true;
        lookup.FormElement = formElement;
        lookup.AutoReloadFormFields = false;
        lookup.Attributes.Add("panelName", controlContext.ParentComponentName);
        lookup.FormStateData = controlContext.FormStateData;
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