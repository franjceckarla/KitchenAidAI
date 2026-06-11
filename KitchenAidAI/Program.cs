using KitchenAidAI.Data;
using KitchenAidAI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
var authenticationBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    options.SlidingExpiration = false;
})
.AddCookie(IdentityConstants.ExternalScheme, options =>
{
    options.Cookie.Name = ".KitchenAidAI.External";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    options.SlidingExpiration = false;
});

// Register ASP.NET Core Identity Core (AppUser + roles)
builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole<int>>()
    .AddSignInManager()
    .AddEntityFrameworkStores<KitchenAidDbContext>()
    .AddDefaultTokenProviders();

var googleSection = builder.Configuration.GetSection("Authentication:Google");
if (!string.IsNullOrWhiteSpace(googleSection["ClientId"]) && !string.IsNullOrWhiteSpace(googleSection["ClientSecret"]))
{
    authenticationBuilder.AddGoogle(options =>
    {
        options.ClientId = googleSection["ClientId"]!;
        options.ClientSecret = googleSection["ClientSecret"]!;
        options.SignInScheme = IdentityConstants.ExternalScheme;
    });
}

var facebookSection = builder.Configuration.GetSection("Authentication:Facebook");
if (!string.IsNullOrWhiteSpace(facebookSection["AppId"]) && !string.IsNullOrWhiteSpace(facebookSection["AppSecret"]))
{
    authenticationBuilder.AddFacebook(options =>
    {
        options.AppId = facebookSection["AppId"]!;
        options.AppSecret = facebookSection["AppSecret"]!;
        options.SignInScheme = IdentityConstants.ExternalScheme;
    });
}

var microsoftSection = builder.Configuration.GetSection("Authentication:Microsoft");
if (!string.IsNullOrWhiteSpace(microsoftSection["ClientId"]) && !string.IsNullOrWhiteSpace(microsoftSection["ClientSecret"]))
{
    authenticationBuilder.AddMicrosoftAccount(options =>
    {
        options.ClientId = microsoftSection["ClientId"]!;
        options.ClientSecret = microsoftSection["ClientSecret"]!;
        options.SignInScheme = IdentityConstants.ExternalScheme;
    });
}
var connectionString = builder.Configuration.GetConnectionString("KitchenAidConnection")
    ?? throw new InvalidOperationException("Connection string 'KitchenAidConnection' was not found.");

builder.Services.AddDbContext<KitchenAidDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

// Configure the HTTP request pipeline (development-only setup).
app.UseStaticFiles();

var supportedCultures = new[] { "hr-HR", "en-US", "en-GB", "de-DE", "fr-FR", "es-ES" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("hr-HR")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "korisnici",
    pattern: "korisnici",
    defaults: new { controller = "Korisnici", action = "Index" });

app.MapControllerRoute(
    name: "recepti",
    pattern: "recepti",
    defaults: new { controller = "Recepti", action = "Index" });

app.MapControllerRoute(
    name: "korisnici-create",
    pattern: "korisnici/novi",
    defaults: new { controller = "Korisnici", action = "Create" });

app.MapControllerRoute(
    name: "korisnici-edit",
    pattern: "korisnici/uredi/{id:int}",
    defaults: new { controller = "Korisnici", action = "Edit" });

app.MapControllerRoute(
    name: "recepti-create",
    pattern: "recepti/novi",
    defaults: new { controller = "Recepti", action = "Create" });

app.MapControllerRoute(
    name: "recepti-edit",
    pattern: "recepti/uredi/{id:int}",
    defaults: new { controller = "Recepti", action = "Edit" });

app.MapControllerRoute(
    name: "namirnice-create",
    pattern: "namirnice/novi/{friziderId:int}",
    defaults: new { controller = "Namirnice", action = "Create" });

app.MapControllerRoute(
    name: "namirnice-edit",
    pattern: "namirnice/uredi/{id:int}",
    defaults: new { controller = "Namirnice", action = "Edit" });

app.MapControllerRoute(
    name: "crud-regex",
    pattern: "{controller:regex(^(Korisnici|Recepti|Namirnice)$)}/{action:regex(^(Create|Edit)$)}/{id:int?}");

app.MapControllerRoute(
    name: "kuharice",
    pattern: "kuharice",
    defaults: new { controller = "Kuharice", action = "Index" });

app.MapControllerRoute(
    name: "datoteke",
    pattern: "datoteke",
    defaults: new { controller = "Datoteke", action = "Index" });

app.MapControllerRoute(
    name: "frizider-po-korisniku",
    pattern: "frizider/{userId:int}",
    defaults: new { controller = "Frizider", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    if (!app.Environment.IsEnvironment("Testing"))
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
}

app.Run();

public partial class Program { }
