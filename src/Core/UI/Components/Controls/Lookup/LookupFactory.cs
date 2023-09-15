using System;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Factories;

internal class LookupFactory : IControlFactory<JJLookup>
{
    private IFormValues FormValues { get; }
    private ILookupService LookupService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IComponentFactory ComponentFactory { get; }

    private ILoggerFactory LoggerFactory { get; }

    public LookupFactory(       
        IFormValues formValues,
        ILookupService lookupService,
        JJMasterDataUrlHelper urlHelper,
        IComponentFactory componentFactory,
        ILoggerFactory loggerFactory)
    {
        FormValues = formValues;
        LookupService = lookupService;
        UrlHelper = urlHelper;
        ComponentFactory = componentFactory;
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
            FormValues,
            LookupService,
            ComponentFactory);
       
        return lookup;
    }

    
}