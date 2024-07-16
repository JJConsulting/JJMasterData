using System;

namespace JJMasterData.Core.UI;

/// <summary>
/// Helper to Bootstrap strings, everything defaults to version 5.
/// Supports Bootstrap 3,4 and 5
/// </summary>
/// <author>
/// Gustavo Barros - 04/2022
/// </author>
public static class BootstrapHelper
{
    #region Options

    public static readonly int Version;

    #endregion

    static BootstrapHelper()
    {
        if (int.TryParse(AppContext.GetData("JJMasterData.BootstrapVersion")?.ToString(), out var version))
        {
            Version = version;
        }
        else
        {
            Version = 5; 
        }
        PanelTitle = Version switch
        {
            >= 4 => " jj-panel-title",
            _ => " panel-title"
        };
        PanelBody = Version switch
        {
            >= 4 => " card-body",
            _ => " panel-body"
        };
        PanelGroup = Version switch
        {
            >= 4 => " card-group",
            _ => " panel-group"
        };
        PanelCollapse = Version switch
        {
            >= 4 => " panel-collapse in collapse show",
            _ => " panel-collapse collapse in"
        };
        InputGroupBtn = Version switch
        {
            5 => "input-group",
            4 => " input-group-prepend",
            _ => " input-group-btn"
        };
        InputGroupAddon = Version switch
        {
            5 => "input-group-text",
            4 => " input-group-prepend",
            _ => " input-group-addon"
        };
        Show = Version switch
        {
            >= 4 => " show",
            _ => " in"
        };
        Label = Version switch
        {
            5 => " form-label",
            4 => " col-form-label",
            _ => " control-label"
        };
        FormHorizontal = Version switch
        {
            >= 4 => string.Empty,
            _ => " form-horizontal"
        };
        HasError = Version switch
        {
            >= 4 => "is-invalid",
            _ => "has-error"
        };
        CenterBlock = Version switch
        {
            >= 4 => "d-block mx-auto",
            _ => "center-block"
        };
        PageHeader = Version switch
        {
            >= 4 => "pb-2 mb-2 border-bottom",
            _ => "page-header"
        };
        TextRight = Version switch
        {
            5 => " text-end",
            _ => " text-right"
        };
        TextLeft = Version switch
        {
            5 => " text-start",
            _ => " text-left"
        };
        MarginLeft = Version switch
        {
            5 => " ms",
            _ => " ml"
        };
        MarginRight = Version switch
        {
            5 => " me",
            _ => " mr"
        };
        FormGroup = Version switch
        {
            5 => " mb-3",
            _ => " form-group"
        };
        Close = Version switch
        {
            5 => " btn-close",
            _ => " close"
        };
        CloseButtonTimes = Version switch
        {
            5 => string.Empty,
            _ => "×"
        };
        DataDismiss = Version switch
        {
            5 => "data-bs-dismiss",
            _ => "data-dismiss"
        };
        DataToggle = Version switch
        {
            5 => "data-bs-toggle",
            _ => "data-toggle"
        };
        PullRight = Version switch
        {
            5 => "float-end",
            4 => "float-right",
            _ => "pull-right"
        };
        PullLeft = Version switch
        {
            5 => "float-start",
            4 => "float-left",
            _ => "pull-left"
        };
        LabelSucess = Version switch
        {
            >= 4 => "badge bg-success",
            _ => "label label-success"
        };
        LabelDefault = Version switch
        {
            >= 4 => "badge bg-secondary",
            _ => "label label-default"
        };
        LabelWarning = Version switch
        {
            >= 4 => "badge bg-warning",
            _ => "label label-warning"
        };
        LabelDanger = Version switch
        {
            >= 4 => "badge bg-danger",
            _ => "label label-danger"
        };
        BtnDefault = Version switch
        {
            >= 4 => " btn btn-secondary",
            _ => " btn btn-default"
        };
    }

    #region Panel

    public static string GetPanel(string className)
    {
        if (Version == 3)
        {
            return $"panel panel-{className}";
        }

        return $"card border-{className.Replace("default", "jjmasterdata")} border-opacity-75";
    }

    public static string GetPanelHeading(string className)
    {
        return Version switch
        {
            >= 4 => $" card-header bg-{className.Replace("default", "jjmasterdata")} bg-opacity-75",
            _ => " panel-heading"
        };
    }

    public static string PanelTitle { get; }

    public static string PanelBody { get; }

    public static string PanelGroup { get; }

    public static string PanelCollapse { get; }

    #endregion

    #region InputGroup

    public static string InputGroupBtn { get; }

    public static string InputGroupAddon { get; }

    #endregion

    #region Miscellaneous

    public static string Show { get; }

    public static string Label { get; }

    public static string FormHorizontal { get; }

    public static string HasError { get; }

    public static string CenterBlock { get; }

    public static string PageHeader { get; }

    #endregion

    #region Bootstrap5 Breaking Changes

    public static string TextRight { get; }

    public static string TextLeft { get; }

    public static string MarginLeft { get; }

    public static string MarginRight { get; }


    public static string FormGroup { get; }

    public static string Close { get; }

    public static string CloseButtonTimes { get; }

    public static string DataDismiss { get; }
    
    public static string GetDataDismiss(string value)
    {
        return $"{DataDismiss}={value}";
    }
    
    public static string DataToggle { get; }

    public static string PullRight { get; }

    public static string PullLeft { get; }


    public static string LabelSucess { get; }

    public static string LabelDefault { get; }

    public static string LabelWarning { get; }

    public static string LabelDanger { get; }

    public static string BtnDefault { get; }

    public static string GetDataToggle(string value)
    {
        return $"{DataToggle}={value}";
    }

    public static string GetModalScript(string modalCssId) => Version switch
    {
        5 => $"bootstrap.Modal.getOrCreateInstance(document.getElementById('{modalCssId}'),{{}}).show();",
        _ => $"$('#{modalCssId}').modal();"
    };

    public static string GetCloseModalScript(string modalCssId) => Version switch
    {
        5 => $"bootstrap.Modal.getOrCreateInstance(document.getElementById('{modalCssId}'),{{}}).hide();",
        _ => $"$('#{modalCssId}').modal('hide');"
    };

    #endregion
}