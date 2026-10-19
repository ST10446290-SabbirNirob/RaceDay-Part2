using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDayAPI.Data;

namespace RaceDayAPI.Tests;

// Creates the actual API in memory. Tests never touch the user's SQL Server.
public sealed class RaceDayApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "RaceDayTests_" + Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:RaceDayConnection"] = "Server=localhost;Database=RaceDayTests;Trusted_Connection=True"
            });
        });
        builder.ConfigureTestServices(services =>
        {
            // Remove SQL Server options and replace them with an isolated in-memory DB.
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<RaceDayDbContext>>();
            services.AddDbContext<RaceDayDbContext>(options => options.UseInMemoryDatabase(_dbName));
        });
    }
}
