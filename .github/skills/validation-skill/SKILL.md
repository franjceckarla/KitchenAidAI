---
name: validation-skill
description: 'Vodic za server-side i client-side validaciju u ASP.NET Core MVC aplikacijama. Koristi se kada se dodaju ili dokumentiraju validacijska pravila, ModelState provjere i prikaz poruka u UI-ju.'
argument-hint: 'Koja forma ili feature treba validaciju (npr. Registracija, Prijava, Novi recept)?'
user-invocable: true
---

# Vodic za validaciju (Server + Client)

## Kada koristiti
- Dodavanje validacije na novu formu ili feature.
- Provjera da postoje i server-side i client-side validacije.
- Dokumentiranje pravila i mjesta gdje su definirana.

## Ishodi
- Definirana data annotations pravila i custom logika.
- ModelState provjere u kontrolerima.
- Client validacija ukljucena i okida se na blur.
- UI dosljedno prikazuje validacijske poruke.

## Postupak

### 1. Identificiraj formu i model
- Pronadi ViewModel ili Model koji forma koristi.
- Odredi koja polja su obavezna i koja ogranicenja vrijede (duljina, format, raspon).

### 2. Dodaj server-side pravila (data annotations)
- Koristi `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]`, `[RegularExpression]`, `[DataType]`.
- Poruke neka budu dosljedne i razumljive korisniku.

### 3. Dodaj server-side custom logiku
- U POST akciji kontrolera dodaj `ModelState.AddModelError` za provjere izmedu polja ili iz baze.
- Prije spremanja uvijek provjeri `if (!ModelState.IsValid) { return View(input); }`.

### 4. Ukljucci client-side validaciju
- Provjeri da su `asp-for` na inputima i `asp-validation-for` za poruke.
- U layout ukljuci partial za validation skripte.
- U JS inicijalizatoru omoguci validaciju na blur (onfocusout).

### 5. Osiguraj dosljedan prikaz poruka
- Dodaj `asp-validation-summary="ModelOnly"` za top-level greske.
- Stiliziraj `.field-validation-error`, `.validation-summary-errors` i `.input-validation-error`.

### 6. Provjeri end-to-end ponasanje
- Prazna obavezna polja -> client prikazuje gresku na blur.
- Nevalidan format -> client prikazuje gresku formata.
- Pravila samo na serveru (jedinstvenost, cross-field) -> server vraca greske.

## Checklist kvalitete
- Server-side validacija uvijek blokira neispravne podatke.
- Client-side validacija radi na blur i submit.
- Validacijske poruke su vidljive i stilizirane.
- Greske se vracaju na istu formu uz sacuvane unose.

## Napomene
- Client-side validacija je pomocna; nikad se ne oslanjati samo na nju.
- Za datumska polja provjeri da client parser podrzava prikazani format.
