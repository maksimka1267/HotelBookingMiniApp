using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelBooking.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IGatewayClient _gateway;
    private readonly ITokenStore _tokenStore;

    public LoginModel(IGatewayClient gateway, ITokenStore tokenStore)
    {
        _gateway = gateway;
        _tokenStore = tokenStore;
    }

    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = "";

    [BindProperty, Required, MinLength(6)]
    public string Password { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Tab { get; set; }

    public string ActiveTab => (Tab?.ToLowerInvariant() == "register") ? "register" : "login";

    public string? Error { get; set; }
    public string? Success { get; set; }

    public IActionResult OnGet()
    {
        // Если уже залогинен (cookie), не держим на странице входа
        if (User.Identity?.IsAuthenticated == true)
            return Redirect(ReturnUrl ?? "/Hotels/Index");

        return Page();
    }

    // ЕДИНЫЙ POST: handler = "login" | "register" | "logout"
    public async Task<IActionResult> OnPostAsync([FromForm] string handler)
    {
        handler = (handler ?? "").Trim().ToLowerInvariant();

        if (handler == "logout")
            return await DoLogoutAsync();

        if (!ModelState.IsValid)
        {
            Error = "Перевірте правильність введених даних.";
            return Page();
        }

        try
        {
            return handler switch
            {
                "register" => await DoRegisterAsync(),
                "login" => await DoLoginAsync(),
                _ => BadRequest("Unknown handler")
            };
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            return Page();
        }
    }

    private async Task<IActionResult> DoLoginAsync()
    {
        var token = await _gateway.LoginAsync(Email.Trim(), Password);
        await SignInWithTokenAsync(token);

        return Redirect(ReturnUrl ?? "/Hotels/Index");
    }

    private async Task<IActionResult> DoRegisterAsync()
    {
        var token = await _gateway.RegisterAsync(Email.Trim(), Password);
        await SignInWithTokenAsync(token);

        return Redirect(ReturnUrl ?? "/Hotels/Index");
    }

    private async Task<IActionResult> DoLogoutAsync()
    {
        _tokenStore.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToPage("/Account/Login");
    }

    private async Task SignInWithTokenAsync(string token)
    {
        // 1) сохранить JWT для gateway (session)
        _tokenStore.SetToken(token);
        Console.WriteLine("SESSION ID: " + HttpContext.Session.Id);
        Console.WriteLine("TOKEN LEN NOW: " + (HttpContext.Session.GetString("access_token")?.Length ?? 0));

        // 2) распарсить claims из JWT и создать cookie auth для Razor Pages
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var claims = new List<Claim>();

        // ✅ Сохраняем JWT прямо в cookie-claims (переживает рестарт Web)
        claims.Add(new Claim("access_token", token));

        // NameIdentifier
        var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        if (!string.IsNullOrWhiteSpace(sub))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, sub));

        // Email
        var email = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value
                    ?? jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                    ?? Email;

        if (!string.IsNullOrWhiteSpace(email))
            claims.Add(new Claim(ClaimTypes.Email, email));

        // Roles (из твоего токена — ClaimTypes.Role)
        foreach (var r in jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))
            claims.Add(new Claim(ClaimTypes.Role, r));

        // friendly name
        if (!string.IsNullOrWhiteSpace(email))
            claims.Add(new Claim(ClaimTypes.Name, email));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                RedirectUri = ReturnUrl ?? "/Hotels/Index"
            });
    }
}
