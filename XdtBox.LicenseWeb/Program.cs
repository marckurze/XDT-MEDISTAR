using Microsoft.AspNetCore.Authentication.Cookies;
using XdtBox.LicenseWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<LicenseWebOptions>(builder.Configuration.GetSection(LicenseWebOptions.SectionName));
builder.Services.AddSingleton<LicenseWebPasswordHasher>();
builder.Services.AddSingleton<LicenseWebAuthService>();
builder.Services.AddSingleton<LicenseWebDataStore>();
builder.Services.AddSingleton<LicenseWebLicenseService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "XDTBox.LicenseWeb";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

public partial class Program;
