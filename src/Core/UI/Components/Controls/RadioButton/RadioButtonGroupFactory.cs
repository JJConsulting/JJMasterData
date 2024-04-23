using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal class RadioButtonGroupFactory(DataItemService dataItemService, IFormValues formValues) : IControlFactory<JJRadioButtonGroup>
{
    private DataItemService DataItemService { get; } = dataItemService;
    private IFormValues FormValues { get; } = formValues;

    public JJRadioButtonGroup Create()
    {
        return new JJRadioButtonGroup(DataItemService, FormValues);
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