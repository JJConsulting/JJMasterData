using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Factories;

internal class ComboBoxFactory : IControlFactory<JJComboBox>
{
    private IFormValues FormValues { get; }
    private DataItemService DataItemService { get; }
    private ExpressionsService ExpressionsService { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ILoggerFactory LoggerFactory { get; }

    public ComboBoxFactory(
        IFormValues formValues, 
        DataItemService dataItemService,
        ExpressionsService expressionsService, 
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    {
        FormValues = formValues;
        DataItemService = dataItemService;
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
    }

    public JJComboBox Create()
    {
        return new JJComboBox(
            FormValues,
            DataItemService,
            ExpressionsService,
            StringLocalizer,
            LoggerFactory.CreateLogger<JJComboBox>());
    }

    public JJComboBox Create(FormElement formElement, FormElementField field, ControlContext controlContext)
    {
        if (field.DataItem == null)
            throw new ArgumentNullException(nameof(field.DataItem));

        var comboBox = Create();
        comboBox.DataItem = field.DataItem;
        comboBox.Name = field.Name;
        comboBox.Visible = true;
        comboBox.FormStateData = controlContext.FormStateData;
        comboBox.MultiSelect = field.DataItem!.EnableMultiSelect;
        comboBox.SelectedValue = controlContext.Value?.ToString();
        comboBox.UserValues = controlContext.FormStateData.UserValues;

        return comboBox;
    }
}