using System.Security.Claims;
using System.Text.Encodings.Web;
using JJMasterData.WebApi.Models;
using JJMasterData.WebApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace JJMasterData.WebApi.Handlers;

public class TokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private AccountService AccountService { get; }
    public const string Name = "TokenAuthenticationScheme";

    public TokenAuthenticationHandler(
        AccountService accountService,
        IOptionsMonitor<AuthenticationSchemeOptions> options, 
                                      ILoggerFactory logger, 
                                      UrlEncoder encoder, 
                                      ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
        AccountService = accountService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (TryRetrieveToken(Request, out string? token))
        {
            var tokenInfo = AccountService.GetTokenInfo(token);

            if (tokenInfo != null)
            {
                var tokenIdentity = new TokenIdentity
                {
                    IsAuthenticated = true
                };

                var tokenPrincipal = new TokenPrincipal(tokenIdentity)
                {
                    TokenInfo = tokenInfo
                };

                Thread.CurrentPrincipal = tokenPrincipal;

                Request.HttpContext.User = tokenPrincipal;
            }
        }

        else
        {
            return Task.FromResult(AuthenticateResult.Fail($"Balancer not authorize token : for token={token}"));
        }

        var claims = new[] { new Claim("token",token!) };
        var identity = new ClaimsIdentity(claims, nameof(TokenAuthenticationHandler));
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private static bool TryRetrieveToken(HttpRequest request, out string? token)
    {
        if (!request.Headers.TryGetValue("Token", out StringValues tokens) || tokens.Count > 1)
        {
            token = null;

            return false;
        }
        token = tokens.ElementAt(0);
        return true;
    }
}