using System.Collections.Generic;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Structure;

public class LoggerFormElementFactory(IOptionsSnapshot<DbLoggerOptions> options,IMasterDataUrlHelper urlHelper, IStringLocalizer<MasterDataResources> stringLocalizer)
{
    private IMasterDataUrlHelper UrlHelper { get; } = urlHelper;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private DbLoggerOptions Options { get; } = options.Value;

    public FormElement GetFormElement()
    {
        var formElement = new FormElement(DbLoggerElement.GetInstance(Options))
        {
            Title = StringLocalizer["Application Log"],
            SubTitle = string.Empty
        };
        
        formElement.Options.GridToolbarActions.InsertAction.SetVisible(false);
        
        formElement.Fields["Id"].VisibleExpression = "val:0";
        
        var logLevel = formElement.Fields[Options.LevelColumnName];
        logLevel.Component = FormComponent.ComboBox;
        logLevel.DataItem = new FormElementDataItem
        {
            Items = new List<DataItemValue>
            {
                new("0", LogLevel.Trace.ToString(),  IconType.SolidMapLocation, "#808080"), 
                new("1", LogLevel.Debug.ToString(), IconType.Bug, "#198754"), 
                new("2", LogLevel.Information.ToString(), IconType.InfoCircle,  "#0d6efd"),
                new("3", LogLevel.Warning.ToString(), IconType.SolidTriangleExclamation, "#ffc107"), 
                new("4", LogLevel.Error.ToString(), IconType.TimesCircle, "#dc3545"), 
                new("5", LogLevel.Critical.ToString(), IconType.Fire,  "#FF5733"), 
                new("6", LogLevel.None.ToString(), IconType.CircleO,  "#808080")
            },
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
            UrlRedirect = UrlHelper.Action("ClearAll", "Log", new {Area="DataDictionary"})
        };
        
        formElement.Options.GridTableActions.Clear();
        
        formElement.Options.GridToolbarActions.Add(btnClearAll);
        formElement.Options.GridToolbarActions.FilterAction.Text = "Filters";
        formElement.Options.GridToolbarActions.FilterAction.ShowIconAtCollapse = true;
        formElement.Options.Grid.IsCompact = true;
        
        return formElement;
    }
}