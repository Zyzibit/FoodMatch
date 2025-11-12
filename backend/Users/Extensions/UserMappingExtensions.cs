 using inzynierka.Users.Model;
using inzynierka.Users.Responses;

namespace inzynierka.Users.Extensions;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user)
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

    public static FoodPreferencesDto ToDto(this FoodPreferences preferences)
    {
        return new FoodPreferencesDto
        {
            IsVegan = preferences.IsVegan,
            IsVegetarian = preferences.IsVegetarian,
            HasGlutenIntolerance = preferences.HasGlutenIntolerance,
            HasLactoseIntolerance = preferences.HasLactoseIntolerance,
            HasNutAllergy = preferences.HasNutAllergy,
            DailyCarbohydrateGoal = preferences.DailyCarbohydrateGoal,
            DailyProteinGoal = preferences.DailyProteinGoal,
            DailyFatGoal = preferences.DailyFatGoal,
            DailyCalorieGoal = preferences.DailyCalorieGoal
        };
    }
}

