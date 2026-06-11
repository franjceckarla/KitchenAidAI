namespace KitchenAidAI.Models.DTOs
{
    public static class PublicDtoMapper
    {
        public static ReceptDto ToDto(this ReceptPublicDto recipe)
        {
            return new ReceptDto
            {
                id = recipe.id,
                naziv = recipe.naziv,
                opis = recipe.opis,
                vrijemeKuhanja = recipe.vrijemeKuhanja,
                tezina = recipe.tezina,
                brojPorcija = recipe.brojPorcija,
                isDeleted = false
            };
        }

        public static NamirnicaDto ToDto(this NamirnicaPublicDto item)
        {
            return new NamirnicaDto
            {
                id = item.id,
                friziderId = item.friziderId,
                naziv = item.naziv,
                kategorija = item.kategorija,
                mjera = item.mjera,
                kolicinaUFrizideru = item.kolicinaUFrizideru,
                isDeleted = false
            };
        }

        public static FriziderDto ToDto(this FriziderPublicDto fridge)
        {
            return new FriziderDto
            {
                id = fridge.id,
                userId = fridge.userId,
                isDeleted = false,
                namirnice = fridge.namirnice.Select(item => item.ToDto()).ToList()
            };
        }

        public static ReceptKuharicaDto ToDto(this ReceptKuharicaPublicDto join)
        {
            return new ReceptKuharicaDto
            {
                id = join.id,
                receptId = join.receptId,
                kuharicaId = join.kuharicaId,
                kreirano = join.kreirano,
                isDeleted = false,
                recept = join.recept is null ? null : join.recept.ToDto()
            };
        }

        public static KuharicaDto ToDto(this KuharicaPublicDto cookbook)
        {
            return new KuharicaDto
            {
                id = cookbook.id,
                naziv = cookbook.naziv,
                userId = cookbook.userId,
                isDeleted = false,
                receptKuharice = cookbook.receptKuharice.Select(join => join.ToDto()).ToList()
            };
        }

        public static UserDto ToDto(this UserPublicDto user)
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
                kreirano = default,
                isAdmin = false,
                isDeleted = false,
                frizider = user.frizider is null ? null : user.frizider.ToDto(),
                kuharica = user.kuharica is null ? null : user.kuharica.ToDto()
            };
        }

        public static ChatMessageDto ToDto(this ChatMessagePublicDto message)
        {
            return new ChatMessageDto
            {
                id = message.id,
                userId = message.userId,
                message = message.message,
                response = message.response,
                kreirano = message.kreirano,
                tip = message.tip,
                isDeleted = false
            };
        }
    }
}
