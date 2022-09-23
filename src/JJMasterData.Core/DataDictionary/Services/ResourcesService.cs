using System.Collections.Generic;
using System.Globalization;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Services.Abstractions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.DataDictionary.Services;

public class ResourcesService : BaseService
{
    public ResourcesService(IValidationDictionary validationDictionary) : base(validationDictionary)
    {
    }

    public JJFormView GetFormView(IList<CultureInfo> supportedCultures)
    {
        supportedCultures ??= CultureInfo.GetCultures(CultureTypes.AllCultures);
            
        var element = new DbTranslatorProvider().GetElement();
        
        var formElement = new FormElement(element)
        {
            Title = "Resources",
            SubTitle = "Languages"
        };

        var formView = new JJFormView(formElement);
        
        formView.ImportAction.SetVisible(true);
        formView.ViewAction.SetVisible(false);
        formView.FilterAction.ExpandedByDefault = true;

        var cultureField = formView.FormElement.Fields["cultureCode"];
        cultureField.IsRequired = true;
        cultureField.Component = FormComponent.Search;
        cultureField.DataItem = new FormElementDataItem();
        cultureField.DataItem.Itens = new List<DataItemValue>();
        cultureField.DataItem.ReplaceTextOnGrid = false;
        foreach (CultureInfo ci in supportedCultures)
        {
            var item = new DataItemValue();
            item.Id = ci.Name;
            item.Description = ci.Name + " " + ci.DisplayName;
            cultureField.DataItem.Itens.Add(item);
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

    private void ClearCache(object sender, FormAfterActionEventArgs e) => Translate.ClearCache();

    private void ValidateEspecialChars(object sender, FormBeforeActionEventArgs e)
    {
        if (e?.Values == null)
            return;

        if (e.Values.Count == 0)
            return;

        if (e.Values["resourceKey"].ToString().Contains("'") ||
            e.Values["resourceKey"].ToString().Contains("\"") ||
            e.Values["resourceValue"].ToString().Contains("'") ||
            e.Values["resourceValue"].ToString().Contains("\""))
        {
            e.Errors.Add("Error", Translate.Key("Character {0} not allowed", "'"));
        }
    }

      
}