using KitchenAidAI.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KitchenAidAI.IntegrationTests.Infrastructure;

public class KitchenAidApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"KitchenAidTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbRegistrations = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(DbContextOptions<KitchenAidDbContext>)
                    || descriptor.ServiceType == typeof(DbContextOptions)
                    || descriptor.ServiceType == typeof(KitchenAidDbContext)
                    || (descriptor.ServiceType.IsGenericType
                        && descriptor.ServiceType.GetGenericTypeDefinition().Name == "IDbContextOptionsConfiguration`1"
                        && descriptor.ServiceType.GenericTypeArguments[0] == typeof(KitchenAidDbContext)))
                .ToList();

            foreach (var registration in dbRegistrations)
            {
                services.Remove(registration);
            }

            services.AddDbContext<KitchenAidDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            // Override app auth defaults so tests can inject identity via headers.
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName,
                    _ => { });
        });
    }
}
