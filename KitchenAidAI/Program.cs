using KitchenAidAI.Data;
using KitchenAidAI.Helpers;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("KitchenAidConnection")
    ?? throw new InvalidOperationException("Connection string 'KitchenAidConnection' was not found.");

builder.Services.AddDbContext<KitchenAidDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

// Configure the HTTP request pipeline (development-only setup).
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

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
    name: "frizider-po-korisniku",
    pattern: "frizider/{userId:int}",
    defaults: new { controller = "Frizider", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
