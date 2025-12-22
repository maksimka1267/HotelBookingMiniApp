using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace HotelBooking.Web.Services;

public sealed class BearerTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _http;
    private readonly ITokenStore _tokenStore;

    public BearerTokenHandler(IHttpContextAccessor http, ITokenStore tokenStore)
    {
        _http = http;
        _tokenStore = tokenStore;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = _http.HttpContext;

        var sessionId = ctx?.Session?.Id ?? "NO_HTTP_CONTEXT_OR_SESSION";
        var cookieHeader = request.Headers.TryGetValues("Cookie", out var vals) ? string.Join(";", vals) : "(none)";
        var token = _tokenStore.GetToken();

        Console.WriteLine($"[BearerTokenHandler] SessionId={sessionId} TokenLen={(token?.Length ?? 0)} Url={request.RequestUri}");

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
