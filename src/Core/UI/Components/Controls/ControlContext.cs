#nullable enable

using JJMasterData.Core.DataManager;

namespace JJMasterData.Core.Web.Factories;

internal record ControlContext(
    FormStateData FormStateData,
    object? Value = null);