using System;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal sealed class ComboBoxFactory(
        IFormValues formValues,
        DataItemService dataItemService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    : IControlFactory<JJComboBox>
{
    public JJComboBox Create()
    {
        return new JJComboBox(
            formValues,
            dataItemService,
            stringLocalizer,
            loggerFactory.CreateLogger<JJComboBox>());
    }

    public JJComboBox Create(FormElement formElement, FormElementField field, ControlContext controlContext)
    {
        if (field.DataItem == null)
            throw new ArgumentNullException(nameof(field.DataItem));

        var comboBox = Create();
        comboBox.ConnectionId = formElement.ConnectionId;
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