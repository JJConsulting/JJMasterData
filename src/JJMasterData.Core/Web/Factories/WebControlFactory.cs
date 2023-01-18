using System;
using System.Collections;
using System.Linq;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.Web.Factories;

internal class WebControlFactory
{
    public readonly EventHandler<ActionEventArgs> OnRenderAction;

    private ActionManager _actionManager;

    internal ActionManager ActionManager
    {
        get
        {
            if (_actionManager != null) 
                return _actionManager;
            
            var expManager = new ExpressionManager(new Hashtable(), JJService.EntityRepository);
            _actionManager = new ActionManager(FormElement, expManager, PanelName);

            return _actionManager;
        }
        private set => _actionManager = value;
    }

    public string PanelName { get; private set; }

    public ExpressionOptions ExpressionOptions { get; private set; }

    public FormElement FormElement { get; set; }

    public WebControlFactory(JJDataPanel dataPanel)
    {
        OnRenderAction += dataPanel.OnRenderAction;
        FormElement = dataPanel.FormElement;
        PanelName = dataPanel.Name;
        ExpressionOptions = new ExpressionOptions(dataPanel.UserValues, dataPanel.Values, dataPanel.PageState, dataPanel.EntityRepository);
    }

    public WebControlFactory(FormElement formElement, ExpressionOptions expressionOptions, string panelName)
    {
        FormElement = formElement;
        ExpressionOptions = expressionOptions;
        PanelName = panelName;
    }

    public JJBaseControl CreateControl(FormElementField f, object value)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f), "FormElementField can not be null");

        JJBaseControl baseView;
        switch (f.Component)
        {
            case FormComponent.ComboBox:
                baseView = JJComboBox.GetInstance(f, ExpressionOptions, value);
                break;
            case FormComponent.Search:
                baseView = JJSearchBox.GetInstance(f, ExpressionOptions, value, PanelName);
                break;
            case FormComponent.Lookup:
                baseView = JJLookup.GetInstance(f, ExpressionOptions, value, PanelName);
                break;
            case FormComponent.CheckBox:
                baseView = JJCheckBox.GetInstance(f, value);

                if (ExpressionOptions.PageState != PageState.List)
                    baseView.Text = string.IsNullOrEmpty(f.Label) ? f.Name : f.Label;

                break;
            case FormComponent.TextArea:
                baseView = JJTextArea.GetInstance(f, value);
                break;
            case FormComponent.Slider:
                baseView = JJSlider.GetInstance(f, value);
                break;
            case FormComponent.File:
                if (ExpressionOptions.PageState == PageState.Filter)
                {
                    baseView = WebControlTextFactory.CreateTextGroup(f, value);
                }
                else
                {
                    var textFile = JJTextFile.GetInstance(FormElement, f, ExpressionOptions, value, PanelName);
                    baseView = textFile;
                }
                break;
            default:
                var textGroup = WebControlTextFactory.CreateTextGroup(f,  value);


                if (ExpressionOptions.PageState == PageState.Filter)
                {
                    textGroup.Actions = textGroup.Actions.Where(a => a.ShowInFilter).ToList();
                }
                else
                {
                    AddUserActions(textGroup, f);
                }

                baseView = textGroup;

                break;
        }

        baseView.ReadOnly = f.DataBehavior == FieldBehavior.ViewOnly && ExpressionOptions.PageState != PageState.Filter;


        return baseView;
    }

    private void AddUserActions(JJTextGroup textGroup, FormElementField f)
    {
        var actions = f.Actions.GetAll().FindAll(x => x.IsVisible);
        foreach (BasicAction action in actions)
        {
            var link = ActionManager.GetLinkField(action, ExpressionOptions.FormValues, ExpressionOptions.PageState, PanelName);
            var onRender = OnRenderAction;
            if (onRender != null)
            {
                var args = new ActionEventArgs(action, link, ExpressionOptions.FormValues);
                onRender.Invoke(this, args);
            }
            if (link != null)
                textGroup.Actions.Add(link);
        }
    }

}


