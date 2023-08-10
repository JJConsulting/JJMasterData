using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Logging.Db;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.UI.FormEvents.Abstractions;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.DataDictionary.Factories;

public class LoggerGridEventHandler : GridEventHandlerBase
{
    private DbLoggerOptions Options { get; }
    public override string ElementName { get; }


    public LoggerGridEventHandler(IOptions<DbLoggerOptions> dbLoggerOptions )
    {
        Options = dbLoggerOptions.Value;
        ElementName = dbLoggerOptions.Value.TableName;
    }

    public override void OnGridViewCreated(JJGridView gridView)
    {
        gridView.CurrentOrder = $"{Options.CreatedColumnName} DESC";
    }

    public override void OnRenderCell(object sender, GridCellEventArgs eventArgs)
    {
        string? message;
        if (eventArgs.Field.Name.Equals(Options.MessageColumnName))
        {
            message = eventArgs.DataRow[Options.MessageColumnName].ToString()?.Replace("\n", "<br>");
        }
        else
        {
            message = eventArgs.Sender.GetHtml();
        }

        eventArgs.HtmlResult = message;
    }

}