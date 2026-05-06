
Sažetak: Projekt koristi eksplicitne MVC rute u `Program.cs` i tag helper-e u viewovima za generiranje linkova.
---
name: ASP.NET Core Routing
description: Vodič kako dodavati i razumjeti rute u ASP.NET Core MVC projektu - eksplicitne rute u Program.cs, tag helper-i u viewovima, constraint-i i troubleshooting
argument-hint: "Primjer: Kako dodati novu rutu, Razumijevanje prioriteta ruta, Kako koristiti tag helper-e za generiranje linkova"
tools: ['vscode', 'read', 'edit', 'search']
model: Claude Haiku 4.5 (copilot)
---

# Routing skill: kako dodavati i razumjeti rute u projektu (ASP.NET Core MVC)

Sažetak: Projekt koristi eksplicitne MVC rute u `Program.cs` i tag helper-e u viewovima za generiranje linkova.

1. Gdje se rute nalaze
- Centralno mjesto: `Program.cs` gdje su pozivi `app.MapControllerRoute(...)`.
- Primjer custom rute u projektu: `app.MapControllerRoute(name: "frizider-po-korisniku", pattern: "frizider/{userId:int}", defaults: new { controller = "Frizider", action = "Index" });`

2. Kako rute rade (prioritet)
- Rute su evaluirane redom kako su registrirane; prvi match se koristi.
- Definiraj specifične rute prije općeg `default` route-a (`{controller=Home}/{action=Index}/{id?}`) kako bi specifične URL-ove usmjerio točno.

3. Generiranje linkova iz viewova
- Koristi tag helper-e: `asp-controller`, `asp-action`, `asp-route-{name}`. Primjer:
  - `<a asp-controller="Frizider" asp-action="Index" asp-route-userId="@user.id">Frižider</a>`
- Tag helper će iz generirane rute napraviti URL `/frizider/5` ako postoji odgovarajuća ruta s constraintom `{userId:int}`.

4. Dodavanje nove rute
- Dodaj `app.MapControllerRoute(...)` u `Program.cs` prije `default` rute.
- Ako treba constraint (int, guid, ...), koristi `:int` ili `:guid` u patternu.
- Za kompleksnije obrasce razmisli o `MapAreaControllerRoute` ili attribute routing (postavljanjem `[Route("..."/)]` na controller/action).

5. Troubleshooting
- Ako tag helper generira neočekivani URL, provjeri redoslijed `MapControllerRoute` poziva.
- Ako želiš prisiliti attribute routing samo na određenim akcijama, dodaj `[Route("fridge/{userId:int}")]` iznad action metode.
- Korištenje `app.UseRouting()` i `app.UseAuthorization()` mora biti prije `MapControllerRoute` poziva (već je u projektu).

Reference u repo:
- `KitchenAidAI/Program.cs` (gdje su definirane rute)
- `KitchenAidAI/Views/*` (primjeri `asp-controller`/`asp-action`/`asp-route-userId`)
- `KitchenAidAI/Controllers/*` (akcije koje primaju parametre iz rute)

Ako želiš, mogu dodati primjer nove rute s `guid` constraintom i pripadajućim linkovima u viewu.