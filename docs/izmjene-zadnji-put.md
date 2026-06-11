# Dokumentacija izmjena (posljednje izmjene)

Ovaj zapis sazima sve zadnje vece preinake u aplikaciji: upload datoteka, prijelaz na ASP.NET Core Identity, vanjske prijave, API dorade i pripadajuca UI/validacijska poboljsanja.

## 1) ASP.NET Core Identity i autentikacija
- Dodan je `AppUser` model i Identity bazni sloj kroz `KitchenAidDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>`.
- Identity je registriran u `Program.cs` zajedno s cookie autentikacijom i vanjskim providerima.
- `AuthController` je prebacen na `UserManager<AppUser>` i `SignInManager<AppUser>` za:
  - login,
  - registraciju,
  - vanjske prijave,
  - odjavu.
- Registracija sada kreira Identity korisnika, dodaje ga u ulogu `User` i automatski prijavljuje.
- Ulogu `Admin` i pocetnog admin korisnika sada seed-a `DatabaseSeeder`.
- Dodana je Identity migracija `AddIdentitySchema` koja stvara `AspNetUsers`, `AspNetRoles` i povezane tablice.
- `AuthSession` je zadrzan kao kompatibilnosni sloj, ali sada moze citati podatke i iz Identity principal-a ako session nije popunjen.
- `RequireSessionAttribute` i dalje cuva postojece stranice, ali se oslanja na novi auth sloj kroz `AuthSession`.

## 2) Vanjske prijave
- Dodana je podrska za Google login u `Program.cs`.
- `AuthController` koristi `Challenge(...)` za pokretanje vanjske prijave i `ExternalLoginCallback` za dovrsetak.
- Vanjski login sada stvara ili povezuje Identity korisnika, umjesto starog rucnog auth flowa.
- Google callback je uskladjen s lokalnim redirect URI-jem koji je registriran u provider konzoli.
- Konfiguracija provider kljuceva ostaje u `appsettings.json` / `appsettings.Development.json`.

## 3) Upload datoteka
- Implementiran je upload datoteka na disk uz spremanje metapodataka u bazu.
- Dodan je `Datoteka` model, API i MVC prikaz za listanje i upravljanje datotekama.
- Upload je vezan uz prijavljenog korisnika i koristi postojece auth podatke.
- Dodana je soft-delete logika za datoteke.
- UI je nadogradjen tako da korisnik moze otvoriti upload iz aplikacije, a admin vidi prosireni pregled i filtriranje.
- AJAX osvjezavanje liste datoteka omogucuje brzi pregled bez punog reload-a stranice.

## 4) API dorade i MVC -> API remap
- Dio MVC kontrolera je preusmjeren na server-side API pozive preko `MvcApiControllerBase`.
- Dodani su ili doradjeni API kontroleri za CRUD i pregled podataka.
- Uveden je DTO sloj za API odgovore kako bi MVC prikaz mogao ostati isti, a data shape biti kontroliran.
- Dodano je filtriranje, sortiranje i search ponasanje na listama gdje je to bilo potrebno.
- API response helperi koriste se za konzistentne poruke o uspjehu i gresci.

## 5) Validacija i UI
- Zadrsana je server-side i client-side validacija za registraciju, prijavu i CRUD forme.
- Date picker i lokalizacija ostaju povezani s hrvatskim formatom datuma.
- AJAX pretrage, autocomplete polja i klik na red tablice i dalje rade kroz postojece `site.js` skripte i partiale.

## 6) Trenutno stanje i bitne napomene
- Legacy `User` tablica je jos uvijek prisutna zbog kompatibilnosti i postupnog prijelaza.
- Novi Identity sloj je sada primarni auth model za prijavu i vanjske provider-e.
- Vecina postojece aplikacije i dalje moze raditi preko `AuthSession` bridge-a dok se ne prepišu sve autorizacijske provjere na `User`/roles.
- Aplikacija se uspjesno builda nakon Identity integracije.

## 7) Korisni ulazni fileovi
- [KitchenAidAI/Controllers/AuthController.cs](../KitchenAidAI/Controllers/AuthController.cs)
- [KitchenAidAI/Helpers/AuthSession.cs](../KitchenAidAI/Helpers/AuthSession.cs)
- [KitchenAidAI/Data/KitchenAidDbContext.cs](../KitchenAidAI/Data/KitchenAidDbContext.cs)
- [KitchenAidAI/Data/DatabaseSeeder.cs](../KitchenAidAI/Data/DatabaseSeeder.cs)
- [KitchenAidAI/Program.cs](../KitchenAidAI/Program.cs)
- [KitchenAidAI/Migrations/20260608144843_AddIdentitySchema.cs](../KitchenAidAI/Migrations/20260608144843_AddIdentitySchema.cs)
- [KitchenAidAI/Migrations/20260608144843_AddIdentitySchema.Designer.cs](../KitchenAidAI/Migrations/20260608144843_AddIdentitySchema.Designer.cs)

Ako zelis, mogu u sljedecem koraku dodatno raspisati kratki "sto je sve promijenjeno po folderima" ili napraviti tocno upute za testiranje login/register/upload toka.