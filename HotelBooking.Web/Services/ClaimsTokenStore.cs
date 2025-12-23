using HotelBooking.Web.Services;

public sealed class ClaimsTokenStore : ITokenStore
{
    private readonly IHttpContextAccessor _http;
    public ClaimsTokenStore(IHttpContextAccessor http) => _http = http;

    public string? GetToken()
        => _http.HttpContext?.User?.FindFirst("access_token")?.Value;

    public void SetToken(string token) { /* можно не хранить */ }

    public void Clear() { /* токен уберётся при SignOut */ }
}
