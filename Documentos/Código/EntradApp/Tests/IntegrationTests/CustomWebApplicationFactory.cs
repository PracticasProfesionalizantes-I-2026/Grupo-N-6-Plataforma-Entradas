using EntradApp.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EntradApp.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<EntradApp.API.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<EntradAppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Add in-memory SQLite for testing
            services.AddDbContext<EntradAppDbContext>(options =>
            {
                options.UseSqlite("Data Source=:memory:");
            });

            // Build service provider and initialize DB
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EntradAppDbContext>();
            db.Database.OpenConnection();
            db.Database.EnsureCreated();
            EntradApp.DataAccess.DbInitializer.InitializeAsync(db).GetAwaiter().GetResult();
        });
    }
}