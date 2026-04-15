using KitchenAidAI.Models;

namespace KitchenAidAI.Helpers.MockData
{
    internal static class MockCookbookBuilder
    {
        public static Dictionary<int, Kuharica> BuildCookbooks(IReadOnlyList<Recept> recipes)
        {
            var recipesById = recipes.ToDictionary(r => r.id);

            return new Dictionary<int, Kuharica>
            {
                [1] = new Kuharica
                {
                    id = 1,
                    naziv = "Markova kuharica",
                    receptKuharice =
                    [
                        new ReceptKuharica { id = 1, kuharicaId = 1, receptId = 1, recept = recipesById[1] },
                        new ReceptKuharica { id = 2, kuharicaId = 1, receptId = 2, recept = recipesById[2] }
                    ]
                },
                [2] = new Kuharica
                {
                    id = 2,
                    naziv = "Petrina kuharica",
                    receptKuharice =
                    [
                        new ReceptKuharica { id = 3, kuharicaId = 2, receptId = 2, recept = recipesById[2] },
                        new ReceptKuharica { id = 4, kuharicaId = 2, receptId = 3, recept = recipesById[3] }
                    ]
                },
                [3] = new Kuharica
                {
                    id = 3,
                    naziv = "Anina kuharica",
                    receptKuharice = []
                }
            };
        }
    }
}