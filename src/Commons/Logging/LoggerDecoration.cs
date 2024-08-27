#nullable enable

using System;
using System.Collections;
using System.Text;

namespace JJMasterData.Commons.Logging;

internal static class LoggerDecoration
{

    public static string GetMessageException(Exception exception)
    {
        var message = new StringBuilder();

        if (!string.IsNullOrEmpty(exception.Message))
        {
            message.Append("Message: ");
            message.AppendLine();
            message.AppendLine(exception.Message);
        }
        
        if (exception.InnerException != null)
        {
            message.AppendLine();
            message.Append("InnerException: ");
            message.AppendLine(exception.InnerException.Message);
        }

        if (exception.Data.Count > 0)
        {
            message.Append(GetDataAccessMessage(exception));
        }

        if (!string.IsNullOrEmpty(exception.StackTrace))
        {
            message.AppendLine();
            message.AppendLine("StackTrace:");
            message.AppendLine(exception.StackTrace);
        }

        if (string.IsNullOrEmpty(exception.Source))
        {
            message.AppendLine();
            message.AppendLine("Source:");
            message.AppendLine(exception.Source);
        }


        return message.ToString();
    }


    private static string GetDataAccessMessage(Exception exception)
    {
        var message = new StringBuilder();
        foreach (DictionaryEntry data in exception.Data)
        {
            var key = data.Key?.ToString();
            if (key == null)
                continue;

            if (!key.Contains("DataAccess"))
                continue;

            message.AppendLine();
            message.Append(key);
            message.AppendLine(": ");
            message.Append(data.Value);
        }

        return message.ToString();
    }

}

