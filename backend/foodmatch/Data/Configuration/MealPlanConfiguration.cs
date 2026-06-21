using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using foodmatch.MealPlans.Model;

namespace foodmatch.Data.Configuration;

public class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
{
    public void Configure(EntityTypeBuilder<MealPlan> builder)
    {
        // Unikalny indeks zostanie dodany ręcznie w migracji
        // używając DATE() funkcji PostgreSQL
    }
}


