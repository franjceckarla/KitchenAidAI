using KitchenAidAI.Models;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Helpers
{
    public class MockDataService
    {
        private readonly List<User> _users;
        private readonly List<Recept> _recipes;

        public MockDataService()
        {
            var recept1 = new Recept
            {
                id = 1,
                naziv = "Torta od cokolade",
                opis = "Kremasta torta s bogatim cokoladnim punjenjem.",
                vrijemeKuhanja = 90,
                tezina = TezinaRecepta.Srednje,
                brojPorcija = 8
            };

            var recept2 = new Recept
            {
                id = 2,
                naziv = "Pasta Carbonara",
                opis = "Kremasta tjestenina s jajima i sirom.",
                vrijemeKuhanja = 30,
                tezina = TezinaRecepta.Lako,
                brojPorcija = 4
            };

            var recept3 = new Recept
            {
                id = 3,
                naziv = "Risotto sa gljivama",
                opis = "Aromaticni risotto sa svjezim gljivama.",
                vrijemeKuhanja = 45,
                tezina = TezinaRecepta.Srednje,
                brojPorcija = 4
            };

            _recipes = new List<Recept> { recept1, recept2, recept3 };

            var user1Frizider = new Frizider
            {
                id = 1,
                namirnice = new List<Namirnica>
                {
                    new Namirnica { id = 1, naziv = "Jaja", kategorija = KategorijaNamirnice.MlijecniProizvod, mjera = Mjera.Komad, kolicinaUFrizideru = 10 },
                    new Namirnica { id = 2, naziv = "Maslac", kategorija = KategorijaNamirnice.MlijecniProizvod, mjera = Mjera.Gram, kolicinaUFrizideru = 250 },
                    new Namirnica { id = 3, naziv = "Cokolada", kategorija = KategorijaNamirnice.Zacini, mjera = Mjera.Gram, kolicinaUFrizideru = 300 }
                }
            };

            var user2Frizider = new Frizider
            {
                id = 2,
                namirnice = new List<Namirnica>
                {
                    new Namirnica { id = 4, naziv = "Parmezan", kategorija = KategorijaNamirnice.MlijecniProizvod, mjera = Mjera.Gram, kolicinaUFrizideru = 150 },
                    new Namirnica { id = 5, naziv = "Tjestenina", kategorija = KategorijaNamirnice.Zitarice, mjera = Mjera.Gram, kolicinaUFrizideru = 500 },
                    new Namirnica { id = 6, naziv = "Gljive", kategorija = KategorijaNamirnice.Povrce, mjera = Mjera.Gram, kolicinaUFrizideru = 400 }
                }
            };

            var kuharica1 = new Kuharica
            {
                id = 1,
                naziv = "Markova kuharica",
                receptKuharice = new List<ReceptKuharica>
                {
                    new ReceptKuharica { id = 1, kuharicaId = 1, receptId = recept1.id, recept = recept1 },
                    new ReceptKuharica { id = 2, kuharicaId = 1, receptId = recept2.id, recept = recept2 }
                }
            };

            var kuharica2 = new Kuharica
            {
                id = 2,
                naziv = "Petrina kuharica",
                receptKuharice = new List<ReceptKuharica>
                {
                    new ReceptKuharica { id = 3, kuharicaId = 2, receptId = recept2.id, recept = recept2 },
                    new ReceptKuharica { id = 4, kuharicaId = 2, receptId = recept3.id, recept = recept3 }
                }
            };

            var kuharica3 = new Kuharica
            {
                id = 3,
                naziv = "Anina kuharica",
                receptKuharice = new List<ReceptKuharica>()
            };

            _users = new List<User>
            {
                new User
                {
                    id = 1,
                    username = "marko92",
                    email = "marko@example.com",
                    preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
                    frizider = user1Frizider,
                    kuharicaId = kuharica1.id,
                    kuharica = kuharica1
                },
                new User
                {
                    id = 2,
                    username = "petra_love",
                    email = "petra@example.com",
                    preferencijaPrehrane = PreferencijaPrehrane.Vegetarijanac,
                    frizider = user2Frizider,
                    kuharicaId = kuharica2.id,
                    kuharica = kuharica2
                },
                new User
                {
                    id = 3,
                    username = "ana_fit",
                    email = "ana@example.com",
                    preferencijaPrehrane = PreferencijaPrehrane.Vegan,
                    frizider = new Frizider { id = 3, namirnice = new List<Namirnica>() },
                    kuharicaId = kuharica3.id,
                    kuharica = kuharica3
                }
            };
        }

        public IReadOnlyList<User> GetUsers() => _users;

        public User? GetUserById(int id) => _users.FirstOrDefault(u => u.id == id);

        public Frizider? GetFriziderByUserId(int userId) => GetUserById(userId)?.frizider;

        public IReadOnlyList<Recept> GetRecipes() => _recipes;

        public Recept? GetRecipeById(int id) => _recipes.FirstOrDefault(r => r.id == id);

        public Kuharica? GetCookbookByUserId(int userId) => GetUserById(userId)?.kuharica;
    }
}
