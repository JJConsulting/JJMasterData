#nullable enable

using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.Web.Factories;

internal record ControlContext(
    FormStateData FormStateData,
    string ParentComponentName,
    object? Value = null);