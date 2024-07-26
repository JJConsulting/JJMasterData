using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal sealed class RadioButtonGroupFactory(DataItemService dataItemService, IFormValues formValues) : IControlFactory<JJRadioButtonGroup>
{
    public JJRadioButtonGroup Create()
    {
        return new JJRadioButtonGroup(dataItemService, formValues);
    }

    public JJRadioButtonGroup Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var radioButtonGroup = Create();
        radioButtonGroup.DataItem = field.DataItem!;
        radioButtonGroup.ConnectionId = formElement.ConnectionId;
        radioButtonGroup.Name = field.Name;
        radioButtonGroup.Visible = true;
        radioButtonGroup.FormStateData = context.FormStateData;
        radioButtonGroup.SelectedValue = context.Value?.ToString();
        radioButtonGroup.UserValues = context.FormStateData.UserValues;

        return radioButtonGroup;
    }
}