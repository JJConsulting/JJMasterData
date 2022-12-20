using System;

namespace JJMasterData.Commons.Exceptions;

public class DataAccessException : JJMasterDataException
{
    public DataAccessException(string message) : base(message)
    {
    }
    
    public DataAccessException(string message, Exception ex) : base(message, ex)
    {
    }
    
    public DataAccessException(Exception ex) : base(ex.Message, ex.InnerException)
    {
    }
}

