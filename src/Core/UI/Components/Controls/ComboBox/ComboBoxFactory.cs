using System;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal class ComboBoxFactory(IFormValues formValues,
        DataItemService dataItemService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    : IControlFactory<JJComboBox>
{
    private IFormValues FormValues { get; } = formValues;
    private DataItemService DataItemService { get; } = dataItemService;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private ILoggerFactory LoggerFactory { get; } = loggerFactory;

    public JJComboBox Create()
    {
        return new JJComboBox(
            FormValues,
            DataItemService,
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