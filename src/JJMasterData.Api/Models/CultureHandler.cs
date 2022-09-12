using System.Globalization;

namespace JJMasterData.Api.Models;

public class CultureHandler : DelegatingHandler
{
    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request != null && request.Headers != null && request.Headers.Count() > 0)
        {
            var reqHdrs = request.Headers.AcceptLanguage;
            if (reqHdrs != null && reqHdrs.Count > 0)
            {
                var headerValue = reqHdrs.OrderByDescending(e => e.Quality ?? 1.0D)
                    .Where(e => !e.Quality.HasValue || e.Quality.Value > 0.0D)
                    .First();

                Thread.CurrentThread.CurrentUICulture = new CultureInfo(headerValue.Value);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);
        return response;
    }
}