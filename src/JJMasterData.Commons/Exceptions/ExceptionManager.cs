using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;

namespace JJMasterData.Commons.Exceptions;

public static class ExceptionManager
{
    public static ResponseLetter GetResponse(Exception ex)
    {
        var err = new ResponseLetter();
        switch (ex)
        {
            case UnauthorizedAccessException unauthorizedAccessException:
                err.Message = unauthorizedAccessException.Message;
                err.Status = (int)HttpStatusCode.Unauthorized;
                break;
            case JJMasterDataException jjMasterDataException:
                err.Message = jjMasterDataException.Message;
                err.Status = (int)HttpStatusCode.BadRequest;
                Log.AddError(ex.Message);
                break;
            case SqlException sqlException:
            {
                string errMsg = GetMessage(sqlException);
                err.Message = errMsg;
                err.Status = (int)HttpStatusCode.BadRequest;
                err.ValidationList = new Hashtable { { "DB", errMsg } };
                break;
            }
            case KeyNotFoundException keyNotFoundException:
                err.Message = keyNotFoundException.Message ?? "Page not found.";
                err.Status = (int)HttpStatusCode.NotFound;
                break;
            default:
                Log.AddError(ex, ex.Message);
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
                message = Translate.Key("The record cannot be deleted because it is being used as a dependency.");
                break;
            case 2627 or 2601:
                message = Translate.Key("Record already registered.");
                break;
            case 170:
                message = Translate.Key("Invalid character.");
                break;
            case >= 50000:
                message = ex.Message;
                break;
            default:
                message = Translate.Key("Unexpected error.");
                Log.AddError(ex.ToString());
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
