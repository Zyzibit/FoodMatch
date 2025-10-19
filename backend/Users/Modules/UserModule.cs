using inzynierka.Users.Contracts;
using inzynierka.Users.Contracts.Models;
using inzynierka.Users.Model;
using inzynierka.Users.Services;

namespace inzynierka.Users.Modules;

public class UserModule : IUserContract
{
    private readonly IUserService _userService;
    private readonly ILogger<UserModule> _logger;

    public UserModule(IUserService userService, ILogger<UserModule> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<List<UserDto>> GetUsersAsync(int pageNumber = 1, int pageSize = 10)
    {
        var users = await _userService.GetUsersAsync(pageNumber, pageSize);
        return users.Select(MapToUserDto).ToList();
    }

    public async Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request)
    {
        return await _userService.UpdateUserProfileAsync(userId, request.Name, request.Email);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        return await _userService.DeleteUserAsync(userId);
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _userService.GetTotalUsersCountAsync();
    }

    public Task<bool> UpdateUserFoodPreferencesAsync(string userId, UpdateFoodPreferencesRequest request) {
        return _userService.UpdateUserFoodPreferencesAsync(
            userId,
            new FoodPreferences
            {
                IsVegan = request.IsVegan,
                IsVegetarian = request.IsVegetarian,
                HasGlutenIntolerance = request.HasGlutenIntolerance,
                HasLactoseIntolerance = request.HasLactoseIntolerance,
                HasNutAllergy = request.HasNutAllergy,
                DailyCarbohydrateGoal = request.DailyCarbohydrateGoal,
                DailyProteinGoal = request.DailyProteinGoal,
                DailyFatGoal = request.DailyFatGoal,
                DailyCalorieGoal = request.DailyCalorieGoal
            }
        );
    }

    public async Task<FoodPreferencesDto?> GetUserFoodPreferencesAsync(string userId) {
        var prefs = await _userService.GetUserFoodPreferencesAsync(userId);
        if (prefs == null) {
            return null;
        }

        return new FoodPreferencesDto {
            IsVegan = prefs.IsVegan,
            IsVegetarian = prefs.IsVegetarian,
            HasGlutenIntolerance = prefs.HasGlutenIntolerance,
            HasLactoseIntolerance = prefs.HasLactoseIntolerance,
            HasNutAllergy = prefs.HasNutAllergy,
            DailyCarbohydrateGoal = prefs.DailyCarbohydrateGoal,
            DailyProteinGoal = prefs.DailyProteinGoal,
            DailyFatGoal = prefs.DailyFatGoal,
            DailyCalorieGoal = prefs.DailyCalorieGoal
        };
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}

