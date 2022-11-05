using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
//not remove

namespace JJMasterData.Commons.Exceptions;

public class ExceptionManager
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
            case DataDictionaryException dcEx:
                err.Message = dcEx.Message;
                err.Status = (int)HttpStatusCode.BadRequest;
                Log.AddError(ex.Message);
                break;
            case SqlException exSql:
            {
                string errMsg = GetMessage(exSql);
                err.Message = errMsg;
                err.Status = (int)HttpStatusCode.BadRequest;
                err.ValidationList = new Hashtable();
                err.ValidationList.Add("DB", errMsg);
                break;
            }
            case KeyNotFoundException or null:
                err.Message = ex?.Message ?? "Page not found.";
                err.Status = (int)HttpStatusCode.NotFound;
                break;
            default:
                Log.AddError(ex.Message);
                err.Message = ex.Message;
                err.Status = (int)HttpStatusCode.InternalServerError;
                break;
        }

        return err;
    }

    public static string GetMessage(SqlException ex)
    {
        string message;
        if (ex.Number == 547)
        {
            message = Translate.Key("The record cannot be deleted because it is being used as a dependency.");
        }
        else if (ex.Number == 2627 || ex.Number == 2601)
        {
            message = Translate.Key("Record already registered.");
        }
        else if (ex.Number == 170)
        {
            message = Translate.Key("Invalid character.");
        }
        else if (ex.Number >= 50000)
        {
            message = ex.Message;
        }
        else
        {
            message = Translate.Key("Unexpected error.");
            Log.AddError(ex.ToString());
        }
            

        return message;

    }

    public static string GetMessage(Exception ex)
    {
        string message;
        if (ex is SqlException exSql)
        {
            message = GetMessage(exSql);
        }
        else
        {
            message = ex.Message;
        }

        return message;
    }


}
