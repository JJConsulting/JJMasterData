using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Net;
using JJMasterData.Commons.Data.Entity.Models;
using Microsoft.Data.SqlClient;

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
            case DataAccessException exSql:
            {
                var errMsg = GetMessage(exSql);
                letter.Message = errMsg;
                letter.Status = (int)HttpStatusCode.BadRequest;
                letter.ValidationList = new Dictionary<string, string> { { "DB", errMsg } };
                break;
            }
            case JJMasterDataException mdException:
                letter.Message = mdException.Message;
                letter.Status = (int)HttpStatusCode.BadRequest;
                break;
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

    public static string GetMessage(DataAccessCommandException commandException)
    {
        string message;
        switch (commandException.ErrorKind)
        {
            case DataAccessErrorKind.DependencyCannotBeDeleted:
                message = "The record cannot be deleted because it is being used as a dependency.";
                break;
            case DataAccessErrorKind.RecordAlreadyRegistered:
                message = "Record already registered.";
                break;
            case DataAccessErrorKind.InvalidCharacter:
                message = "Invalid character.";
                break;
            case DataAccessErrorKind.Custom:
                message = commandException.Message;
                break;
            case DataAccessErrorKind.Unhandled:
            default:
#if DEBUG
                message = commandException.Message;
#else
                message = UnexpectedErrorMessage;
#endif
                break;
        }

        return message;
    }
    
    public static string GetMessage(Exception ex)
    {
        if (ex is DataAccessCommandException commandException)
            return GetMessage(commandException);
        if (ex is IOException)
            return "Error while processing file.";
#if DEBUG
        return ex.Message;
#else
        return UnexpectedErrorMessage;
#endif

    }

}
