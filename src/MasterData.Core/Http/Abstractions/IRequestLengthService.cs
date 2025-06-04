namespace JJMasterData.Core.Http.Abstractions;

public interface IRequestLengthService
{
    public long GetMaxRequestBodySize();
}
