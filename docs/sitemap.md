# Sitemap - KitchenAidAI

## Custom rute

- GET `/` -> HomeController.Index()
- GET `/korisnici` -> KorisniciController.Index()
- GET `/recepti` -> ReceptiController.Index()
- GET `/kuharice` -> KuhariceController.Index()
- GET `/frizider/{userId}` -> FriziderController.Index(userId)

## Standardne MVC rute

- GET `/Home/Index` -> HomeController.Index()
- GET `/Home/Chat` -> HomeController.Chat()
- GET `/Home/Privacy` -> HomeController.Privacy()
- GET `/Korisnici/Details/{id}` -> KorisniciController.Details(id)
- GET `/Recepti/Details/{id}` -> ReceptiController.Details(id)
- GET `/Kuharice/Details/{userId}` -> KuhariceController.Details(userId)

## View povezivanje

- Home/Index -> pregled korisnika, frižidera i chata
- Korisnici/Index -> lista korisnika
- Korisnici/Details -> profil korisnika
- Frizider/Index -> sadržaj frižidera
- Recepti/Index -> lista recepata
- Recepti/Details -> detalji recepta
- Kuharice/Index -> pregled kuharica
- Kuharice/Details -> detalji kuharice