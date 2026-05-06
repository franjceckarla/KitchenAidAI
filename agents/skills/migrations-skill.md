
Cilj: objasniti korake za stvaranje i primjenu migracija, čitanje generiranih fajlova i rješavanje uobičajenih problema.
---
name: EF Core Migrations
description: Vodič kako kreirati i koristiti EF Core migracije - stvaranje migracija, primjena na bazu, čitanje generiranih fajlova, seed podaci i rješavanje problema
argument-hint: "Primjer: Kako dodati novu migraciju, Kako primjeniti migracije na bazu, Kako vratiti migraciju, Seed podaci i migracije"
tools: ['vscode', 'run_in_terminal', 'read', 'edit', 'search']
model: Claude Haiku 4.5 (copilot)
---

# Migrations skill: kako kreirati i koristiti EF Core migracije u ovom projektu

Cilj: objasniti korake za stvaranje i primjenu migracija, čitanje generiranih fajlova i rješavanje uobičajenih problema.

Preduvjeti
- Instalirani EF Core CLI alati (`dotnet tool install --global dotnet-ef`) ili referenca u projektu.
- Ispravno postavljen connection string u `appsettings.json` (pogledaj `KitchenAidConnection`).

1. Dodavanje migracije
- U root folderu projekta (gdje se nalazi `.csproj`) pokreni:

```bash
dotnet ef migrations add InitialCreate -p KitchenAidAI --startup-project KitchenAidAI
```

- `-p` (project) pokazuje projekt koji sadrži `DbContext` ako nije root; `--startup-project` definira projekt za konfiguraciju i DI.

2. Primjena migracije na bazu
- Primijeni migracije:

```bash
dotnet ef database update -p KitchenAidAI --startup-project KitchenAidAI
```

3. Što migracija sadrži
- U folderu `Migrations/` nalazi se fajl s timestamp imenom (`20260505144157_InitialCreate.cs`) i `ModelSnapshot`.
- Migration fajl sadrži `Up()` i `Down()` metode koje stvaraju ili brišu tablice.

4. Seed podaci i migracije
- Seed logika se obično izvodi nakon `app.Build()` u `Program.cs` koristeći `CreateScope()` i poziv na `DatabaseSeeder`.
- Seed je odvojen od migracija — migracije definiraju šemu, seed ubacuje podatke.

5. Uobičajeni problemi
- Ako migracija ne može detektirati promjene: provjeri da su sve entitet klase javne i da `DbSet` postoji u `DbContext`.
- Konflikti sa starim migracijama: razmotri `dotnet ef migrations remove` za zadnju migraciju i popravak modela.
- Ako koristiš MySQL / Pomelo, provjeri kompatibilnost tipova i verziju servera koju koristiš u `new MySqlServerVersion(...)`.

6. Pregled i revizija migracijskih fajlova
- Otvori `Migrations/<timestamp>_*.cs` da vidiš SQL operacije koje će biti izvršene.
- `KitchenAidDbContextModelSnapshot.cs` služi EF-u da prati trenutni model za sljedeće migracije.

7. Best practices
- Drži male, semantički jasne migracije (često commitiš ih u VCS).
- Ne mijenjaj ručno već primijenjene migracije bez razumijevanja posljedica.
- Za produkciju, testiraj migracije na staging bazi prije primjene.

Reference u repo:
- `KitchenAidAI/Migrations/20260505144157_InitialCreate.cs`
- `KitchenAidAI/Data/KitchenAidDbContext.cs`
- `Program.cs` (gdje se poziva seeder i registrira DbContext)

Ako želiš, mogu pokrenuti naredbe za stvaranje dodatne migracije (trebam potvrdu da želiš izvršiti CLI naredbe ovdje).