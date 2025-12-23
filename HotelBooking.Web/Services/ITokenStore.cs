namespace HotelBooking.Web.Services;

public interface ITokenStore
{
    string? GetToken();
    void SetToken(string token);
    void Clear();
}
