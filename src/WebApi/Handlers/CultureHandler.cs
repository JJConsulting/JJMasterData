using System.Globalization;

namespace JJMasterData.WebApi.Handlers;

public class CultureHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Any())
        {
            var reqHdrs = request.Headers.AcceptLanguage;
            if (reqHdrs.Count > 0)
            {
                var headerValue = reqHdrs
                    .OrderByDescending(e => e.Quality ?? 1.0D)
                    .First(e => e.Quality is null or > 0.0D);

                Thread.CurrentThread.CurrentUICulture = new CultureInfo(headerValue.Value);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }
}