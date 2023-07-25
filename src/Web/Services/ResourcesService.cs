using System.Globalization;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Services;

public class ResourcesService : BaseService
{
    private FormViewFactory FormViewFactory { get; }
    private IMemoryCache MemoryCache { get; }
    private JJMasterDataCommonsOptions Options { get; }

    public ResourcesService(
        FormViewFactory formViewFactory,
        IValidationDictionary validationDictionary, 
        IDataDictionaryRepository dataDictionaryRepository,
        IMemoryCache memoryCache,
        IOptions<JJMasterDataCommonsOptions> options,
        IStringLocalizer<JJMasterDataResources> stringLocalizer
        )
        : base(validationDictionary, dataDictionaryRepository,stringLocalizer)
    {
        FormViewFactory = formViewFactory;
        MemoryCache = memoryCache;
        Options = options.Value;
    }

    public JJFormView GetFormView(IList<CultureInfo>? supportedCultures)
    {
        supportedCultures ??= CultureInfo.GetCultures(CultureTypes.AllCultures);
            
        var element = JJMasterDataStringLocalizerElement.GetElement(Options);
        
        var formElement = new FormElement(element)
        {
            Title = "Resources",
            SubTitle = "Languages"
        };

        var formView = FormViewFactory.CreateFormView(formElement);
        
        formView.GridView.ImportAction.SetVisible(true);
        formView.ViewAction.SetVisible(false);
        formView.GridView.FilterAction.ExpandedByDefault = true;

        var cultureField = formView.FormElement.Fields["cultureCode"];
        cultureField.IsRequired = true;
        cultureField.Component = FormComponent.ComboBox;
        cultureField.DataItem = new FormElementDataItem
        {
            Items = new List<DataItemValue>(),
            ReplaceTextOnGrid = false
        };
        foreach (var cultureInfo in supportedCultures)
        {
            var item = new DataItemValue
            {
                Id = cultureInfo.Name,
                Description = cultureInfo.Name + " " + cultureInfo.DisplayName
            };
            cultureField.DataItem.Items.Add(item);
        }
   
        formView.FormElement.Fields["resourceKey"].IsRequired = true;
        formView.FormElement.Fields["resourceOrigin"].VisibleExpression = "val:0";
        formView.FormElement.Fields["resourceOrigin"].Export = false;

        formView.OnBeforeInsert += ValidateEspecialChars;
        formView.OnBeforeUpdate += ValidateEspecialChars;
        formView.OnAfterInsert += ClearCache;
        formView.OnAfterUpdate += ClearCache;
        formView.DataImp.OnAfterProcess += ClearCache;
        formView.DataImp.OnBeforeImport += ValidateEspecialChars;

        return formView;
    }

    private void ClearCache(object? sender, FormAfterActionEventArgs e)
    {
        MemoryCache.Remove($"JJMasterData.Commons.Localization.JJMasterDataResources_localization_strings_{Thread.CurrentThread.CurrentCulture.Name}");
    }

    private static void ValidateEspecialChars(object? sender, FormBeforeActionEventArgs e)
    {
        if (e?.Values == null)
            return;

        if (e.Values.Count == 0)
            return;

        if (e.Values["resourceKey"]!.ToString()!.Contains("'") ||
            e.Values["resourceKey"]!.ToString()!.Contains("\"") ||
            e.Values["resourceValue"]!.ToString()!.Contains("'") ||
            e.Values["resourceValue"]!.ToString()!.Contains("\""))
        {
            e.Errors.Add("Error", "Character \' not allowed");
        }
    }

      
}