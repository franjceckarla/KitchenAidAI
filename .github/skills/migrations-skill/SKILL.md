---
name: migrations-skill
description: "Vodic kako kreirati i koristiti EF Core migracije - stvaranje migracija, primjena na bazu, citanje generiranih fajlova, seed podaci i rjesavanje problema"
argument-hint: "Primjer: Kako dodati novu migraciju, Kako primijeniti migracije na bazu, Kako vratiti migraciju, Seed podaci i migracije"
---

# Migrations skill: kako kreirati i koristiti EF Core migracije u ovom projektu

Cilj: objasniti korake za stvaranje i primjenu migracija, citanje generiranih fajlova i rjesavanje uobicajenih problema.

Preduvjeti
- Instalirani EF Core CLI alati (`dotnet tool install --global dotnet-ef`) ili referenca u projektu.
- Ispravno postavljen connection string u `appsettings.json` (pogledaj `KitchenAidConnection`).

1. Dodavanje migracije
- U root folderu projekta (gdje se nalazi `.csproj`) pokreni:

```bash
dotnet ef migrations add InitialCreate -p KitchenAidAI --startup-project KitchenAidAI
```

- `-p` (project) pokazuje projekt koji sadrzi `DbContext` ako nije root; `--startup-project` definira projekt za konfiguraciju i DI.

2. Primjena migracije na bazu
- Primijeni migracije:

```bash
dotnet ef database update -p KitchenAidAI --startup-project KitchenAidAI
```

3. Sto migracija sadrzi
- U folderu `Migrations/` nalazi se fajl s timestamp imenom (`20260505144157_InitialCreate.cs`) i `ModelSnapshot`.
- Migration fajl sadrzi `Up()` i `Down()` metode koje stvaraju ili brisu tablice.

4. Seed podaci i migracije
- Seed logika se obicno izvodi nakon `app.Build()` u `Program.cs` koristeci `CreateScope()` i poziv na `DatabaseSeeder`.
- Seed je odvojen od migracija - migracije definiraju semu, seed ubacuje podatke.

5. Uobicajeni problemi
- Ako migracija ne moze detektirati promjene: provjeri da su sve entitet klase javne i da `DbSet` postoji u `DbContext`.
- Konflikti sa starim migracijama: razmotri `dotnet ef migrations remove` za zadnju migraciju i popravak modela.
- Ako koristis MySQL / Pomelo, provjeri kompatibilnost tipova i verziju servera koju koristis u `new MySqlServerVersion(...)`.

6. Pregled i revizija migracijskih fajlova
- Otvori `Migrations/<timestamp>_*.cs` da vidis SQL operacije koje ce biti izvrsene.
- `KitchenAidDbContextModelSnapshot.cs` sluzi EF-u da prati trenutni model za sljedece migracije.

7. Best practices
- Drzi male, semanticki jasne migracije (cesto commitis ih u VCS).
- Ne mijenjaj rucno vec primijenjene migracije bez razumijevanja posljedica.
- Za produkciju, testiraj migracije na staging bazi prije primjene.

Reference u repo:
- `KitchenAidAI/Migrations/20260505144157_InitialCreate.cs`
- `KitchenAidAI/Data/KitchenAidDbContext.cs`
- `Program.cs` (gdje se poziva seeder i registrira DbContext)

Ako zelis, mogu pokrenuti naredbe za stvaranje dodatne migracije (trebam potvrdu da zelis izvrsiti CLI naredbe ovdje).
