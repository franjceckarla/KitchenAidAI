
Cilj: zamijeniti lokalni `MockDataService` i `MockData/*` klase stvarnim EF Core pristupom koristeći `KitchenAidDbContext`.
---
name: Mock to EF Core Migration
description: Praktični vodič kako prebaciti projekt sa mock servisa na EF Core - zamijeniti MockDataService i MockData klase stvarnim EF Core pristupom koristeći KitchenAidDbContext
argument-hint: "Primjer: Kako prebaciti Korisnici kontroler na EF, Zašto EF umjesto mocka, Kako ažurirati DI registraciju"
tools: ['vscode', 'read', 'edit', 'search', 'semantic_search']
model: Claude Haiku 4.5 (copilot)
---

# Kako prebaciti projekt sa "mock" servisa na EF Core (praktični vodič)

Cilj: zamijeniti lokalni `MockDataService` i `MockData/*` klase stvarnim EF Core pristupom koristeći `KitchenAidDbContext`.

1. Pregled koda
- Pronađi postojeći mock servis (npr. `KitchenAidAI/Helpers/MockDataService.cs` i `Helpers/MockData/*`).
- Identificiraj gdje se servis koristi (kontroleri, view modeli, DI registracije).

2. Modeli i DbContext
- Provjeri da su modeli (`Models/*.cs`) kompatibilni s EF (javne `Id`/property, kolekcije kao `ICollection<T>`).
- Ako treba, prilagodi navigacijske property-je (npr. `public Frizider frizider { get; set; }`) i nullable reference tipove.
- U `KitchenAidDbContext` (ako već postoji) dodaj `DbSet<T>` za svaku entitet klasu, npr. `public DbSet<User> Users { get; set; }`.

3. Registracija servisa (DI)
- U `Program.cs` ukloni ili zamijeni registraciju mock servisa s registracijom DbContexta:
  - `builder.Services.AddDbContext<KitchenAidDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(8,0,36))));`
- Ukloni `builder.Services.AddScoped<MockDataService>();` ili slično.

4. Ažuriraj kontrolere
- Umjesto injektiranog `MockDataService`, injektiraj `KitchenAidDbContext` u konstruktore kontrolera:
  - `private readonly KitchenAidDbContext _dbContext;`
  - `public KorisniciController(KitchenAidDbContext dbContext) { _dbContext = dbContext; }`
- Zamijeni pozive prema mocku s EF query-ima, npr. `var users = _dbContext.Users.AsNoTracking().ToList();`.
- Koristi `Include` i `ThenInclude` za navigacijske kolekcije kad treba (vidi `FriziderController` primjer).

5. Seed podataka
- Ako ti treba inicijalni skup podataka, zadrži / prilagodi `DatabaseSeeder` koji koristi `KitchenAidDbContext`.
- Poziv za seed u `Program.cs` (ima u projektu): kreiraj scope i pozovi `seeder.SeedAsync()`.

6. Migracije i DB
- Dodaj migraciju (pogledaj migrations-skill.md) i primijeni je na DB.

7. Testiranje
- Pokreni aplikaciju i provjeri sve rute i stranice koje su ranije koristile mock.
- Obrati pažnju na razlike u ponašanju (npr. lazy loading, null vrijednosti).

Savjeti i često viđene prilagodbe
- Ako su DTO-ovi korišteni za view modele, zadrži ih i mapiraj iz entiteta (manje rizika od kružnih referenci u viewovima).
- Za performanse koristi `AsNoTracking()` kad vraćaš podatke samo za čitanje.
- Drži seed logiku idempotentnom (provjera postoji li zapis prije insert-a).

Reference u repo:
- `KitchenAidAI/Helpers/MockDataService.cs` (izvorno mjesto mocka)
- `KitchenAidAI/Data/KitchenAidDbContext.cs` (DB kontekst)
- `KitchenAidAI/Controllers/*` (mijenjati konstruktore i upite)

Ako želiš, mogu napraviti pull-request s primjerom konverzije jednog kontrolera (npr. `KorisniciController`) na EF implementaciju.