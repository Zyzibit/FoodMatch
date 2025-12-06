using inzynierka.UserPreferences.Services;
using Microsoft.Extensions.DependencyInjection;

namespace inzynierka.UserPreferences.Extensions;

public static class UserPreferencesServiceExtensions
{
    public static IServiceCollection AddUserPreferencesServices(this IServiceCollection services)
    {
        services.AddScoped<IUserPreferencesService, UserPreferencesService>();
        
        return services;
    }
}

