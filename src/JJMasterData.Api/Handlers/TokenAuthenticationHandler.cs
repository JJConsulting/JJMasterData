using System.Security.Claims;
using System.Text.Encodings.Web;
using JJMasterData.Api.Models;
using JJMasterData.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace JJMasterData.Api.Handlers;

public class TokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Name = "TokenAuthenticationScheme";

    private readonly AccountService _accountService;

    public TokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, 
                                      ILoggerFactory logger, 
                                      UrlEncoder encoder, 
                                      ISystemClock clock,
                                      AccountService accountService)
        : base(options, logger, encoder, clock)
    {
        _accountService = accountService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headers = Request.Headers;

        if (TryRetrieveToken(Request, out string? token))
        {
            var tokenInfo = _accountService.GetTokenInfo(token);

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