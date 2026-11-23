using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJConsulting.Html.Bootstrap.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataDictionary.Services;

public class PanelService(IValidationDictionary validationDictionary,
        IEnumerable<IExpressionProvider> expressionProviders,
        DataDictionaryService dataDictionaryService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : DataDictionaryServiceBase(validationDictionary, dataDictionaryService,stringLocalizer)
{
    private IEnumerable<IExpressionProvider> ExpressionProviders { get; } = expressionProviders;

    public async Task SavePanelAsync(string elementName, FormElementPanel panel, string[]? selectedFields)
    {
        var formElement = await DataDictionaryService.GetFormElementAsync(elementName);
        selectedFields = selectedFields?.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() ?? [];

        if (panel.ParentId == 0)
            panel.ParentId = null;

        if (panel.ParentId.HasValue && formElement.Panels.All(x => x.PanelId != panel.ParentId.Value))
            AddError(nameof(panel.ParentId), StringLocalizer["Invalid parent panel."]);
        else if (panel.ParentId == panel.PanelId && panel.PanelId != 0)
            AddError(nameof(panel.ParentId), StringLocalizer["A panel cannot be its own parent."]);
        else if (panel.ParentId.HasValue && HasDescendant(formElement.Panels, panel.PanelId, panel.ParentId.Value))
            AddError(nameof(panel.ParentId), StringLocalizer["Panel hierarchy cannot contain recursion."]);

        if (!ValidatePanel(panel))
            return;


        var orderedFields = selectedFields
            .Select(fieldName => formElement.Fields.FirstOrDefault(field => field.Name == fieldName))
            .ToList();

        if (orderedFields.Any(field => field is null) || orderedFields.Distinct().Count() != orderedFields.Count)
        {
            AddError(nameof(selectedFields), StringLocalizer["Invalid fields selected for this panel."]);
            return;
        }

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
            if (selectedFields.Contains(field.Name))
            {
                field.PanelId = panel.PanelId;
            }
            else
            {
                if (field.PanelId == panel.PanelId)
                    field.PanelId = 0;
            }
        }

        var selectedFieldIndex = 0;
        for (var i = 0; i < formElement.Fields.Count; i++)
        {
            if (formElement.Fields[i].PanelId == panel.PanelId)
                formElement.Fields[i] = orderedFields[selectedFieldIndex++]!;
        }
        
        await DataDictionaryService.InsertOrReplaceAsync(formElement);
    }

    public async Task<bool> MovePanelAsync(string elementName, int panelId, int? parentId, int? beforePanelId)
    {
        var formElement = await DataDictionaryService.GetFormElementAsync(elementName);
        var panel = formElement.Panels.FirstOrDefault(x => x.PanelId == panelId);
        if (parentId == 0)
            parentId = null;
        var before = formElement.Panels.FirstOrDefault(x => x.PanelId == beforePanelId);
        if (panel is null || parentId == panelId ||
            (parentId.HasValue && (formElement.Panels.All(x => x.PanelId != parentId) ||
                                  HasDescendant(formElement.Panels, panelId, parentId.Value))) ||
            (beforePanelId.HasValue && (before is null || before == panel || before.ParentId != parentId)))
            return false;

        panel.ParentId = parentId;
        formElement.Panels.Remove(panel);
        if (before is null)
            formElement.Panels.Add(panel);
        else
            formElement.Panels.Insert(formElement.Panels.IndexOf(before), panel);
        await DataDictionaryService.InsertOrReplaceAsync(formElement);
        return true;
    }

    private static bool HasDescendant(IReadOnlyCollection<FormElementPanel> panels, int panelId, int candidateParentId)
    {
        var children = panels.Where(x => x.ParentId == panelId).Select(x => x.PanelId).ToHashSet();
        var visited = new HashSet<int>();
        while (children.Count > 0)
        {
            if (children.Contains(candidateParentId))
                return true;
            var next = panels.Where(x => children.Contains(x.ParentId ?? 0)).Select(x => x.PanelId).ToHashSet();
            visited.UnionWith(children);
            next.ExceptWith(visited);
            children = next;
        }
        return false;
    }
    
    public bool ValidatePanel(FormElementPanel panel)
    {
        if (panel.Layout is PanelLayout.Tab or PanelLayout.Collapse && string.IsNullOrWhiteSpace(panel.Title))
            AddError(nameof(panel.Title), StringLocalizer["Title is required for Tab or Collapse panels."]);

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
        var dictionary = await DataDictionaryService.GetFormElementAsync(elementName);

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

        foreach (var panel in dictionary.Panels.Where(x => x.ParentId == panelId))
            panel.ParentId = null;

        await DataDictionaryService.InsertOrReplaceAsync(dictionary);

        return IsValid;
    }

    public async Task<bool> SortPanelsAsync(string elementName, string[] orderFields)
    {
        var formElement =await DataDictionaryService.GetFormElementAsync(elementName);
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
        
        await DataDictionaryService.InsertOrReplaceAsync(formElement);

        return true;
    }

    public async Task<FormElementPanel> CopyPanel(string elementName, FormElementPanel panel)
    {
        var formElement = await DataDictionaryService.GetFormElementAsync(elementName);
        var newPanel = panel.DeepCopy();
        newPanel.PanelId = 1 + formElement.Panels.Max(x => x.PanelId);
        formElement.Panels.Add(newPanel);
        await DataDictionaryService.InsertOrReplaceAsync(formElement);

        return newPanel;
    }

}
