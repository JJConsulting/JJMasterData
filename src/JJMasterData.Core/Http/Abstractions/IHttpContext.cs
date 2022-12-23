namespace JJMasterData.Core.Http.Abstractions;

public interface IHttpContext
{
    bool IsPost { get; }
    IHttpSession Session { get; }
    IHttpRequest Request { get; }
    IHttpResponse Response { get; }

    /// <summary>
    /// Verify if the current User has a valid Claims property.
    /// </summary>
    /// <returns></returns>
    bool HasClaimsIdentity();

    /// <summary>
    /// Returns a User claim.
    /// </summary>
    /// <param name="key">The claim type.</param>
    /// <returns></returns>
    string GetClaim(string key);
}