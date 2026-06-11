using KitchenAidAI.Models;

namespace KitchenAidAI.Models.DTOs
{
    public static class DtoMapper
    {
        public static ReceptDto ToDto(this Recept recipe)
        {
            return new ReceptDto
            {
                id = recipe.id,
                naziv = recipe.naziv,
                opis = recipe.opis,
                vrijemeKuhanja = recipe.vrijemeKuhanja,
                tezina = recipe.tezina,
                brojPorcija = recipe.brojPorcija,
                isDeleted = recipe.isDeleted
            };
        }

        public static ReceptPublicDto ToPublicDto(this Recept recipe)
        {
            return new ReceptPublicDto
            {
                id = recipe.id,
                naziv = recipe.naziv,
                opis = recipe.opis,
                vrijemeKuhanja = recipe.vrijemeKuhanja,
                tezina = recipe.tezina,
                brojPorcija = recipe.brojPorcija
            };
        }

        public static ReceptFormDto ToFormDto(this Recept recipe)
        {
            return new ReceptFormDto
            {
                id = recipe.id,
                naziv = recipe.naziv,
                opis = recipe.opis,
                vrijemeKuhanja = recipe.vrijemeKuhanja,
                tezina = recipe.tezina,
                brojPorcija = recipe.brojPorcija
            };
        }

        public static UserDto ToDto(this User user)
        {
            return new UserDto
            {
                id = user.id,
                username = user.username,
                ime = user.ime,
                prezime = user.prezime,
                datumRodenja = user.datumRodenja,
                zemlja = user.zemlja,
                email = user.email,
                preferencijaPrehrane = user.preferencijaPrehrane,
                kreirano = user.kreirano,
                isAdmin = user.isAdmin,
                isDeleted = user.isDeleted,
                frizider = user.frizider is null ? null : user.frizider.ToDto(),
                kuharica = user.kuharica is null ? null : user.kuharica.ToDto()
            };
        }

        public static UserPublicDto ToPublicDto(this User user)
        {
            return new UserPublicDto
            {
                id = user.id,
                username = user.username,
                ime = user.ime,
                prezime = user.prezime,
                datumRodenja = user.datumRodenja,
                zemlja = user.zemlja,
                email = user.email,
                preferencijaPrehrane = user.preferencijaPrehrane,
                frizider = user.frizider is null ? null : user.frizider.ToPublicDto(),
                kuharica = user.kuharica is null ? null : user.kuharica.ToPublicDto()
            };
        }

        public static UserEditDto ToEditDto(this User user)
        {
            return new UserEditDto
            {
                id = user.id,
                username = user.username,
                email = user.email,
                preferencijaPrehrane = user.preferencijaPrehrane,
                isAdmin = user.isAdmin
            };
        }

        public static UserEditDto ToEditDto(this UserDto user)
        {
            return new UserEditDto
            {
                id = user.id,
                username = user.username,
                email = user.email,
                preferencijaPrehrane = user.preferencijaPrehrane,
                isAdmin = user.isAdmin
            };
        }

        public static FriziderDto ToDto(this Frizider fridge)
        {
            return new FriziderDto
            {
                id = fridge.id,
                userId = fridge.userId,
                isDeleted = fridge.isDeleted,
                namirnice = fridge.namirnice.Select(item => item.ToDto()).ToList()
            };
        }

        public static FriziderPublicDto ToPublicDto(this Frizider fridge)
        {
            return new FriziderPublicDto
            {
                id = fridge.id,
                userId = fridge.userId,
                namirnice = fridge.namirnice
                    .Where(item => !item.isDeleted)
                    .Select(item => item.ToPublicDto())
                    .ToList()
            };
        }

        public static NamirnicaDto ToDto(this Namirnica item)
        {
            return new NamirnicaDto
            {
                id = item.id,
                friziderId = item.friziderId,
                naziv = item.naziv,
                kategorija = item.kategorija,
                mjera = item.mjera,
                kolicinaUFrizideru = item.kolicinaUFrizideru,
                isDeleted = item.isDeleted
            };
        }

        public static NamirnicaPublicDto ToPublicDto(this Namirnica item)
        {
            return new NamirnicaPublicDto
            {
                id = item.id,
                friziderId = item.friziderId,
                naziv = item.naziv,
                kategorija = item.kategorija,
                mjera = item.mjera,
                kolicinaUFrizideru = item.kolicinaUFrizideru
            };
        }

        public static NamirnicaFormDto ToFormDto(this Namirnica item)
        {
            return new NamirnicaFormDto
            {
                id = item.id,
                friziderId = item.friziderId,
                naziv = item.naziv,
                kategorija = item.kategorija,
                mjera = item.mjera,
                kolicinaUFrizideru = item.kolicinaUFrizideru
            };
        }

        public static KuharicaDto ToDto(this Kuharica cookbook)
        {
            return new KuharicaDto
            {
                id = cookbook.id,
                naziv = cookbook.naziv,
                userId = cookbook.userId,
                isDeleted = cookbook.isDeleted,
                receptKuharice = cookbook.receptKuharice.Select(join => join.ToDto()).ToList()
            };
        }

        public static KuharicaPublicDto ToPublicDto(this Kuharica cookbook)
        {
            return new KuharicaPublicDto
            {
                id = cookbook.id,
                naziv = cookbook.naziv,
                userId = cookbook.userId,
                receptKuharice = cookbook.receptKuharice
                    .Where(join => !join.isDeleted && join.recept != null && !join.recept.isDeleted)
                    .Select(join => join.ToPublicDto())
                    .ToList()
            };
        }

        public static ReceptKuharicaDto ToDto(this ReceptKuharica join)
        {
            return new ReceptKuharicaDto
            {
                id = join.id,
                receptId = join.receptId,
                kuharicaId = join.kuharicaId,
                kreirano = join.kreirano,
                isDeleted = join.isDeleted,
                recept = join.recept is null ? null : join.recept.ToDto()
            };
        }

        public static ReceptKuharicaPublicDto ToPublicDto(this ReceptKuharica join)
        {
            return new ReceptKuharicaPublicDto
            {
                id = join.id,
                receptId = join.receptId,
                kuharicaId = join.kuharicaId,
                kreirano = join.kreirano,
                recept = join.recept is null ? null : join.recept.ToPublicDto()
            };
        }

        public static DatotekaDto ToDto(this Datoteka file)
        {
            return new DatotekaDto
            {
                id = file.id,
                userId = file.userId,
                naziv = file.naziv,
                opis = file.opis,
                contentType = file.contentType,
                velicina = file.velicina,
                putanja = file.putanja,
                kreirano = file.kreirano,
                isDeleted = file.isDeleted,
                deletedAt = file.deletedAt
            };
        }
    }
}
