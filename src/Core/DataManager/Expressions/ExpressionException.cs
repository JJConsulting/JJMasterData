using JJMasterData.Commons.Exceptions;

using System;

namespace JJMasterData.Core.DataManager.Expressions;

public class ExpressionException : JJMasterDataException
{
    public required string Expression { get; init; }
    
    public ExpressionException(string message) : base(message)
    {
    }

    public ExpressionException(string message, Exception innerException) : base(message, innerException)
    {
    }
} 