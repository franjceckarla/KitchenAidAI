---
name: api-integration-testing-skill
description: 'KitchenAidAI-specific vodic za kvalitetne ASP.NET Core API integracijske testove (xUnit + WebApplicationFactory + InMemory). Koristi kada treba pokriti sve CRUD operacije, autentikaciju/autorizaciju, validaciju, edge caseove, soft-delete/restore i response body strukturu uz status kodove.'
argument-hint: 'Koji kontroler i koje CRUD akcije zelis pokriti (npr. UsersApiController: GET/POST/PUT/DELETE/RESTORE)?'
user-invocable: true
---

# API Integration Testing Skill (KitchenAidAI)

## Kada koristiti
- Kada dodajes nove integration testove za API kontroler.
- Kada zelis standardizirati testove za CRUD + auth + validaciju.
- Kada zelis pouzdan obrazac koji je vec dokazan na Countries i Users testovima.
- Kada zelis testove koji su specificni za KitchenAidAI ponasanja (soft-delete, restore admin prava, owner-vs-admin pravila).

## Ishod
- Test class sa jasnim i ponovljivim scenarijima.
- Pokriveni auth i authorization rubni slucajevi.
- Pokriveni validacijski i not-found scenariji.
- CRUD testovi koji provjeravaju status kod i body strukturu (success/data/alert).
- Pokriveni edge caseovi i duplicate scenariji gdje poslovna pravila to zahtijevaju.

## KitchenAidAI-specific pravila (obavezno)
1. Integracijski testovi moraju pokriti API endpointe za sve CRUD operacije koje kontroler izlaže.
2. Moraju postojati testovi za uspjesne scenarije (barem jedan 2xx po operaciji gdje je primjenjivo).
3. Moraju postojati testovi za nepostojece ID-eve (404 gdje je primjenjivo).
4. Moraju postojati testovi za validacijske pogreske gdje postoje pravila (400).
5. Za DELETE koji radi soft-delete mora se potvrditi soft-delete ponasanje, ne samo status kod.
6. Za RESTORE mora se testirati da je admin dozvoljen, a non-admin zabranjen.
7. Test mora validirati response body strukturu uz status kod:
  - success
  - data (za uspjeh)
  - alert i alert.code (za greske, kada je dostupno)
8. Za entitete s jedinstvenim pravilima (npr. Users, Countries) mora postojati duplicate test koji potvrduje da unos duplikata nije moguc.

## Obavezni preduvjeti
- WebApplicationFactory custom factory koristi Testing environment.
- DbContext registracije su zamijenjene InMemory providerom.
- InMemory database name je stabilan po factory instanci (ne novi GUID po requestu).
- Test auth handler podrzava header-based identity:
  - X-Test-UserId
  - X-Test-Role

## Postupak

### 1. Pripremi test infrastrukturu
1. Koristi zajednicki factory i helper za autorizirani client.
2. Za neautorizirani scenario koristi client bez auth headera.
3. Za autorizirani scenario koristi helper koji postavlja user id i role header.

### 2. Napravi bazni auth test set za svaki kontroler
1. Bez autentikacije -> ocekuj 401 Unauthorized.
2. Ulogiran user bez prava -> ocekuj 403 Forbidden na admin-only akciji.
3. Ulogiran admin -> ocekuj 2xx uspjeh na admin-only akciji.

### 3. Pokrij CRUD matricu
Za svaku akciju mapiraj minimalne testove:
1. GET collection
- No auth -> 401
- Auth allowed role -> 200
- Body: success=true i data nije null.
2. GET by id
- No auth -> 401
- User bez prava na tudji resource -> 403
- Vlastiti id (ako je dozvoljeno) -> 200
- Nepostojeci id -> 404
- Body: za 200 success=true; za 404 success=false i alert nije null.
3. POST create
- No auth -> 401
- Non-admin -> 403 (ako je admin-only)
- Invalid payload -> 400
- Valid payload + allowed role -> 201 Created
- Duplicate payload (ako postoji unique pravilo) -> 400 sa business code (npr. USERNAME_EXISTS, EMAIL_EXISTS, COUNTRY_EXISTS).
- Body: za 201 success=true i data.id > 0.
4. PUT update
- No auth -> 401
- Non-admin (ili ne-vlasnik) -> 403
- Valid payload + owner/admin -> 200
- Nepostojeci id -> 404
- Body: success i data su konzistentni sa izmjenjenim poljima.
5. DELETE soft delete
- No auth -> 401
- Non-admin -> 403 (ako je admin-only)
- Admin + postojeci id -> 200
- Nepostojeci id -> 404
- Nakon delete pozovi GET by id i potvrdi expected ponasanje (npr. 404 za non-admin ili oznaceno kao obrisano za admin endpoint).
6. RESTORE (ako postoji)
- No auth -> 401
- Non-admin -> 403
- Admin restore obrisanog -> 200
- Nakon restore potvrdi da je entitet opet dostupan kroz GET.

### 3.1 Edge caseovi (obavezno gdje ima smisla)
1. Prazan ili whitespace string za obavezna tekstualna polja.
2. Predugacak unos koji krsi StringLength.
3. Neispravan format (npr. email).
4. Pokusaj pristupa tudjem resursu kada postoji owner pravilo.
5. Pokusaj ponovnog restore/delete kada je resource vec u tom stanju (ako endpoint to podrzava).

