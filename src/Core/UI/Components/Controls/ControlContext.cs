#nullable enable

using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.UI.Components;

public record ControlContext(
    FormStateData FormStateData,
    string ParentComponentName,
    object? Value = null);