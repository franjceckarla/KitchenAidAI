using KitchenAidAI.Models;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Helpers.MockData
{
    internal static class MockFridgeBuilder
    {
        public static Dictionary<int, Frizider> BuildFridges()
        {
            return new Dictionary<int, Frizider>
            {
                [1] = new Frizider
                {
                    id = 1,
                    namirnice =
                    [
                        new Namirnica { id = 1, naziv = "Jaja", kategorija = KategorijaNamirnice.MlijecniProizvod, mjera = Mjera.Komad, kolicinaUFrizideru = 10 },
                        new Namirnica { id = 2, naziv = "Maslac", kategorija = KategorijaNamirnice.MlijecniProizvod, mjera = Mjera.Gram, kolicinaUFrizideru = 250 },
                        new Namirnica { id = 3, naziv = "Cokolada", kategorija = KategorijaNamirnice.Zacini, mjera = Mjera.Gram, kolicinaUFrizideru = 300 }
                    ]
                },
                [2] = new Frizider
                {
                    id = 2,
                    namirnice =
                    [
                        new Namirnica { id = 4, naziv = "Parmezan", kategorija = KategorijaNamirnice.MlijecniProizvod, mjera = Mjera.Gram, kolicinaUFrizideru = 150 },
                        new Namirnica { id = 5, naziv = "Tjestenina", kategorija = KategorijaNamirnice.Zitarice, mjera = Mjera.Gram, kolicinaUFrizideru = 500 },
                        new Namirnica { id = 6, naziv = "Gljive", kategorija = KategorijaNamirnice.Povrce, mjera = Mjera.Gram, kolicinaUFrizideru = 400 }
                    ]
                },
                [3] = new Frizider
                {
                    id = 3,
                    namirnice = []
                }
            };
        }
    }
}