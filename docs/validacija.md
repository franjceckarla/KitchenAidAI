# Validacija u aplikaciji

Ovaj dokument popisuje sve postavljene validacije i lokacije gdje su definirane.

## Server-side validacije (data annotations)

### Models/User.cs
- username: [Required], [StringLength(100)]
- email: [Required], [StringLength(200)], [RegularExpression("email format")]
- ime: [StringLength(120)]
- prezime: [StringLength(120)]
- datumRodenja: [DataType(DataType.Date)]
- zemlja: [StringLength(120)]
- passwordHash: [StringLength(2000)]

### Models/ViewModels/LoginViewModel.cs
- Username: [Required], [StringLength(100)]
- Password: [Required], [DataType(DataType.Password)]

### Models/ViewModels/RegisterViewModel.cs
- Username: [Required], [StringLength(100)]
- Ime: [Required], [StringLength(120)]
- Prezime: [Required], [StringLength(120)]
- DatumRodenja: [Required], [DataType(DataType.DateTime)]
- Email: [Required], [StringLength(200)], [EmailAddress]
- Zemlja: [Required], [StringLength(120)]
- Password: [Required], [DataType(DataType.Password)]

### Models/Namirnica.cs
- naziv: [Required], [StringLength(200)]

### Models/Recept.cs
- naziv: [Required], [StringLength(200)]
- opis: [StringLength(1000)]

### Models/Country.cs
- naziv: [Required], [StringLength(120)]

## Server-side validacije (custom logic)

### Controllers/AuthController.cs
- Register: SelectedPreferences mora imati tocno jednu vrijednost.
- Register: Username mora biti jedinstven u bazi.
- Register: Email mora biti jedinstven u bazi.
- Login: krive vjerodajnice vracaju ModelState gresku.

## Client-side validacije

### Views/Shared/_Layout.cshtml i Views/Shared/_LayoutLogin.cshtml
- Ucitavaju jQuery validation i unobtrusive validation.

### wwwroot/js/site.js
- Validacija se okida na blur (onfocusout).
- Custom date parser prihvaca dd.MM.yyyy i ISO format.

## Prikaz validacijskih poruka u UI

- Views/Auth/Login.cshtml: asp-validation-for + validation-summary.
- Views/Auth/Register.cshtml: asp-validation-for + validation-summary.
- Views/Korisnici/Create.cshtml: asp-validation-for polja.
- Views/Korisnici/Edit.cshtml: asp-validation-for polja.
- Views/Namirnice/Create.cshtml: asp-validation-for polja.
- Views/Namirnice/Edit.cshtml: asp-validation-for polja.
- Views/Recepti/Create.cshtml: asp-validation-for polja.
- Views/Recepti/Edit.cshtml: asp-validation-for polja.

## Stilizacija validacije

- wwwroot/css/y2k-theme/components/forms.css: izgled poruka i error state polja.

## Primjeri validacije (primjeri ponasanja)

### Registracija - Email
- Validno: "ime@domena.com" -> prolazi client i server validaciju.
- Nevalidno: "ime@" -> client-side prikazuje poruku o formatu emaila.
- Nevalidno: email vec postoji -> server vraca poruku "Email je vec registriran."

### Registracija - Preferencija prehrane
- Nevalidno: nista odabrano -> server vraca poruku "Odaberite barem jednu preferenciju prehrane."
- Nevalidno: vise od jedne -> server vraca poruku "Odaberite samo jednu preferenciju prehrane."

### Prijava - Lozinka
- Prazno polje -> client-side prikazuje da je polje obavezno.
- Pogresne vjerodajnice -> server vraca poruku "Neispravno korisnicko ime ili lozinka."

### Namirnice - Naziv
- Prazno polje -> client-side prikazuje obavezno polje, server odbija spremanje.
