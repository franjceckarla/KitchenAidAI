using KitchenAidAI.Models;
using KitchenAidAI.Models.Enums;

namespace KitchenAidAI.Helpers.MockData
{
    internal static class MockUserBuilder
    {
        public static List<User> BuildUsers(
            IReadOnlyDictionary<int, Frizider> fridges,
            IReadOnlyDictionary<int, Kuharica> cookbooks)
        {
            return
            [
                new User
                {
                    id = 1,
                    username = "marko92",
                    email = "marko@example.com",
                    preferencijaPrehrane = PreferencijaPrehrane.Omnivorte,
                    frizider = fridges[1],
                    kuharica = cookbooks[1]
                },
                new User
                {
                    id = 2,
                    username = "petra_love",
                    email = "petra@example.com",
                    preferencijaPrehrane = PreferencijaPrehrane.Vegetarijanac,
                    frizider = fridges[2],
                    kuharica = cookbooks[2]
                },
                new User
                {
                    id = 3,
                    username = "ana_fit",
                    email = "ana@example.com",
                    preferencijaPrehrane = PreferencijaPrehrane.Vegan,
                    frizider = fridges[3],
                    kuharica = cookbooks[3]
                }
            ];
        }
    }
}