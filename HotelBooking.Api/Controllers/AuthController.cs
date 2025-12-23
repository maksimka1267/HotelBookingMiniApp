using HotelBooking.Data.Infrastructure.Identity;
using HotelBooking.Data.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _cfg;

    public AuthController(UserManager<AppUser> userManager, IConfiguration cfg)
    {
        _userManager = userManager;
        _cfg = cfg;
    }

    public sealed record RegisterRequest(string Email, string Password);
    public sealed record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var email = req.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email/Password required");

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null)
            return BadRequest("User already exists");

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var created = await _userManager.CreateAsync(user, req.Password);
        if (!created.Succeeded)
            return BadRequest(created.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, DbSeeder.RoleClient);

        var token = await IssueToken(user);
        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var email = req.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email/Password required");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Unauthorized("Invalid credentials");

        var ok = await _userManager.CheckPasswordAsync(user, req.Password);
        if (!ok)
            return Unauthorized("Invalid credentials");

        var token = await IssueToken(user);
        return Ok(new { token });
    }
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            isAuth = User.Identity?.IsAuthenticated,
            nameId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            sub = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
        });
    }

    private async Task<string> IssueToken(AppUser user)
    {
        var key = _cfg["Jwt:Key"]!;
        var issuer = _cfg["Jwt:Issuer"]!;
        var audience = _cfg["Jwt:Audience"]!;
        var expireMinutes = int.TryParse(_cfg["Jwt:ExpireMinutes"], out var m) ? m : 120;

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? "")
        };

        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
}
