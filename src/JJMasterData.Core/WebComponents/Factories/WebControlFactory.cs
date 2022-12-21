using System;
using System.Collections;
using System.Linq;
using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.WebComponents.Factories;

internal class WebControlFactory
{
    public readonly EventHandler<ActionEventArgs> OnRenderAction;

    internal ActionManager ActionManager { get; }

    public string PanelName { get; private set; }

    public ExpressionOptions ExpressionOptions { get; private set; }

    public FormElement FormElement { get; set; }

    public IDataDictionaryRepository DataDictionaryRepository { get; }
    public IEntityRepository EntityRepository { get; }
    
    public WebControlTextFactory WebControlTextFactory { get; }
    
    public WebControlFactory(
        JJDataPanel dataPanel, 
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository)
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        ActionManager = new ActionManager(dataPanel.FormElement,
            new ExpressionManager(new Hashtable(), dataPanel.EntityRepository), dataDictionaryRepository,
            dataPanel.Name);
        OnRenderAction += dataPanel.OnRenderAction;
        FormElement = dataPanel.FormElement;
        PanelName = dataPanel.Name;
        WebControlTextFactory = new WebControlTextFactory();
        ExpressionOptions = new ExpressionOptions(dataPanel.UserValues, dataPanel.Values, dataPanel.PageState,
            dataPanel.EntityRepository);
    }

    public WebControlFactory(
        FormElement formElement, 
        IEntityRepository entityRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        ExpressionManager expressionManager,
        ExpressionOptions expressionOptions, string panelName)
    {
        EntityRepository = entityRepository;
        DataDictionaryRepository = dataDictionaryRepository;
        FormElement = formElement;
        ExpressionOptions = expressionOptions;
        PanelName = panelName;
        WebControlTextFactory = new WebControlTextFactory();
        ActionManager = new ActionManager(FormElement, expressionManager, dataDictionaryRepository, panelName);
    }

    public JJBaseControl CreateControl(FormElementField f, object value)
    {
        if (f == null)
            throw new ArgumentNullException(nameof(f), "FormElementField can not be null");

        JJBaseControl baseView;
        switch (f.Component)
        {
            case FormComponent.ComboBox:
                baseView = JJComboBox.GetInstance(f,EntityRepository, ExpressionOptions, value);
                break;
            case FormComponent.Search:
                baseView = JJSearchBox.GetInstance(f, ExpressionOptions, value, PanelName);
                break;
            case FormComponent.Lookup:
                baseView = JJLookup.GetInstance(f, DataDictionaryRepository, ExpressionOptions, value, PanelName);
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
                    var textFile = JJTextFile.GetInstance(FormElement, f, DataDictionaryRepository, ExpressionOptions, value, PanelName);
                    baseView = textFile;
                }

                break;
            default:
                var textGroup = WebControlTextFactory.CreateTextGroup(f, value);


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
        foreach (var action in actions)
        {
            var link = ActionManager.GetLinkField(action, ExpressionOptions.FormValues, ExpressionOptions.PageState,
                PanelName);
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