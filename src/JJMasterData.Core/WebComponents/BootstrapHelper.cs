using JJMasterData.Commons.DI;
using JJMasterData.Commons.Settings;

namespace JJMasterData.Core.WebComponents;

/// <summary>
/// Helper to Bootstrap strings, everything defaults to version 3.
/// Supports Bootstrap 3,4 and 5
/// </summary>
/// <author>
/// Gustavo Barros - 04/2022
/// </author>
public static class BootstrapHelper
{
    #region Settings
    private static ISettings Settings => JJService.Settings;

    public static int Version => Settings.BootstrapVersion;
    #endregion

    #region Button
    private static string Button(string className) => " btn btn-" + className;
    public static string DefaultButton => Version switch
    {
        >= 4 => Button("light border border-secondary"),
        _ => Button("default")
    };
    #endregion

    #region Panel
    public static string GetPanel(string className)
    {
        if (Version == 3)
        {
            return "panel panel-" + className;
        }

        return "card border-" + className.Replace("default", "jjmasterdata");
    }

    public static string GetPanelHeading(string className) => Version switch
    {
        >= 4 => " card-header bg-" + className.Replace("default", "jjmasterdata"),
        _ => " panel-heading"
    };

    public static string PanelTitle => Version switch
    {
        >= 4 => " jj-panel-title",
        _ => " panel-title"
    };

    public static string PanelBody => Version switch
    {
        >= 4 => " card-body",
        _ => " panel-body"
    };

    public static string PanelGroup => Version switch
    {
        >= 4 => " card-group",
        _ => " panel-group"
    };

    public static string PanelCollapse => Version switch
    {
        >= 4 => " panel-collapse in collapse show",
        _ => " panel-collapse collapse in"
    };
    #endregion

    #region InputGroup

    public static string InputGroupBtn => Version switch
    {
        5 => "input-group",
        4 => " input-group-prepend",
        _ => " input-group-btn"
    };

    public static string InputGroupAddon => Version switch
    {
        5 => "input-group-text",
        4 => " input-group-prepend",
        _ => " input-group-addon"
    };
    #endregion

    #region NavBar 
    public static string NavBar => Version switch
    {
        >= 4 => " navbar navbar-light bg-light navbar-expand-sm",
        _ => " navbar navbar-default"
    };
    #endregion

    #region Miscellaneous
    public static string Show => Version switch
    {
        >= 4 => " show",
        _ => string.Empty
    };
    public static string Label => Version switch
    {
        >= 4 => " col-form-label",
        _ => " control-label"
    };

    public static string FormHorizontal => Version switch
    {
        >= 4 => string.Empty,
        _ => " form-horizontal"
    };
    
    public static string DateIcon => Version switch
    {
        >= 4 => "calendar",
        _ => "th"
    };

    public static string Well => Version switch
    {
        >= 4 => " card",
        _ => " well"
    };

    public static string HasError => Version switch
    {
        >= 4 => "is-invalid",
        _ => "has-error"
    };

    public static string CenterBlock => Version switch
    {
        >= 4 => "d-block mx-auto",
        _ => "center-block"
    };

    public static string PageHeader => Version switch
    {
        >= 4 => "pb-2 mt-4 mb-2 border-bottom",
        _ => "page-header"
    };

    #endregion

    #region Bootstrap5 Breaking Changes
    public static string TextRight => Version switch
    {
        5 => " text-end",
        _ => " text-right"
    };

    public static string TextLeft => Version switch
    {
        5 => " text-start",
        _ => " text-left"
    };

    public static string MarginLeft => Version switch
    {
        5 => " ms",
        _ => " ml"
    };

    public static string MarginRight => Version switch
    {
        5 => " me",
        _ => " mr"
    };

    public static string FormGroup => Version switch
    {
        5 => " mb-3",
        _ => " form-group"
    };

    public static string Close => Version switch
    {
        5 => " btn-close",
        _ => " close"
    };

    public static string CloseButtonTimes => Version switch
    {
        5 => string.Empty,
        _ => "×"
    };

    public static string DataDismiss => Version switch
    {
        5 => "data-bs-dismiss",
        _ => "data-dismiss"
    };
    
    public static string GetDataDismiss(string value)
    {
        return $"{DataDismiss}={value}";
    }

    public static string DataToggle => Version switch
    {
        5 => "data-bs-toggle",
        _ => "data-toggle"
    };

    public static string PullRight => Version switch
    {
        5 => "float-end",
        4 => "float-right",
        _ => "pull-right"
    };

    public static string PullLeft => Version switch
    {
        5 => "float-start",
        4 => "float-left",
        _ => "pull-left"
    };

    public static string GetDataToggle(string value)
    {
        return $"{DataToggle}={value}";
    }

    public static string GetModalScript(string modalCssId) => Version switch
    {
        5 => "new bootstrap.Modal(document.getElementById('" + modalCssId + "'),{}).show();",
        _ => "$('#" + modalCssId + "').modal();"
    };

    #endregion
    public static string ApplyCompatibility(string cssClass)
    {
        if (Version == 3) return null;

        cssClass = cssClass?.Replace("pull-right", PullRight);

        return cssClass;
    }
}

