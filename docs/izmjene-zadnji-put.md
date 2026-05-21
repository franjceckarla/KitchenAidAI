# Dokumentacija izmjena (posljednje izmjene)

Ovaj dokument opisuje zadnje dodane funkcionalnosti i izmjene: sto je promijenjeno, gdje se nalazi kod, zasto je dodano i kako se ponasa u praksi.

**1) Autentikacija i sesije**
- **Kontroler:** Prijava/registracija i odjava su u [KitchenAidAI/Controllers/AuthController.cs](KitchenAidAI/Controllers/AuthController.cs#L22-L196). Tu se:
  - sprjecava pristup login/registraciji ako je korisnik vec ulogiran (redirect na Home),
  - provjeravaju vjerodajnice i hash lozinke,
  - sprema novi korisnik uz kreiranje frizidera i kuharice,
  - odjavljuje sesija.
- **Session helper:** [KitchenAidAI/Helpers/AuthSession.cs](KitchenAidAI/Helpers/AuthSession.cs#L1-L38) zapisuje `UserId`, `Username`, `IsAdmin` u session i koristi ih u cijeloj aplikaciji.
- **Filter:** [KitchenAidAI/Filters/RequireSessionAttribute.cs](KitchenAidAI/Filters/RequireSessionAttribute.cs#L1-L33) blokira neautorizirane zahtjeve i popunjava `ViewBag` s korisnickim podacima za viewove.

**2) Validacija (server + client)**
- **Data annotations (HR poruke):**
  - [KitchenAidAI/Models/ViewModels/RegisterViewModel.cs](KitchenAidAI/Models/ViewModels/RegisterViewModel.cs#L1-L38)
  - [KitchenAidAI/Models/ViewModels/LoginViewModel.cs](KitchenAidAI/Models/ViewModels/LoginViewModel.cs#L1-L14)
  - [KitchenAidAI/Models/User.cs](KitchenAidAI/Models/User.cs#L1-L55)
  - [KitchenAidAI/Models/Namirnica.cs](KitchenAidAI/Models/Namirnica.cs#L1-L36)
  - [KitchenAidAI/Models/Recept.cs](KitchenAidAI/Models/Recept.cs#L1-L40)
  - [KitchenAidAI/Models/Country.cs](KitchenAidAI/Models/Country.cs#L1-L13)
- **Custom provjere:**
  - Registracija i admin create provjeravaju preferencije, jedinstvenost username/email u [KitchenAidAI/Controllers/AuthController.cs](KitchenAidAI/Controllers/AuthController.cs#L78-L135) i [KitchenAidAI/Controllers/KorisniciController.cs](KitchenAidAI/Controllers/KorisniciController.cs#L86-L145).
- **Client-side validacija:**
  - Scriptovi se ucitavaju u layoutima [KitchenAidAI/Views/Shared/_Layout.cshtml](KitchenAidAI/Views/Shared/_Layout.cshtml) i [KitchenAidAI/Views/Shared/_LayoutLogin.cshtml](KitchenAidAI/Views/Shared/_LayoutLogin.cshtml#L1-L28).
  - `onfocusout` i custom `date` parser su u [KitchenAidAI/wwwroot/js/site.js](KitchenAidAI/wwwroot/js/site.js).
- **Styling gresaka:** [KitchenAidAI/wwwroot/css/y2k-theme/components/forms.css](KitchenAidAI/wwwroot/css/y2k-theme/components/forms.css).
- **Dodatna dokumentacija:** [docs/validacija.md](docs/validacija.md).

**3) Date picker + lokalizacija datuma**
- **Lokalizacija kultura:** `RequestLocalizationOptions` u [KitchenAidAI/Program.cs](KitchenAidAI/Program.cs#L32-L38) (hr-HR default i 5 dodatnih kultura).
- **DateTime picker:** inicijalizacija i formatiranje u [KitchenAidAI/wwwroot/js/site.js](KitchenAidAI/wwwroot/js/site.js) (`initJqueryDateTimePicker`, `resolveDateLocaleConfig`, `buildDateFormat`).
- **Partial za datum:** [KitchenAidAI/Views/Shared/_DateTimePicker.cshtml](KitchenAidAI/Views/Shared/_DateTimePicker.cshtml#L1-L32) dodaje `data-val` atribute kad je polje required i prebacuje vrijednost u ISO format.
- **Primjena u formama:** Registracija i admin create koriste ovaj partial u [KitchenAidAI/Views/Auth/Register.cshtml](KitchenAidAI/Views/Auth/Register.cshtml) i [KitchenAidAI/Views/Korisnici/Create.cshtml](KitchenAidAI/Views/Korisnici/Create.cshtml).

**4) AJAX pretraga (liste i kuharice)**
- **JS logika:** [KitchenAidAI/wwwroot/js/site.js](KitchenAidAI/wwwroot/js/site.js) (`wireAjaxSearch`, `runSearch`) dodaje debounce i partial refresh.
- **Server endpoints:**
  - Korisnici: [KitchenAidAI/Controllers/KorisniciController.cs](KitchenAidAI/Controllers/KorisniciController.cs#L22-L51)
  - Kuharice: [KitchenAidAI/Controllers/KuhariceController.cs](KitchenAidAI/Controllers/KuhariceController.cs#L19-L130)
  - Frizider: [KitchenAidAI/Controllers/FriziderController.cs](KitchenAidAI/Controllers/FriziderController.cs#L19-L96)
- **Partiali koji se refreshaju:**
  - [KitchenAidAI/Views/Korisnici/_UserCards.cshtml](KitchenAidAI/Views/Korisnici/_UserCards.cshtml)
  - [KitchenAidAI/Views/Kuharice/_CookbookCards.cshtml](KitchenAidAI/Views/Kuharice/_CookbookCards.cshtml)
  - [KitchenAidAI/Views/Frizider/_FridgeItems.cshtml](KitchenAidAI/Views/Frizider/_FridgeItems.cshtml)
  - [KitchenAidAI/Views/Kuharice/_CookbookRecipes.cshtml](KitchenAidAI/Views/Kuharice/_CookbookRecipes.cshtml)
- **Search UI:** primjer u [KitchenAidAI/Views/Kuharice/Details.cshtml](KitchenAidAI/Views/Kuharice/Details.cshtml).

**5) Autocomplete polja (registracija + admin create)**
- **Frontend:** [KitchenAidAI/wwwroot/js/site.js](KitchenAidAI/wwwroot/js/site.js) (`initAutocompleteDropdowns`) uz CSS u [KitchenAidAI/wwwroot/css/y2k-theme/components/forms.css](KitchenAidAI/wwwroot/css/y2k-theme/components/forms.css).
- **API pozivi:**
  - Drzave: [KitchenAidAI/Controllers/AuthController.cs](KitchenAidAI/Controllers/AuthController.cs#L137-L156)
  - Ime/Prezime/Email: [KitchenAidAI/Controllers/AuthController.cs](KitchenAidAI/Controllers/AuthController.cs#L158-L188)
- **Forme:** [KitchenAidAI/Views/Auth/Register.cshtml](KitchenAidAI/Views/Auth/Register.cshtml) i [KitchenAidAI/Views/Korisnici/Create.cshtml](KitchenAidAI/Views/Korisnici/Create.cshtml).

**6) Klik na red tablice (brzi ulaz u detalje)**
- **JS handler:** [KitchenAidAI/wwwroot/js/site.js](KitchenAidAI/wwwroot/js/site.js) (`initRowLinks`) otvara `data-row-href` ako klik nije na gumbu ili inputu.
- **Tablice:**
  - Korisnici: [KitchenAidAI/Views/Korisnici/_UserCards.cshtml](KitchenAidAI/Views/Korisnici/_UserCards.cshtml)
  - Recepti: [KitchenAidAI/Views/Recepti/_RecipeCards.cshtml](KitchenAidAI/Views/Recepti/_RecipeCards.cshtml)
  - Kuharice: [KitchenAidAI/Views/Kuharice/_CookbookCards.cshtml](KitchenAidAI/Views/Kuharice/_CookbookCards.cshtml)
  - Kuharica detalji: [KitchenAidAI/Views/Kuharice/_CookbookRecipes.cshtml](KitchenAidAI/Views/Kuharice/_CookbookRecipes.cshtml)
  - Frizider: [KitchenAidAI/Views/Frizider/_FridgeItems.cshtml](KitchenAidAI/Views/Frizider/_FridgeItems.cshtml)

**7) Home dashboard + navigacija**
- **Filtriranje podataka po ulozi:** [KitchenAidAI/Controllers/HomeController.cs](KitchenAidAI/Controllers/HomeController.cs#L24-L65) prikazuje adminu sve korisnike, a korisniku samo njegov frizider/kuharicu.
- **CTA za frizider:** na Home stranici bira `userId` kad nije admin u [KitchenAidAI/Views/Home/Index.cshtml](KitchenAidAI/Views/Home/Index.cshtml).
- **Layout + breadcrumb:** postavljen u [KitchenAidAI/Views/Shared/_Layout.cshtml](KitchenAidAI/Views/Shared/_Layout.cshtml).

**8) Admin filtriranje i statusi**
- **Korisnici i kuharice:** admin vidi sve osim admin korisnika, obican korisnik vidi samo sebe. Logika u [KitchenAidAI/Controllers/KorisniciController.cs](KitchenAidAI/Controllers/KorisniciController.cs#L22-L51) i [KitchenAidAI/Controllers/KuhariceController.cs](KitchenAidAI/Controllers/KuhariceController.cs#L19-L56).
- **Frizider:** zahtijeva `userId`, a obican korisnik ne smije otvoriti tudji frizider. Logika u [KitchenAidAI/Controllers/FriziderController.cs](KitchenAidAI/Controllers/FriziderController.cs#L19-L63).

**9) Chef animacija (fix i nova logika)**
- **CSS animacije:** [KitchenAidAI/wwwroot/css/y2k-theme/animations.css](KitchenAidAI/wwwroot/css/y2k-theme/animations.css).
- **JS kontrola prikaza:** [KitchenAidAI/wwwroot/js/site.js](KitchenAidAI/wwwroot/js/site.js) (`initChefGreeting`, `showChefGreeting`, `hideChefGreeting`).
- **Markup u layoutu:** [KitchenAidAI/Views/Shared/_Layout.cshtml](KitchenAidAI/Views/Shared/_Layout.cshtml).

---

## Primjeri ponasanja (prakticno)

**A) Registracija korisnika**
1. Otvori [KitchenAidAI/Views/Auth/Register.cshtml](KitchenAidAI/Views/Auth/Register.cshtml).
2. Ako je email pogresan, client odmah prikaze poruku (data annotations) i blokira submit.
3. Ako je email vec zauzet, server vrati poruku iz [KitchenAidAI/Controllers/AuthController.cs](KitchenAidAI/Controllers/AuthController.cs#L106-L113).

**B) Pretraga kuharice bez reload-a**
1. U detaljima kuharice upisi pojam u search input u [KitchenAidAI/Views/Kuharice/Details.cshtml](KitchenAidAI/Views/Kuharice/Details.cshtml).
2. `site.js` salje AJAX GET na `Kuharice/Search` i mijenja partial [KitchenAidAI/Views/Kuharice/_CookbookRecipes.cshtml](KitchenAidAI/Views/Kuharice/_CookbookRecipes.cshtml).

**C) Lokalizirani datum**
1. Ako je kultura `hr-HR`, date picker koristi format `dd.MM.yyyy` iz [KitchenAidAI/wwwroot/js/site.js](KitchenAidAI/wwwroot/js/site.js).
2. Kod spremanja se ISO vrijednost salje kroz [KitchenAidAI/Views/Shared/_DateTimePicker.cshtml](KitchenAidAI/Views/Shared/_DateTimePicker.cshtml#L1-L32).

**D) Klik na red**
1. Klik na red u tablici korisnika otvara detalje preko `data-row-href` u [KitchenAidAI/Views/Korisnici/_UserCards.cshtml](KitchenAidAI/Views/Korisnici/_UserCards.cshtml).
2. Klik na gumb unutar reda ne mijenja ponasanje (JS ignorira `a`, `button`, `input`).

---

Ako zelis, mogu dodati i dodatne primjere (npr. validacija za Namirnice ili Recepti) ili napraviti kratki "how-to" za testiranje svih novih funkcionalnosti.