using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary.Services.Abstractions;

namespace JJMasterData.Core.DataDictionary.Services;

public class PanelService : BaseService
{
    public PanelService(IValidationDictionary validationDictionary) : base(validationDictionary)
    {
    }

    public bool SavePanel(FormElement formElement, FormElementPanel panel, List<FormElementField> listSelected)
    {
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

        foreach (FormElementField f in formElement.Fields)
        {
            var selField = listSelected.Find(x => x.Name.Equals(f.Name));
            if (selField != null)
            {
                f.PanelId = panel.PanelId;
            }
            else
            {
                if (f.PanelId == panel.PanelId)
                    f.PanelId = 0;
            }
        }

        DicDao.SetFormElement(formElement);

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

    public bool DeleteField(FormElement formElement, int panelId)
    {
        for (int i = 0; i < formElement.Panels.Count; i++)
        {
            if (formElement.Panels[i].PanelId == panelId)
                formElement.Panels.Remove(formElement.Panels[i]);
        }

        foreach (FormElementField f in formElement.Fields)
        {
            if (f.PanelId == panelId)
                f.PanelId = 0;
        }

        DicDao.SetFormElement(formElement);

        return IsValid;
    }

    public bool SortPanels(string elementName, string[] orderFields)
    {
        var formElement = DicDao.GetFormElement(elementName);
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

        DicDao.SetFormElement(formElement);
        return true;

    }

}