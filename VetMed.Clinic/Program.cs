using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using VetMed.Clinic.Components;
using VetMed.Clinic.Services;
using VetMed.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connStr = builder.Configuration.GetConnectionString("DefaultConnection")!;
builder.Services.AddInfrastructure(connStr);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IClinicService, ClinicService>();
builder.Services.AddScoped<StaffAuthService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = "/login";
        opt.AccessDeniedPath = "/login";
        opt.ExpireTimeSpan = TimeSpan.FromHours(8);
        opt.SlidingExpiration = true;
        opt.Cookie.Name = "VetMed.Clinic.Auth";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthStateProvider>();

var app = builder.Build();

// Za reverse-proxy (Cloud Run) odczytaj oryginalny schemat/host z nagłówków X-Forwarded-*
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
};
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/auth/login", async (HttpContext ctx, StaffAuthService auth) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var staff = await auth.ValidateAsync(form["email"].ToString(), form["password"].ToString());
    var returnUrl = form["returnUrl"].ToString();

    if (staff is null)
        return Results.Redirect($"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl)}");

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, staff.Id.ToString()),
        new(ClaimTypes.Name, staff.FullName),
        new(ClaimTypes.Email, staff.Email),
        new(ClaimTypes.Role, "Admin"),
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

    return Results.Redirect(string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/') ? "/" : returnUrl);
}).DisableAntiforgery();

app.MapPost("/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
}).DisableAntiforgery();

var port = Environment.GetEnvironmentVariable("PORT") ?? "5278";
app.Run($"http://0.0.0.0:{port}");
