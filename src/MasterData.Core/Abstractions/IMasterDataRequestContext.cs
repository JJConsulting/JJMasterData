namespace JJMasterData.Core.Abstractions;


//Com isso essa lib pode ser usada em aplicações console por exemplo
public interface IMasterDataRequestContext
{
    bool HasFormContentType { get; }
    string ApplicationPath { get; }
    string ApplicationUri { get; }
    string AbsoluteUri { get; }
    string? RemoteIpAddress { get; }
    string? UserAgent { get; }
    string? GetQueryValue(string key) => null;
    string? GetFormValue(string key);
    string? GetClaimValue(string claimType);
}


//Não implementou? Sem problemas
internal sealed class EmptyMasterDataRequestContext : IMasterDataRequestContext
{
    public bool HasFormContentType => false;
    public string ApplicationPath => string.Empty;
    public string ApplicationUri => string.Empty;
    public string AbsoluteUri => string.Empty;
    public string? RemoteIpAddress => null;
    public string? UserAgent => null;
    public string? GetQueryValue(string key) => null;
    public string? GetFormValue(string key) => null;
    public string? GetClaimValue(string claimType) => null;
}
