using System.Net.Http.Json;
using System.Text;
using HotelBooking.Data.Application.Dto;

namespace HotelBooking.Web.Services;

public sealed class GatewayClient : IGatewayClient
{
    private readonly HttpClient _http;
    private readonly ITokenStore _tokenStore;

    public GatewayClient(HttpClient http, ITokenStore tokenStore )
    {
        _http = http;
        _tokenStore = tokenStore;
    }

    // --------------------
    // Auth
    // --------------------
    public async Task<string> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/login", new LoginRequest(email, password), ct);
        return await ReadTokenOrThrow(resp, "Login failed", ct);
    }

    public async Task<string> RegisterAsync(string email, string password, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/register", new RegisterRequest(email, password), ct);
        return await ReadTokenOrThrow(resp, "Register failed", ct);
    }

    // --------------------
    // Hotels (public)
    // --------------------
    public async Task<IReadOnlyList<HotelDto>> GetHotelsAsync(CancellationToken ct = default)
        => await GetListOrEmptyAsync<HotelDto>("api/hotels", ct);

    public async Task<HotelDto?> GetHotelByIdAsync(Guid id, CancellationToken ct = default)
        => await GetOrNullAsync<HotelDto>($"api/hotels/{id}", ct);

    // --------------------
    // Rooms (public)
    // --------------------
    public async Task<IReadOnlyList<RoomDto>> GetRoomsAsync(Guid? hotelId = null, CancellationToken ct = default)
    {
        var url = hotelId.HasValue ? $"api/rooms?hotelId={hotelId.Value}" : "api/rooms";
        return await GetListOrEmptyAsync<RoomDto>(url, ct);
    }

    public async Task<RoomDto?> GetRoomByIdAsync(Guid id, CancellationToken ct = default)
        => await GetOrNullAsync<RoomDto>($"api/rooms/{id}", ct);

    public async Task<IReadOnlyList<RoomDto>> SearchRoomsAsync(SearchRoomsRequest request, CancellationToken ct = default)
    {
        // query: city, checkIn, checkOut, minCapacity
        var qs = new StringBuilder("api/rooms/search?");
        qs.Append($"checkIn={request.CheckIn:yyyy-MM-dd}&checkOut={request.CheckOut:yyyy-MM-dd}");

        if (!string.IsNullOrWhiteSpace(request.City))
            qs.Append($"&city={Uri.EscapeDataString(request.City.Trim())}");

        if (request.MinCapacity.HasValue)
            qs.Append($"&minCapacity={request.MinCapacity.Value}");

        return await GetListOrEmptyAsync<RoomDto>(qs.ToString(), ct);
    }

    // --------------------
    // Bookings (client)
    // --------------------
    public async Task CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default)
    {
        Console.WriteLine("BaseAddress: " + _http.BaseAddress);

        var tok = _tokenStore.GetToken();
        Console.WriteLine("TOKEN LEN IN UPDATE: " + (tok?.Length ?? 0));

        Console.WriteLine("Default Auth header: " +
            (_http.DefaultRequestHeaders.Authorization?.ToString() ?? "<null>"));
        var resp = await _http.PostAsJsonAsync("api/bookings", request, ct);
        await EnsureSuccess(resp, "Booking failed", ct);
    }

    public async Task<IReadOnlyList<BookingDto>> GetMyBookingsAsync(CancellationToken ct = default)
        => await GetListOrEmptyAsync<BookingDto>("api/bookings/my", ct);

    // --------------------
    // Admin: Hotels CRUD
    // --------------------
    public async Task CreateHotelAsync(UpsertHotelDto dto, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/admin/hotels", dto, ct);
        await EnsureSuccess(resp, "Create hotel failed", ct);
    }

    public async Task UpdateHotelAsync(Guid id, UpsertHotelDto dto, CancellationToken ct = default)
    {
        var resp = await _http.PutAsJsonAsync($"api/admin/hotels/{id}", dto, ct);
        await EnsureSuccess(resp, "Update hotel failed", ct);
    }

    public async Task DeleteHotelAsync(Guid id, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/admin/hotels/{id}", ct);
        await EnsureSuccess(resp, "Delete hotel failed", ct);
    }

    // --------------------
    // Admin: Rooms CRUD
    // --------------------
    public async Task CreateRoomAsync(UpsertRoomDto dto, CancellationToken ct = default)
    {
        var resp = await _http.PostAsJsonAsync("api/admin/rooms", dto, ct);
        await EnsureSuccess(resp, "Create room failed", ct);
    }

    public async Task UpdateRoomAsync(Guid id, UpsertRoomDto dto, CancellationToken ct = default)
    {
        Console.WriteLine("BaseAddress: " + _http.BaseAddress);

        var tok = _tokenStore.GetToken();
        Console.WriteLine("TOKEN LEN IN UPDATE: " + (tok?.Length ?? 0));

        Console.WriteLine("Default Auth header: " +
            (_http.DefaultRequestHeaders.Authorization?.ToString() ?? "<null>"));

        var resp = await _http.PutAsJsonAsync($"api/admin/rooms/{id}", dto, ct);
        await EnsureSuccess(resp, "Update room failed", ct);
    }
    public async Task DeleteRoomAsync(Guid id, CancellationToken ct = default)
    {
        var resp = await _http.DeleteAsync($"api/admin/rooms/{id}", ct);
        await EnsureSuccess(resp, "Delete room failed", ct);
    }

    // --------------------
    // Admin: Bookings & Stats
    // --------------------
    public async Task<IReadOnlyList<BookingDto>> GetAllBookingsAsync(CancellationToken ct = default)
        => await GetListOrEmptyAsync<BookingDto>("api/admin/bookings", ct);

    public async Task<AdminStatsDto> GetAdminStatsAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var url = $"api/admin/stats/hotels?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";
        var dto = await _http.GetFromJsonAsync<AdminStatsDto>(url, ct);
        if (dto is null) throw new InvalidOperationException("Stats response is empty");
        return dto;
    }
    // ============================
    // Helpers
    // ============================
    private static async Task<string> ReadTokenOrThrow(HttpResponseMessage resp, string prefix, CancellationToken ct)
    {
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"{prefix}: {(int)resp.StatusCode} {body}");
        }

        var json = await resp.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct)
                   ?? throw new InvalidOperationException($"{prefix}: empty response");

        return json.Token;
    }

    private static async Task EnsureSuccess(HttpResponseMessage resp, string message, CancellationToken ct)
    {
        if (resp.IsSuccessStatusCode) return;

        var body = await resp.Content.ReadAsStringAsync(ct);
        throw new HttpRequestException($"{message}. Status={(int)resp.StatusCode} {resp.StatusCode}. Body={body}");
    }

    private async Task<IReadOnlyList<T>> GetListOrEmptyAsync<T>(string url, CancellationToken ct)
    {
        var data = await _http.GetFromJsonAsync<List<T>>(url, ct);
        return data ?? new List<T>();
    }

    private async Task<T?> GetOrNullAsync<T>(string url, CancellationToken ct)
    {
        var resp = await _http.GetAsync(url, ct);
        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            return default;

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"GET {url} failed: {(int)resp.StatusCode} {body}");
        }

        return await resp.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
    }
}
