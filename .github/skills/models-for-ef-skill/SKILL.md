---
name: models-for-ef-skill
description: "Prakticni vodic kako kreirati i mijenjati entitet modele da ih EF Core ispravno prepozna - konvencije, DbSet, relacije, navigacijska svojstva, data annotations i fluent API"
argument-hint: "Primjer: Kako postaviti primarni kljuc, Kako definirati relacije 1:N i N:N, Kako koristiti Fluent API, Kako testirati modele"
---

# Modeli za EF Core: kako ih kreirati i mijenjati da ih EF prepozna

Cilj: prakticne smjernice kako definirati i prilagoditi entitet klase tako da EF Core ispravno prepozna semu, veze i kljucna svojstva.

1. Osnovna pravila i konvencije
- Klase entiteta moraju biti `public` i imati javne get/set properties.
- Primarni kljuc po konvenciji: `Id` ili `<ClassName>Id`. Ako koristis drugacije ime, oznaci ga s `[Key]` ili konfiguriraj u `OnModelCreating`.
- UUID/GUID primarni kljucevi: tip `Guid` i mozes koristiti `ValueGeneratedOnAdd()` u konfiguraciji.

2. `DbSet<T>` u `DbContext`
- Dodaj `public DbSet<User> Users { get; set; }` za svaki entitet koji zelis ukljuciti u model.
- Bez `DbSet`-a EF i dalje moze prepoznati entitet ako ga referencira drugi entitet, ali eksplicitni `DbSet` je jasniji i pomaze alatima.

3. Relacije i navigacijska svojstva
- Jedan-na-vise: definiraj referencu i kolekciju:
  - `public int UserId { get; set; }`
  - `public User User { get; set; }`
  - `public ICollection<Namirnica> Namirnice { get; set; } = new List<Namirnica>();`
- Vise-na-vise: u EF Core 5+ mozes koristiti skip navigations; za kontrolu stvori eksplicitnu join entitet klasu.
- Koristi `Include`/`ThenInclude` pri dohvacanju povezanih podataka.

4. Foreign key polja i nullable
- Dodaj FK property (`int? FriziderId`) kad zelis eksplicitno kontrolirati kljuceve.
- Ako je referenca optional, koristi nullable FK (`int?`) ili nullable navigation property.

5. Data Annotations vs Fluent API
- Brzo: koristite atributne anotacije kao `[Required]`, `[MaxLength(200)]`, `[Column("col_name")]`.
- Za slozene scenarije (kompozitni kljucevi, owned types, indeksiranje) koristi `modelBuilder` u `OnModelCreating`.

6. Primjeri Fluent API
- Primarni kljuc:
  - `modelBuilder.Entity<User>().HasKey(u => u.id);`
- Konfiguracija veze 1:N:
  - `modelBuilder.Entity<Frizider>().HasOne(f => f.Owner).WithMany(u => u.Frizideri).HasForeignKey(f => f.OwnerId);`
- Slozeni/kompozitni kljuc:
  - `modelBuilder.Entity<RecipeIngredient>().HasKey(ri => new { ri.ReceptId, ri.NamirnicaId });`

7. Owned types i value objects
- Kada imas value object (npr. `NutritivnaVrijednost`), mozes ga mapirati kao owned:
  - `modelBuilder.Entity<Recept>().OwnsOne(r => r.NutritivnaVrijednost);`
- Owned types nemaju vlastiti `DbSet`.

8. Enumi i konverzije
- Enumi se mapiraju kao int po defaultu. Ako zelis string:
  - `modelBuilder.Entity<User>().Property(u => u.PreferencijaPrehrane).HasConversion<string>();`

9. Concurrency i verzioniranje
- Ako trebaš concurrency kontrolu, dodaj `byte[] RowVersion` i oznaci ga:
  - `[Timestamp] public byte[] RowVersion { get; set; }`

10. Mijenjanje modela nakon sto migracije postoje
- Dodaj/izmijeni entitet klase i pokreni novu migraciju (`dotnet ef migrations add ...`).
- Ako si rucno mijenjao postojece migracije koje su vec primijenjene u produkciji, oprez - bolje napraviti novu migraciju koja popravi stanje.

11. Cesto videne greske i rjesenja
- "Type X has no key defined" - provjeri ima li entitet primarni kljuc i/ili `DbSet`.
- Kruzne reference u JSON/razgledima - koristite ViewModel/DTO ili `AsNoTracking()` i mapiranje na DTO.
- Property je private set ili nema setter-a - EF treba pristup getter/setteru (ili koristi backing field konfiguraciju).

12. Testiranje modela lokalno
- Dodaj EF in-memory DB za unit testove (`UseInMemoryDatabase`) da testiras ponasanje bez stvarne baze.

13. Primjeri iz repo-a
- `KitchenAidAI/Models/*` - provjeri da su entiteti `public` i imaju predvidene FK i navigacije.
- `KitchenAidAI/Data/KitchenAidDbContext.cs` - gdje dodajes `DbSet` i `OnModelCreating`.
- `FriziderController` pokazuje primjer `Include` i `ThenInclude` za navigacijske kolekcije.

14. Koraci za promjenu modela (kratki checklist)
- 1) Napiši/izmijeni entitet klasu (`public` + `Id`).
- 2) Dodaj/izmijeni `DbSet<T>` u `KitchenAidDbContext`.
- 3) Ako treba, dodaj Fluent API u `OnModelCreating`.
- 4) Kreiraj novu migraciju i primijeni je.
- 5) Testiraj aplikaciju i seed.

Ako zelis, mogu:
- Generirati gotov primjer izmjene jednog modela u repo-u i dodati pripadajucu migraciju.
- Provjeriti postojece modele u `KitchenAidAI/Models` i predloziti tocne izmjene za sve koje nedostaju za EF.
