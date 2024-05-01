namespace JJMasterData.Commons.Exceptions;

public sealed class DataAccessProviderException(string message) : DataAccessException(message);