using System;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal class LookupFactory : IControlFactory<JJLookup>
{
    private IFormValues FormValues { get; }
    private LookupService LookupService { get; }
    private MasterDataUrlHelper UrlHelper { get; }
    private IComponentFactory ComponentFactory { get; }

    private ILoggerFactory LoggerFactory { get; }

    public LookupFactory(       
        IFormValues formValues,
        LookupService lookupService,
        MasterDataUrlHelper urlHelper,
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