using System;
using System.Runtime.Serialization;

namespace JJMasterData.Protheus;

public class ProtheusException : Exception
{
    public ProtheusException()
    {
    }

    public ProtheusException(string message) : base(message)
    {
    }

    public ProtheusException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected ProtheusException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}