#nullable disable warnings
using System;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Components;

internal sealed class ComboBoxFactory(
        IHttpContextAccessor formValues,
        DataItemService dataItemService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : IControlFactory<JJComboBox>
{
    public JJComboBox Create()
    {
        return new JJComboBox(
            formValues,
            dataItemService,
            stringLocalizer);
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