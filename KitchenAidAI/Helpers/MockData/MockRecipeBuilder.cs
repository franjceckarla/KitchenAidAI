using KitchenAidAI.Models;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Helpers.MockData
{
    internal static class MockRecipeBuilder
    {
        public static List<Recept> BuildRecipes()
        {
            return
            [
                new Recept
                {
                    id = 1,
                    naziv = "Torta od cokolade",
                    opis = "Kremasta torta s bogatim cokoladnim punjenjem.",
                    vrijemeKuhanja = 90,
                    tezina = TezinaRecepta.Srednje,
                    brojPorcija = 8
                },
                new Recept
                {
                    id = 2,
                    naziv = "Pasta Carbonara",
                    opis = "Kremasta tjestenina s jajima i sirom.",
                    vrijemeKuhanja = 30,
                    tezina = TezinaRecepta.Lako,
                    brojPorcija = 4
                },
                new Recept
                {
                    id = 3,
                    naziv = "Risotto sa gljivama",
                    opis = "Aromaticni risotto sa svjezim gljivama.",
                    vrijemeKuhanja = 45,
                    tezina = TezinaRecepta.Srednje,
                    brojPorcija = 4
                }
            ];
        }
    }
}