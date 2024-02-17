#nullable enable
using JJMasterData.Commons.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.Extensions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class PanelService(IValidationDictionary validationDictionary,
        IEnumerable<IExpressionProvider> expressionProviders,
        IDataDictionaryRepository dataDictionaryRepository,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : BaseService(validationDictionary, dataDictionaryRepository,stringLocalizer)
{
    private IEnumerable<IExpressionProvider> ExpressionProviders { get; } = expressionProviders;

    public async Task SavePanelAsync(string elementName, FormElementPanel panel, string[]? selectedFields)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);

        if (selectedFields is null || !selectedFields.Any())
        {
            AddError(nameof(selectedFields), StringLocalizer["No fields selected for this panel."]);
        }

        if (!ValidatePanel(panel))
            return;

        if (panel.PanelId == 0)
        {
            if (formElement.Panels.Count == 0)
                panel.PanelId = 1;
            else
                panel.PanelId = 1 + formElement.Panels.Max(x => x.PanelId);

            formElement.Panels.Add(panel);
        }
        else
        {
            for (int i = 0; i < formElement.Panels.Count; i++)
            {
                if (formElement.Panels[i].PanelId != panel.PanelId) 
                    continue;
                
                formElement.Panels[i] = panel;
                break;
            }
        }

        foreach (var field in formElement.Fields)
        {
            if (selectedFields!.Contains(field.Name))
            {
                field.PanelId = panel.PanelId;
            }
            else
            {
                if (field.PanelId == panel.PanelId)
                    field.PanelId = 0;
            }
        }
        
        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);
    }
    
    public bool ValidatePanel(FormElementPanel panel)
    {
        if (string.IsNullOrWhiteSpace(panel.VisibleExpression))
            AddError(nameof(panel.VisibleExpression), "Required [VisibleExpression] panel");
        else if (!ValidateExpression(panel.VisibleExpression, ExpressionProviders.GetSyncProvidersPrefixes()))
            AddError(nameof(panel.VisibleExpression), "Invalid [VisibleExpression] panel");
        if (string.IsNullOrWhiteSpace(panel.EnableExpression))
            AddError(nameof(panel.EnableExpression), "Required [VisibleExpression] panel");
        else if (!ValidateExpression(panel.EnableExpression, ExpressionProviders.GetSyncProvidersPrefixes()))
            AddError(nameof(panel.EnableExpression), "Invalid [VisibleExpression] panel");

        return IsValid;
    }

    public async Task<bool> DeleteFieldAsync(string elementName, int panelId)
    {
        var dictionary = await DataDictionaryRepository.GetFormElementAsync(elementName);

        for (int i = 0; i < dictionary.Panels.Count; i++)
        {
            if (dictionary.Panels[i].PanelId == panelId)
                dictionary.Panels.Remove(dictionary.Panels[i]);
        }

        foreach (var f in dictionary.Fields)
        {
            if (f.PanelId == panelId)
                f.PanelId = 0;
        }

        await DataDictionaryRepository.InsertOrReplaceAsync(dictionary);

        return IsValid;
    }

    public async Task<bool> SortPanelsAsync(string elementName, string[] orderFields)
    {
        var formElement =await DataDictionaryRepository.GetFormElementAsync(elementName);
        var newList = new List<FormElementPanel>();
        for (int i = 0; i < orderFields.Length; i++)
        {
            int panelId = int.Parse(orderFields[i]);
            newList.Add(formElement.GetPanelById(panelId));
        }

        for (int i = 0; i < formElement.Panels.Count; i++)
        {
            formElement.Panels[i] = newList[i];
        }
        
        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);

        return true;
    }

    public async Task<FormElementPanel> CopyPanel(string elementName, FormElementPanel panel)
    {
        var formElement = await DataDictionaryRepository.GetFormElementAsync(elementName);
        var newPanel = ObjectCloner.DeepCopy(panel);
        newPanel.PanelId = 1 + formElement.Panels.Max(x => x.PanelId);
        formElement.Panels.Add(newPanel);
        await DataDictionaryRepository.InsertOrReplaceAsync(formElement);

        return newPanel;
    }

}