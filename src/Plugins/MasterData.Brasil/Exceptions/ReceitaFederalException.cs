using System;
using JJMasterData.Commons.Exceptions;

namespace JJMasterData.Brasil.Exceptions;

public class ReceitaFederalException : JJMasterDataException
{
    public ReceitaFederalException(string message) : base(message)
    {
    }

    public ReceitaFederalException(string message, Exception innerException) : base(message, innerException)
    {
    }
}