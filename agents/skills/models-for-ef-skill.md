
Cilj: praktične smjernice kako definirati i prilagoditi entitet klase tako da EF Core ispravno prepozna šemu, veze i ključna svojstva.
---
name: EF Core Models
description: Praktični vodič kako kreirati i mijenjati entitet modele da ih EF Core ispravno prepozna - konvencije, DbSet, relacije, navigacijska svojstva, data annotations i fluent API
argument-hint: "Primjer: Kako postaviti primarni ključ, Kako definirati relacije 1:N i N:N, Kako koristiti Fluent API, Kako testirati modele"
tools: ['vscode', 'read', 'edit', 'search']
model: Claude Haiku 4.5 (copilot)
---

# Modeli za EF Core: kako ih kreirati i mijenjati da ih EF prepozna

Cilj: praktične smjernice kako definirati i prilagoditi entitet klase tako da EF Core ispravno prepozna šemu, veze i ključna svojstva.

1. Osnovna pravila i konvencije
- Klase entiteta moraju biti `public` i imati javne get/set properties.
- Primarni ključ po konvenciji: `Id` ili `<ClassName>Id`. Ako koristiš drugačije ime, označi ga s `[Key]` ili konfiguriraj u `OnModelCreating`.
- UUID/GUID primarni ključevi: tip `Guid` i možeš koristiti `ValueGeneratedOnAdd()` u konfiguraciji.

2. `DbSet<T>` u `DbContext`
- Dodaj `public DbSet<User> Users { get; set; }` za svaki entitet koji želiš uključiti u model.
- Bez `DbSet`-a EF i dalje može prepoznati entitet ako ga referencira drugi entitet, ali eksplicitni `DbSet` je jasniji i pomaže alatima.

3. Relacije i navigacijska svojstva
- Jedan-na-više: definiraj referencu i kolekciju:
  - `public int UserId { get; set; }`
  - `public User User { get; set; }`
  - `public ICollection<Namirnica> Namirnice { get; set; } = new List<Namirnica>();`
- Više-na-više: u EF Core 5+ možeš koristiti skip navigations; za kontrolu stvori eksplicitnu join entitet klasu.
- Koristi `Include`/`ThenInclude` pri dohvaćanju povezanih podataka.

4. Foreign key polja i nullable
- Dodaj FK property (`int? FriziderId`) kad želiš eksplicitno kontrolirati ključeve.
- Ako je referenca optional, koristi nullable FK (`int?`) ili nullable navigation property.

5. Data Annotations vs Fluent API
- Brzo: koristite atributne anotacije kao `[Required]`, `[MaxLength(200)]`, `[Column("col_name")]`.
- Za složene scenarije (kompozitni ključevi, owned types, indeksiranje) koristi `modelBuilder` u `OnModelCreating`.

6. Primjeri Fluent API
- Primarni ključ:
  - `modelBuilder.Entity<User>().HasKey(u => u.id);`
- Konfiguracija veze 1:N:
  - `modelBuilder.Entity<Frizider>().HasOne(f => f.Owner).WithMany(u => u.Frizideri).HasForeignKey(f => f.OwnerId);`
- Složeni/kompozitni ključ:
  - `modelBuilder.Entity<RecipeIngredient>().HasKey(ri => new { ri.ReceptId, ri.NamirnicaId });`

7. Owned types i value objects
- Kada imaš value object (npr. `NutritivnaVrijednost`), možeš ga mapirati kao owned:
  - `modelBuilder.Entity<Recept>().OwnsOne(r => r.NutritivnaVrijednost);`
- Owned types nemaju vlastiti `DbSet`.

8. Enumi i konverzije
- Enumi se mapiraju kao int po defaultu. Ako želiš string:
  - `modelBuilder.Entity<User>().Property(u => u.PreferencijaPrehrane).HasConversion<string>();`

9. Concurrency i verzioniranje
- Ako trebaš concurrency kontrolu, dodaj `byte[] RowVersion` i označi ga:
  - `[Timestamp] public byte[] RowVersion { get; set; }`

10. Mijenjanje modela nakon što migracije postoje
- Dodaj/izmijeni entitet klase i pokreni novu migraciju (`dotnet ef migrations add ...`).
- Ako si ručno mijenjao postojeće migracije koje su već primijenjene u produkciji, oprez — bolje napraviti novu migraciju koja popravi stanje.

11. Često viđene greške i rješenja
- "Type X has no key defined" — provjeri ima li entitet primarni ključ i/ili `DbSet`.
- Kružne reference u JSON/razgledima — koristite ViewModel/DTO ili `AsNoTracking()` i mapiranje na DTO.
- Property je private set ili nema setter-a — EF treba pristup getter/setteru (ili koristi backing field konfiguraciju).

12. Testiranje modela lokalno
- Dodaj EF in-memory DB za unit testove (`UseInMemoryDatabase`) da testiraš ponašanje bez stvarne baze.

13. Primjeri iz repo-a
- `KitchenAidAI/Models/*` — provjeri da su entiteti `public` i imaju predviđene FK i navigacije.
- `KitchenAidAI/Data/KitchenAidDbContext.cs` — gdje dodaješ `DbSet` i `OnModelCreating`.
- `FriziderController` pokazuje primjer `Include` i `ThenInclude` za navigacijske kolekcije.

14. Koraci za promjenu modela (kratki checklist)
- 1) Napiši/izmijeni entitet klasu (`public` + `Id`).
- 2) Dodaj/izmijeni `DbSet<T>` u `KitchenAidDbContext`.
- 3) Ako treba, dodaj Fluent API u `OnModelCreating`.
- 4) Kreiraj novu migraciju i primijeni je.
- 5) Testiraj aplikaciju i seed.

Ako želiš, mogu:
- Generirati gotov primjer izmjene jednog modela u repo-u i dodati pripadajuću migraciju.
- Provjeriti postojeće modele u `KitchenAidAI/Models` i predložiti točne izmjene za sve koje nedostaju za EF.