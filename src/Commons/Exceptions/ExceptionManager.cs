using System;
using System.Collections.Generic;
using System.Net;
using JJMasterData.Commons.Data.Entity;
using Microsoft.Data.SqlClient;

namespace JJMasterData.Commons.Exceptions;

public static class ExceptionManager
{
    public static ResponseLetter GetResponse(Exception ex)
    {
        var err = new ResponseLetter();
        switch (ex)
        {
            case UnauthorizedAccessException exAccess:
                err.Message = exAccess.Message;
                err.Status = (int)HttpStatusCode.Unauthorized;
                break;
            case JJMasterDataException dcEx:
                err.Message = dcEx.Message;
                err.Status = (int)HttpStatusCode.BadRequest;
                break;
            case SqlException exSql:
            {
                string errMsg = GetMessage(exSql);
                err.Message = errMsg;
                err.Status = (int)HttpStatusCode.BadRequest;
                err.ValidationList = new Dictionary<string, object>();
                err.ValidationList.Add("DB", errMsg);
                break;
            }
            case KeyNotFoundException exNotFound:
                err.Message = exNotFound.Message ?? "Page not found.";
                err.Status = (int)HttpStatusCode.NotFound;
                break;
            default:
                err.Message = ex.Message;
                err.Status = (int)HttpStatusCode.InternalServerError;
                break;
        }

        return err;
    }

    public static string GetMessage(SqlException ex)
    {
        string message;
        switch (ex.Number)
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
                message = ex.Message;
                break;
            default:
#if DEBUG
                message = ex.Message;
#else
                message = Translate.Key("Unexpected error.");
#endif
                break;
        }

        return message;
    }

    public static string GetMessage(Exception ex)
    {
        if (ex is SqlException exSql)
            return GetMessage(exSql);
        
        return ex.Message;
    }

}
