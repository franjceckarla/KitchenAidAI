# Semantički Model - KitchenAidAI

## Entiteti

- User - korisnik aplikacije
- Frizider - frižider korisnika
- Namirnica - namirnica u frižideru
- Kuharica - spremljena kolekcija recepata korisnika
- Recept - osnovni recept
- KorakRecepta - koraci pripreme recepta
- ReceptKuharica - veza između recepta i kuharice
- ChatMessage - poruke korisnika i AI asistenta
- NutritivnaVrijednost - ugniježđeni podatak o nutritivnim vrijednostima namirnice

## Veze

- User 1-1 Frizider
- User 1-1 Kuharica
- Frizider 1-N Namirnica
- Recept 1-N KorakRecepta
- Recept N-N Kuharica preko ReceptKuharica
- User 1-N ChatMessage
- Namirnica ima jednu NutritivnaVrijednost kao owned entity

## Napomena

- Primarni ključevi su označeni s `[Key]`.
- Strani ključevi su označeni s `[ForeignKey]`.
- MySQL provider je konfiguriran preko `Pomelo.EntityFrameworkCore.MySql`.