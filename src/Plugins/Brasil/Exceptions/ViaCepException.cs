using System;
using JJMasterData.Commons.Exceptions;

namespace JJMasterData.Brasil.Exceptions;

public class ViaCepException : JJMasterDataException
{
    public ViaCepException(string message) : base(message)
    {
    }

    public ViaCepException(string message, Exception innerException) : base(message, innerException)
    {
    }
}