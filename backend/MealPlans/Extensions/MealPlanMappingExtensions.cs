using System;
using inzynierka.MealPlans.Model;
using inzynierka.MealPlans.Responses;

namespace inzynierka.MealPlans.Extensions;

public static class MealPlanMappingExtensions
{
    public static MealPlanDto ToDto(this MealPlan model)
    {
        if (model == null) return null!; 

        return new MealPlanDto
        {
            Id = model.Id,
            Name = model.Name,
            Date = model.Date,
            Recipe = model.Recipe != null ? new MealPlanRecipeDto
            {
                Id = model.Recipe.Id,
                Title = model.Recipe.Title,
                Description = model.Recipe.Description ?? string.Empty,
                CaloriesPer100G = model.Recipe.Calories,
                ProteinPer100G = model.Recipe.Protein,
                CarbohydratesPer100G = model.Recipe.Carbohydrates,
                FatsPer100G = model.Recipe.Fats,
                Servings = model.Recipe.Servings,
                PreparationTimeMinutes = model.Recipe.PreparationTimeMinutes
            } : null
        };
    }

    public static MealPlan ToModel(this MealPlanDto dto)
    {
        if (dto == null) return null!;

        return new MealPlan
        {
            Id = dto.Id,
            Name = dto.Name,
            Date = dto.Date,
        };
    }
}

