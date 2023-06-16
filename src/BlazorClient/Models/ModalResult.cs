using Microsoft.AspNetCore.Components;

namespace JJMasterData.BlazorClient.Models;

public class ModalResult
{
    public ModalResultType ResultType { get; private set; } = ModalResultType.NoSet;

    // Whatever object you wish to pass back
    public object? Data { get; set; } = null;

    // A set of static methods to build a BootstrapModalResult
    public static ModalResult Ok() => new() { ResultType = ModalResultType.Ok };
    public static ModalResult Exit() => new() { ResultType = ModalResultType.Exit };
    public static ModalResult Cancel() => new() { ResultType = ModalResultType.Cancel };
    public static ModalResult Ok(object data) => new() { Data = data, ResultType = ModalResultType.Ok };
    public static ModalResult Exit(object data) => new() { Data = data, ResultType = ModalResultType.Exit };
    public static ModalResult Cancel(object data) => new() { Data = data, ResultType = ModalResultType.Cancel };
}

public enum ModalResultType
{
    NoSet,
    Ok,
    Cancel,
    Exit
}
