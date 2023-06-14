using System;

namespace JJMasterData.Commons.Exceptions;

public class JJMasterDataException : Exception
{
    public JJMasterDataException(string message) : base(message)
    {
    }

    public JJMasterDataException(string message, Exception innerException) : base(message, innerException)
    {
    }
}