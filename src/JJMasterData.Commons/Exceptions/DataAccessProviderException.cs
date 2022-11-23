using System;

namespace JJMasterData.Commons.Exceptions;

public class DataAccessProviderException : DataAccessException
{
    public DataAccessProviderException(string message) : base(message)
    {
    }
}