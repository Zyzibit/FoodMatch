using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using inzynierka.MealPlans.Model;

namespace inzynierka.Data.Configuration;

public class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
{
    public void Configure(EntityTypeBuilder<MealPlan> builder)
    {
        // Unikalny indeks zostanie dodany ręcznie w migracji
        // używając DATE() funkcji PostgreSQL
    }
}


