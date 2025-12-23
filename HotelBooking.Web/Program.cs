using System.Security.Claims;
using HotelBooking.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
});

// Session (JWT)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
    opt.IdleTimeout = TimeSpan.FromHours(8);
});

// Cookie auth (UI)
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/Account/Login";
        opt.AccessDeniedPath = "/Account/Login";
        opt.SlidingExpiration = true;
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
        opt.ReturnUrlParameter = "ReturnUrl";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// Token store (Session)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITokenStore, ClaimsTokenStore>();

// Gateway HttpClient + bearer
builder.Services.AddTransient<BearerTokenHandler>();

builder.Services.AddHttpClient<IGatewayClient, GatewayClient>((sp, http) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["Gateway:BaseUrl"] ?? throw new InvalidOperationException("Gateway:BaseUrl missing");
    http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
})
.AddHttpMessageHandler<BearerTokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();          // обязательно ДО auth
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (HttpContext ctx) =>
{
    ctx.Response.Redirect("/Hotels/Index");
    return Task.CompletedTask;
});

app.MapRazorPages();

app.Run();
