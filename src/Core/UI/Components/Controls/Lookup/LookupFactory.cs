using System;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Factories;

internal class LookupFactory : IControlFactory<JJLookup>
{
    private IHttpRequest HttpRequest { get; }
    private ILookupService LookupService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IComponentFactory<JJTextBox> TextBoxFactory { get; }

    private ILoggerFactory LoggerFactory { get; }

    public LookupFactory(       
        IHttpRequest httpRequest,
        ILookupService lookupService,
        JJMasterDataUrlHelper urlHelper,
        IComponentFactory<JJTextBox> textBoxFactory,
        ILoggerFactory loggerFactory)
    {
        HttpRequest = httpRequest;
        LookupService = lookupService;
        UrlHelper = urlHelper;
        TextBoxFactory = textBoxFactory;
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
            controlContext,
            HttpRequest,
            LookupService,
            TextBoxFactory);
       
        return lookup;
    }

    
}