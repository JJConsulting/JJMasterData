using System.Security.Policy;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.Web;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Structure;

public class LoggerFormElementFactory : IFormElementFactory
{
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private DbLoggerOptions Options { get; }
    public string ElementName { get; }

    public LoggerFormElementFactory(IOptions<DbLoggerOptions> options,JJMasterDataUrlHelper urlHelper, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        UrlHelper = urlHelper;
        StringLocalizer = stringLocalizer;
        Options = options.Value;
        ElementName = options.Value.TableName;
    }
    
    public FormElement GetFormElement()
    {
        var formElement = new FormElement(DbLoggerElement.GetInstance(Options))
        {
            Title = StringLocalizer["Application Log"],
            SubTitle = string.Empty
        };

        formElement.Fields["Id"].VisibleExpression = "val:0";

        var logLevel = formElement.Fields[Options.LevelColumnName];
        logLevel.Component = FormComponent.ComboBox;

        logLevel.DataItem!.Items.Add(new DataItemValue("0", LogLevel.Trace.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("1", LogLevel.Debug.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("2", LogLevel.Information.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("3", LogLevel.Warning.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("4", LogLevel.Error.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("5", LogLevel.Critical.ToString()));
        logLevel.DataItem.Items.Add(new DataItemValue("6", LogLevel.None.ToString()));
        
        var btnClearAll = new UrlRedirectAction
        {
            Name = "btnClearLog",
            Icon = IconType.Trash,
            Text = StringLocalizer["Clear Log"],
            ShowAsButton = true,
            ConfirmationMessage = StringLocalizer["Do you want to clear ALL logs?"],
            UrlRedirect = UrlHelper.GetUrl("ClearAll", "Log","DataDictionary")
        };
        
        formElement.Options.GridTableActions.Clear();
        
        formElement.Options.GridToolbarActions.Add(btnClearAll);
        
        return formElement;
    }
}