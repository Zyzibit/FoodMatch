using inzynierka.Data;
using inzynierka.Users.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace inzynierka.IntegrationTests;

public abstract class DatabaseIntegrationTest : IAsyncLifetime
{
    protected ServiceProvider ServiceProvider;
    protected AppDbContext DbContext;

    public virtual async Task InitializeAsync()
    {
        var services = new ServiceCollection();

        // Dodanie InMemory bazy danych
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        // Dodanie Identity
        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
            
        // Dodanie logowania
        services.AddLogging();

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Inicjalizacja bazy
        await DbContext.Database.EnsureCreatedAsync();
        await Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.DisposeAsync();
        ServiceProvider.Dispose();
        await Task.CompletedTask;
    }
}

