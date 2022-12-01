using JJMasterData.Commons.Extensions;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataDictionary.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.DataDictionary.Services;

public class PanelService : BaseService
{
    public PanelService(IValidationDictionary validationDictionary, IDictionaryRepository dictionaryRepository)
        : base(validationDictionary, dictionaryRepository)
    {
    }

    public bool SavePanel(string dictionaryName, FormElementPanel panel, string[] selectedFields)
    {
        var dictionary = DictionaryRepository.GetMetadata(dictionaryName);
        var formElement = dictionary.GetFormElement();

        if (!ValidatePanel(panel))
            return false;

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
                if (formElement.Panels[i].PanelId == panel.PanelId)
                {
                    formElement.Panels[i] = panel;
                    break;
                }
            }
        }

        foreach (FormElementField f in formElement.FormFields)
        {
            if (selectedFields.Contains(f.Name))
            {
                f.PanelId = panel.PanelId;
            }
            else
            {
                if (f.PanelId == panel.PanelId)
                    f.PanelId = 0;
            }
        }

        dictionary.SetFormElement(formElement);
        DictionaryRepository.InsertOrReplace(dictionary);

        return IsValid;
    }

    private bool ValidatePanel(FormElementPanel panel)
    {
        if (string.IsNullOrWhiteSpace(panel.VisibleExpression))
            AddError(nameof(panel.VisibleExpression), "Required [VisibleExpression] panel");
        else if (!ValidateExpression(panel.VisibleExpression, "val:", "exp:"))
            AddError(nameof(panel.VisibleExpression), "Invalid [VisibleExpression] panel");

        if (string.IsNullOrWhiteSpace(panel.EnableExpression))
            AddError(nameof(panel.EnableExpression), "Required [VisibleExpression] panel");
        else if (!ValidateExpression(panel.EnableExpression, "val:", "exp:"))
            AddError(nameof(panel.EnableExpression), "Invalid [VisibleExpression] panel");

        return IsValid;
    }

    public bool DeleteField(string dictionaryName, int panelId)
    {
        var dictionary = DictionaryRepository.GetMetadata(dictionaryName);

        for (int i = 0; i < dictionary.Form.Panels.Count; i++)
        {
            if (dictionary.Form.Panels[i].PanelId == panelId)
                dictionary.Form.Panels.Remove(dictionary.Form.Panels[i]);
        }

        foreach (var f in dictionary.Form.FormFields)
        {
            if (f.PanelId == panelId)
                f.PanelId = 0;
        }

        DictionaryRepository.InsertOrReplace(dictionary);

        return IsValid;
    }

    public bool SortPanels(string elementName, string[] orderFields)
    {
        var dictionary = DictionaryRepository.GetMetadata(elementName);
        var formElement = dictionary.GetFormElement();
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

        dictionary.SetFormElement(formElement);
        DictionaryRepository.InsertOrReplace(dictionary);

        return true;
    }

    public FormElementPanel CopyPanel(string dictionaryName, FormElementPanel panel)
    {
        var dictionary = DictionaryRepository.GetMetadata(dictionaryName);
        var newPanel = panel.DeepCopy();
        newPanel.PanelId = 1 + dictionary.Form.Panels.Max(x => x.PanelId);
        dictionary.Form.Panels.Add(newPanel);
        DictionaryRepository.InsertOrReplace(dictionary);

        return newPanel;
    }

}