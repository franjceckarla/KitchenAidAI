# Detaljno objašnjenje autentikacije nakon prenamjene

Ovaj dokument opisuje kako autentikacija sada funkcionira u aplikaciji nakon prelaska na ASP.NET Core Identity, sto je ostalo od starog sustava zbog kompatibilnosti, i po cemu se novi pristup razlikuje od prethodne verzije.

## 1) Kratki pregled novog sustava

Nova autentikacija je slojena ovako:

1. Identity je primarni sustav za prijavu, registraciju i uloge.
2. `AppUser` je glavni korisnicki model i nasljeduje Identity korisnika.
3. `KitchenAidDbContext` je prebacen na `IdentityDbContext<AppUser, IdentityRole<int>, int>`.
4. `Program.cs` registrira Identity, cookie autentikaciju i vanjske providere.
5. `AuthController` koristi `UserManager<AppUser>` i `SignInManager<AppUser>`.
6. `AuthSession` je zadrzan samo kao tehnicki helper-naziv, ali cita iskljucivo Identity principal (bez session fallbacka).

Najbitniji ulazni fajlovi su:
- [KitchenAidAI/Models/AppUser.cs](../KitchenAidAI/Models/AppUser.cs)
- [KitchenAidAI/Data/KitchenAidDbContext.cs](../KitchenAidAI/Data/KitchenAidDbContext.cs#L8)
- [KitchenAidAI/Controllers/AuthController.cs](../KitchenAidAI/Controllers/AuthController.cs)
- [KitchenAidAI/Helpers/AuthSession.cs](../KitchenAidAI/Helpers/AuthSession.cs)
- [KitchenAidAI/Filters/RequireSessionAttribute.cs](../KitchenAidAI/Filters/RequireSessionAttribute.cs#L8)
- [KitchenAidAI/Program.cs](../KitchenAidAI/Program.cs#L38)

## 2) Kako radi prijava

Kod prijave korisnik prolazi kroz klasicni Identity flow:

1. Forma salje podatke u `AuthController.Login`.
2. Provjerava se valjanost modela.
3. `SignInManager.PasswordSignInAsync(...)` radi provjeru korisnickog imena i lozinke.
4. Ako je prijava uspjesna, Identity application cookie postaje jedini izvor auth stanja.
5. Nakon toga se korisnik preusmjerava na `Home/Index`.

Relevantan dio koda je u [KitchenAidAI/Controllers/AuthController.cs](../KitchenAidAI/Controllers/AuthController.cs#L156).

Zasto postoji `AuthSession` helper iako je auth sada Identity-only?
- Dio kontrolera i dalje poziva isti helper API (`GetUserId`, `IsAdmin`) radi manjeg refaktora po fajlu.
- Helper vise ne upisuje i ne cita session; koristi samo Identity claimove i role.

## 3) Kako radi registracija

Registracija je sada Identity-based, a ne rucno upisivanje u legacy `User` tablicu.

Tijek je ovakav:

1. `AuthController.Register` validira formu i dodatna pravila za prehrambene preferencije.
2. Provjerava se jedinstvenost usernamea i emaila preko `UserManager`.
3. Kreira se novi `AppUser`.
4. `UserManager.CreateAsync(appUser, password)` sprema korisnika i hash lozinke.
5. Korisnik se dodaje u ulogu `User`.
6. `SignInManager.SignInAsync(...)` ga odmah prijavljuje.
7. Nakon uspjesne registracije Identity cookie je jedini izvor auth stanja.

Povezani dio je u [KitchenAidAI/Controllers/AuthController.cs](../KitchenAidAI/Controllers/AuthController.cs#L131).

Razlika u odnosu na staru verziju:
- prije se koristio vlastiti `PasswordHasher<User>` i manualni upis u `_dbContext.Users`;
- sada Identity sam cuva korisnika, hash i uloge.

## 4) Kako radi Google, Facebook i Microsoft login

Vanjske prijave rade preko Identity external login mehanizma.

Tijek je ovakav:

1. Korisnik klikne Google/Facebook/Microsoft gumb.
2. `AuthController.ExternalLogin(...)` pokrece `Challenge(...)`.
3. Provider vraca korisnika na callback akciju.
4. `SignInManager.GetExternalLoginInfoAsync()` cita vanjski login info.
5. Ako postoji povezani korisnik, `ExternalLoginSignInAsync(...)` ga prijavljuje.
6. Ako korisnik ne postoji, kreira se novi `AppUser`, povezuje se vanjski login i odmah prijavljuje.
7. Nakon uspjeha postavlja se Identity application cookie.

Ključni dio je u [KitchenAidAI/Controllers/AuthController.cs](../KitchenAidAI/Controllers/AuthController.cs#L79).

`Program.cs` registrira providere i odvojeni external cookie:
- [KitchenAidAI/Program.cs](../KitchenAidAI/Program.cs#L24)
- [KitchenAidAI/Program.cs](../KitchenAidAI/Program.cs#L30)
- [KitchenAidAI/Program.cs](../KitchenAidAI/Program.cs#L53)
- [KitchenAidAI/Program.cs](../KitchenAidAI/Program.cs#L64)
- [KitchenAidAI/Program.cs](../KitchenAidAI/Program.cs#L75)

## 5) Logout

Odjava sada radi kroz Identity cookie sloj:

1. `SignInManager.SignOutAsync()` odjavljuje Identity application cookie.
2. External cookie se isto ocisti.

To je u [KitchenAidAI/Controllers/AuthController.cs](../KitchenAidAI/Controllers/AuthController.cs#L306).

## 6) Uloge i autorizacija

Identity sada ima stvarne role:

- `Admin`
- `User`

Te role se seedaju pri pokretanju aplikacije:
- [KitchenAidAI/Data/DatabaseSeeder.cs](../KitchenAidAI/Data/DatabaseSeeder.cs#L40)
- [KitchenAidAI/Data/DatabaseSeeder.cs](../KitchenAidAI/Data/DatabaseSeeder.cs#L61)
- [KitchenAidAI/Data/DatabaseSeeder.cs](../KitchenAidAI/Data/DatabaseSeeder.cs#L64)

`RequireSessionAttribute` i dalje cuva postojece stranice, ali koristi iskljucivo Identity principal (bez session auth stanja):
- [KitchenAidAI/Helpers/AuthSession.cs](../KitchenAidAI/Helpers/AuthSession.cs)
- [KitchenAidAI/Filters/RequireSessionAttribute.cs](../KitchenAidAI/Filters/RequireSessionAttribute.cs#L8)

## 7) Baza i migracije

Identity shema je dodana kroz migraciju:

- [KitchenAidAI/Migrations/20260608144843_AddIdentitySchema.cs](../KitchenAidAI/Migrations/20260608144843_AddIdentitySchema.cs#L10)
- [KitchenAidAI/Migrations/20260608144843_AddIdentitySchema.Designer.cs](../KitchenAidAI/Migrations/20260608144843_AddIdentitySchema.Designer.cs#L14)

Ova migracija stvara Identity tablice poput `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` i ostalih povezanih tablica.

## 8) Razlika prema prethodnoj verziji autentikacije

Stari model:

- autentikacija je bila custom i oslanjala se na vlastiti `User` model;
- password se hashirao rucno preko `PasswordHasher<User>`;
- login je popunjavao session direktno kroz `AuthSession`;
- uloge su bile implicitne kroz polje `isAdmin`;
- vanjska prijava nije bila dio Identity user/link modela;
- najveci dio autorizacije je citao `AuthSession` i legacy tablice.

Novi model:

- Identity je izvor istine za login i korisnicke role;
- `AppUser` je glavni auth korisnik;
- `SignInManager` i `UserManager` rade prijavu, registraciju i linking vanjskih racuna;
- `Admin` i `User` su prave Identity role;
- helper pozivi za auth rade nad Identity claimovima i role-ovima;
- baza ima standardne `AspNet*` tablice;
- vanjske prijave se mogu vezati uz Identity korisnika.

Najpraktičnija razlika u ponašanju:

- prije je aplikacija morala znati o vlastitoj sesiji i custom korisnickoj tablici;
- sada auth stanje dolazi iskljucivo iz Identity cookie/claim modela.

## 9) Sto je jos ostalo kao kompatibilnost

Ovo je namjerno ostavljeno:

- legacy `User` tablica jos postoji zbog domenskih podataka i postepenog ciscenja;
- dio kontrolera jos koristi helper pozive `AuthSession.IsAdmin(...)` i `AuthSession.GetUserId(...)`, ali oni sada citaju Identity principal;
- `ViewBag.IsAdmin` i dalje se puni radi postojecih view layout odluka.

To znaci da je auth model sada Identity-only, dok je dio naziva/helper API-ja zadrzan radi kompatibilnosti koda.

## 10) Koliko bi velika bila potpuna zamjena na samo Identity

Nakon zadnje migracije auth runtime je vec Identity-only. Preostala promjena bi sada bila uglavnom ciscenje naziva i helper poziva.

Otprilike bi trebalo dotaknuti:

- `AuthSession` se moze potpuno izbaciti i zamijeniti direktnim `User`/claims citanjem po kontrolerima.
- svi kontroleri koji sada citaju `AuthSession.IsAdmin(...)` i `AuthSession.GetUserId(...)` trebali bi preci na `User`, claimove i role.
- svi viewovi koji oslanjaju na `ViewBag.IsAdmin` mogu se prebaciti na `User.Identity` i `User.IsInRole(...)`.
- legacy `User` tablica i svi dijelovi koji je jos citaju trebali bi se ili migrirati ili potpuno maknuti.
- `DatabaseSeeder` bi trebalo pojednostaviti da radi samo Identity seed.
- svaka autorizacijska logika u MVC i API sloju trebala bi dobiti jasne `[Authorize]` / `[Authorize(Roles = "Admin")]` attribute umjesto rucnih provjera.

Prakticno, ovo je promjena koja bi dirala dobar dio aplikacije, ali ne zato sto je kompleksna u jednoj tocki, nego zato sto je rasprsena kroz vise slojeva: auth, viewovi, session bridge, seeder i autorizacijske provjere.

Gruba procjena:

- ako se radi samo cleanup helper naziva i poziva, to je nekoliko sati do jedan radni dan;
- ako se uz to zeli potpuno ukinuti legacy `User` model i vezane domenske veze, realnije je vise dana rada.

## 11) Brzi popis promijenjenih fajlova

- [KitchenAidAI/Controllers/AuthController.cs](../KitchenAidAI/Controllers/AuthController.cs)
- [KitchenAidAI/Helpers/AuthSession.cs](../KitchenAidAI/Helpers/AuthSession.cs)
- [KitchenAidAI/Filters/RequireSessionAttribute.cs](../KitchenAidAI/Filters/RequireSessionAttribute.cs)
- [KitchenAidAI/Models/AppUser.cs](../KitchenAidAI/Models/AppUser.cs)
- [KitchenAidAI/Data/KitchenAidDbContext.cs](../KitchenAidAI/Data/KitchenAidDbContext.cs)
- [KitchenAidAI/Data/DatabaseSeeder.cs](../KitchenAidAI/Data/DatabaseSeeder.cs)
- [KitchenAidAI/Program.cs](../KitchenAidAI/Program.cs)
- [KitchenAidAI/Migrations/20260608144843_AddIdentitySchema.cs](../KitchenAidAI/Migrations/20260608144843_AddIdentitySchema.cs)
- [KitchenAidAI/Migrations/20260608144843_AddIdentitySchema.Designer.cs](../KitchenAidAI/Migrations/20260608144843_AddIdentitySchema.Designer.cs)

Ako zelis, mogu napraviti i drugu verziju ovog dokumenta u formatu "za kolegij / seminarski rad", dakle vise narativno i manje tehnicki, ili mogu dodati mini dijagram toka prijave i registracije.