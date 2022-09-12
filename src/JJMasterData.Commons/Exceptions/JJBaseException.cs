using System;

namespace JJMasterData.Commons.Exceptions;

public class JJBaseException : Exception
{
    public JJBaseException(string message) : base(message) { }

    public JJBaseException(string message, Exception innerException) : base(message, innerException)  { }
}