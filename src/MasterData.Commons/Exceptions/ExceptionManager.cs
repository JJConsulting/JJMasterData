using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Util;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Commons.Exceptions;

public static class ExceptionManager
{
    private const string UnexpectedErrorMessage = "Unexpected error.";
    
    public static ResponseLetter GetResponse(Exception ex)
    {
        var letter = new ResponseLetter();
        switch (ex)
        {
            case UnauthorizedAccessException exAccess:
                letter.Message = exAccess.Message;
                letter.Status = (int)HttpStatusCode.Unauthorized;
                break;
            case JJMasterDataException mdException:
                letter.Message = mdException.Message;
                letter.Status = (int)HttpStatusCode.BadRequest;
                break;
            case SqlException exSql:
            {
                var errMsg = GetMessage(exSql);
                letter.Message = errMsg;
                letter.Status = (int)HttpStatusCode.BadRequest;
                letter.ValidationList = new Dictionary<string, string> { { "DB", errMsg } };
                break;
            }
            case KeyNotFoundException exNotFound:
                letter.Message = exNotFound.Message ?? "Page not found.";
                letter.Status = (int)HttpStatusCode.NotFound;
                break;
            default:
                letter.Message = ex.Message;
                letter.Status = (int)HttpStatusCode.InternalServerError;
                break;
        }

        return letter;
    }

    public static string GetMessage(SqlException sqlException)
    {
        string message;
        switch (sqlException.Number)
        {
            case 547:
                message = "The record cannot be deleted because it is being used as a dependency.";
                break;
            case 2627 or 2601:
                message = "Record already registered.";
                break;
            case 170:
                message = "Invalid character.";
                break;
            case >= 50000:
                message = sqlException.Message;
                break;
            default:
#if DEBUG
                message = sqlException.Message;
#else
                message = UnexpectedErrorMessage;
#endif
                break;
        }

        return message;
    }
    

    public static string GetMessage(Exception ex)
    {
        if (ex is SqlException exSql)
            return GetMessage(exSql);
        if (ex is IOException)
            return "Error while processing file.";
#if DEBUG
        return ex.Message;
#else
        return UnexpectedErrorMessage;
#endif

    }
    
    public static string GetExceptionMessage(this IStringLocalizer localizer, Exception ex)
    {
        var message = GetMessage(ex);
        
        var args = StringManager.FindValuesByInterval(message, '{', '}').ToArray<object>();

        if (args.Length == 0) 
            return localizer[message];
        
        for (var i = 0; i < args.Length; i++)
        {
            message = message.Replace("{" + args[i] + "}", "{" + i + "}");
        }
            
        return localizer[message, args].Value;
    }

}