### 4. Napravi stabilne test podatke
1. Koristi helper CreateEntityAsAdminAsync kada test treba id svjeze kreiranog entiteta.
2. Generiraj unikatan payload (npr. username/email/naziv sa Guid) da izbjegnes konflikt jedinstvenosti.
3. U helperu validiraj da create vraca success i procitaj id iz API response objekta.

### 5. Validacije koje treba provjeriti
1. Required field nedostaje -> 400.
2. Format neispravan (npr. email) -> 400.
3. Poslovno pravilo (npr. duplicate username/email/naziv) -> 400 sa business kodom ako postoji.
4. Access rule prekrsen -> 403.
5. Resource ne postoji ili je skriven prema pravilima -> 404.
6. Error body sadrzi success=false i alert.message.

### 6. Kvaliteta i pouzdanost
1. Jedan test = jedan razlog pada.
2. Naziv testa mora opisati uvjet i ocekivanje.
3. Koristi Arrange-Act-Assert strukturu.
4. Ne oslanjaj se na redoslijed testova.
5. Ne dijeli mutable state izmedu testova osim kontrolirano kroz factory i InMemory setup.

### 7. Prakticni template za novi test class
Koristi ovaj kostur i prilagodi endpoint/payload po kontroleru.

```csharp
using System.Net;
using System.Net.Http.Json;
using KitchenAidAI.IntegrationTests.Infrastructure;
using KitchenAidAI.Models.DTOs;

namespace KitchenAidAI.IntegrationTests;

public class ExampleApiTests : IClassFixture<KitchenAidApiFactory>
{
  private readonly KitchenAidApiFactory _factory;

  public ExampleApiTests(KitchenAidApiFactory factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
  {
    using var client = _factory.CreateClient();
    var response = await client.GetAsync("/api/example");

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  public async Task Create_WithAdmin_ReturnsCreated_AndValidBody()
  {
    using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
    var payload = BuildValidPayload();

    var response = await client.PostAsJsonAsync("/api/example", payload);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var body = await response.Content.ReadFromJsonAsync<ApiResponse<ExampleDto>>();
    Assert.NotNull(body);
    Assert.True(body!.success);
    Assert.NotNull(body.data);
    Assert.True(body.data!.id > 0);
  }

  [Fact]
  public async Task Create_WithDuplicatePayload_ReturnsBadRequest_AndBusinessCode()
  {
    using var client = CreateAuthorizedClient(userId: 1, role: "Admin");
    var payload = BuildValidPayload();

    var first = await client.PostAsJsonAsync("/api/example", payload);
    first.EnsureSuccessStatusCode();

    var duplicate = await client.PostAsJsonAsync("/api/example", payload);

    Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
    var body = await duplicate.Content.ReadFromJsonAsync<ApiResponse<object>>();
    Assert.NotNull(body);
    Assert.False(body!.success);
    Assert.NotNull(body.alert);
    Assert.False(string.IsNullOrWhiteSpace(body.alert!.code));
  }

  private HttpClient CreateAuthorizedClient(int userId, string role)
  {
    var client = _factory.CreateClient();
    client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
    client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
    return client;
  }

  private static object BuildValidPayload()
  {
    return new
    {
      naziv = $"example-{Guid.NewGuid():N}"
    };
  }
}
```

### 8. Obavezna test mapa po endpointu (quick checklist)
Za svaki endpoint iz kontrolera potvrdi ove tocke gdje su primjenjive:
1. Unauthorized: bez auth -> 401.
2. Forbidden: ulogiran bez prava -> 403.
3. Success: dozvoljena uloga -> 2xx.
4. NotFound: nepostojeci id -> 404.
5. Validation: invalid payload -> 400.
6. Duplicate: isti unique podatak -> 400 (Users/Countries obavezno).
7. Response body: success/data/alert i kodovi poruka.
8. Soft-delete/restore: delete efekt + restore dostupnost.

### 9. Test review pravila prije commit-a
1. Svaki test ima jasan naziv uvjet_akcija_ocekivanje.
2. Nema testova koji ovise o rezultatu prethodnog testa.
3. Svi helperi su centralizirani i ponovo iskoristivi.
4. Testovi prolaze u jednom runu lokalno bez flaky ponasanja.
5. Ako endpoint vraca ApiResponse<T>, body se verificira u barem jednom happy-path i jednom error-path testu.

## Predlozak testova po kontroleru
1. Unauthorized test za najmanje jednu GET akciju.
2. Forbidden test za jednu admin-only akciju.
3. Created/Ok test za happy path.
4. BadRequest test za invalid payload.
5. NotFound test za nepostojeci id.
6. Owner access test (ako postoji owner pravilo).
7. Duplicate test za unique business pravila (obavezno za Users i Countries).
8. Soft-delete i restore par testova (gdje endpoint postoji).

## Completion checks
- Svi testovi prolaze lokalno i u istom runu su deterministicni.
- Pokriveni su 401, 403, 400, 404 i barem jedan 2xx po kontroleru.
- Admin i user role scenariji su eksplicitno testirani.
- Create helper vraca stvarni id i koristi se za follow-up GET/PUT/DELETE testove.
- Nema flaky testova uzrokovanih novom InMemory bazom po requestu.
- Za svaki bitan scenario validiran je i body format, ne samo status kod.
- Soft-delete/restore i duplicate pravila su testirani gdje su dio domene.

## Brzi primjer promptova
- Napravi integration testove za DatotekeApiController po ovoj CRUD matrici.
- Dodaj owner-vs-admin authorization testove za ReceptiApiController.
- Prosiri postojeci test class sa invalid payload i not found scenarijima.
