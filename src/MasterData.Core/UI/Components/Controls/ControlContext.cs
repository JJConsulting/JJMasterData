#nullable enable

using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.UI.Components;

public class ControlContext(
    FormStateData formStateData,
    string parentComponentName,
    object? value = null)
{
    public FormStateData FormStateData { get; } = formStateData;
    public string ParentComponentName { get; } = parentComponentName;
    public object? Value { get; } = value;
}