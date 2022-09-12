using System.Collections;
using System.Collections.Generic;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary.Action;

namespace JJMasterData.Core.DataDictionary.DictionaryDAL;

public class DicTranslate
{
    public void ApplyTranslation(DicParser dic)
    {
        TranslateTable(dic.Table);
        TranslateForm(dic.Form);
        TranslateUiOptions(dic.UIOptions);
    }

    private void TranslateTable(Element element)
    {
        foreach (var field in element.Fields)
        {
            field.Label = Translate.Key(field.Label);
        }
    }

    private void TranslateForm(DicFormParser form)
    {
        if (form == null)
            return;

        form.Title = Translate.Key(form.Title);
        form.SubTitle = Translate.Key(form.SubTitle);

        foreach (var field in form.FormFields)
        {
            field.HelpDescription = Translate.Key(field.HelpDescription);
            TranslateAttr(field.Attributes);
            TranslateActions(field.Actions.GetAll());
        }
    }

    private void TranslateUiOptions(UIOptions uiOptions)
    {
        if (uiOptions == null)
            return;

        uiOptions.Grid.EmptyDataText = Translate.Key(uiOptions.Grid.EmptyDataText);

        TranslateActions(uiOptions.GridActions.GetAll());
        TranslateActions(uiOptions.ToolBarActions.GetAll());
    }

    private void TranslateAttr(Hashtable attributes)
    {
        if (attributes == null)
            return;

        if (attributes.ContainsKey(FormElementField.PlaceholderAttribute))
        {
            attributes[FormElementField.PlaceholderAttribute] = Translate.Key(attributes[FormElementField.PlaceholderAttribute].ToString());
        }

        if (attributes.ContainsKey(FormElementField.PopUpTitleAttribute))
        {
            attributes[FormElementField.PopUpTitleAttribute] = Translate.Key(attributes[FormElementField.PopUpTitleAttribute].ToString());
        }
    }

    private void TranslateActions(List<BasicAction> listAction)
    {
        if (listAction == null)
            return;

        foreach (BasicAction action in listAction)
        {
            action.ToolTip = Translate.Key(action.ToolTip);
            action.Text = Translate.Key(action.Text);
            action.ConfirmationMessage = Translate.Key(action.ConfirmationMessage);
        }
    }

}