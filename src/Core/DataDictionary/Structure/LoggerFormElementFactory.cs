using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Structure;

public class LoggerFormElementFactory(
    IOptionsSnapshot<DbLoggerOptions> options,
    IMasterDataUrlHelper urlHelper,
    IStringLocalizer<MasterDataResources> stringLocalizer)
{
    private IMasterDataUrlHelper UrlHelper { get; } = urlHelper;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private DbLoggerOptions Options { get; } = options.Value;

    public FormElement GetFormElement(bool isModal)
    {
        var formElement = new FormElement(DbLoggerElement.GetInstance(Options))
        {
            Title = StringLocalizer["Application Log"],
            SubTitle = string.Empty,
            Icon = IconType.Film
        };

        formElement.Fields[Options.IdColumnName].VisibleExpression = "val:0";

        formElement.Fields[Options.CategoryColumnName].VisibleExpression = "val:0";
        formElement.Fields[Options.CategoryColumnName].CssClass = "col-sm-6";

        formElement.Fields[Options.LevelColumnName].LineGroup = 1;
        formElement.Fields[Options.LevelColumnName].CssClass = "col-sm-6";

        formElement.Fields[Options.CreatedColumnName].LineGroup = 2;

        formElement.Fields[Options.MessageColumnName].LineGroup = 3;
        formElement.Fields[Options.MessageColumnName].CssClass = "col-sm-10";

        var levelField = formElement.Fields[Options.LevelColumnName];
        levelField.Component = FormComponent.ComboBox;
        levelField.DataItem = new FormElementDataItem
        {
            Items =
            [
                new("0", nameof(LogLevel.Trace), IconType.SolidMapLocation, "#808080"),
                new("1", nameof(LogLevel.Debug), IconType.Bug, "#198754"),
                new("2", nameof(LogLevel.Information), IconType.InfoCircle, "#0d6efd"),
                new("3", nameof(LogLevel.Warning), IconType.SolidTriangleExclamation, "#ffc107"),
                new("4", nameof(LogLevel.Error), IconType.TimesCircle, "#dc3545"),
                new("5", nameof(LogLevel.Critical), IconType.Fire, "#FF5733"),
                new("6", nameof(LogLevel.None), IconType.CircleO, "#808080")
            ],
            GridBehavior = DataItemGridBehavior.Icon,
            ShowIcon = true
        };

        var btnClearAll = new UrlRedirectAction
        {
            Name = "btnClearLog",
            Icon = IconType.Trash,
            Text = StringLocalizer["Clear Log"],
            ShowAsButton = true,
            ConfirmationMessage = StringLocalizer["Do you want to clear ALL logs?"],
            UrlRedirect = UrlHelper.Action("ClearAll", "Log", new { Area = "DataDictionary", isModal })
        };

        formElement.Options.GridTableActions.Clear();
        formElement.Options.GridToolbarActions.InsertAction.SetVisible(false);
        formElement.Options.GridToolbarActions.Add(btnClearAll);
        formElement.Options.GridToolbarActions.FilterAction.Text = "Filters";
        formElement.Options.GridToolbarActions.FilterAction.ShowIconAtCollapse = true;

        formElement.Options.Grid.IsCompact = true;
        formElement.Options.Grid.UseVerticalLayoutAtFilter = false;

        return formElement;
    }
}