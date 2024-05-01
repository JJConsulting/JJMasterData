using System;
using JJMasterData.Commons.Data;
using Microsoft.Data.SqlClient;

namespace JJMasterData.Commons.Exceptions;

public sealed class DataAccessCommandException : DataAccessException
{
    public DataAccessCommand Command { get; }
    
    public DataAccessErrorKind ErrorKind { get; init; }

    public DataAccessCommandException(string message) : base(message)
    {
    }

    public DataAccessCommandException(string message, Exception ex) : base(message, ex)
    {
    }

    public DataAccessCommandException(Exception ex, DataAccessCommand command) : base(ex)
    {
        Command = command;
    }

    public static DataAccessCommandException FromSqlException(SqlException sqlException, DataAccessCommand command)
    {
        var exception = new DataAccessCommandException(sqlException, command)
        {
            ErrorKind = GetErrorKindFromCode(sqlException.ErrorCode)
        };
        return exception;
    }

    private static DataAccessErrorKind GetErrorKindFromCode(int errorCode)
    {
        return errorCode switch
        {
            547 => DataAccessErrorKind.DependencyCannotBeDeleted,
            2627 or 2601 => DataAccessErrorKind.RecordAlreadyRegistered,
            170 => DataAccessErrorKind.InvalidCharacter,
            >= 50000 => DataAccessErrorKind.Custom,
            _ => DataAccessErrorKind.Unhandled
        };
    }
}