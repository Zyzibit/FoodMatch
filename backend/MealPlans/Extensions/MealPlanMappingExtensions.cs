using System;
using inzynierka.MealPlans.Model;
using inzynierka.MealPlans.Responses;
using inzynierka.Receipts.Extensions.Model;

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
            Receipt = model.Receipt != null ? new MealPlanReceiptDto
            {
                Id = model.Receipt.Id,
                Title = model.Receipt.Title,
                Description = model.Receipt.Description ?? string.Empty,
                CaloriesPer100G = model.Receipt.Calories,
                ProteinPer100G = model.Receipt.Protein,
                CarbohydratesPer100G = model.Receipt.Carbohydrates,
                FatsPer100G = model.Receipt.Fats,
                Servings = model.Receipt.Servings,
                PreparationTimeMinutes = model.Receipt.PreparationTimeMinutes
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

