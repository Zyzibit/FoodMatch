using System;
using inzynierka.MealPlans.Model;
using inzynierka.MealPlans.Constants;

namespace inzynierka.MealPlans.Extensions;

public static class MealPlanExtensions
{
    /// <summary>
    /// Zwraca DateTime z wartością w UTC (jeśli data ma Kind=Unspecified, traktuje ją jako UTC).
    /// </summary>
    public static DateTime GetUtcDate(this MealPlan mealPlan)
    {
        if (mealPlan == null) throw new ArgumentNullException(nameof(mealPlan));
        var date = mealPlan.Date;
        return date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime();
    }

    /// <summary>
    /// Sprawdza czy plan dotyczy tego samego dnia (porównanie daty w UTC, tylko część daty)
    /// </summary>
    public static bool IsForDate(this MealPlan mealPlan, DateTime date)
    {
        if (mealPlan == null) throw new ArgumentNullException(nameof(mealPlan));
        var planDate = mealPlan.GetUtcDate().Date;
        var otherDate = (date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime()).Date;
        return planDate == otherDate;
    }

    /// <summary>
    /// Zwraca sformatowaną nazwę posiłku. Jeśli nazwa jest pusta zwraca pusty string.
    /// Normalizuje wielkość liter i waliduje przeciwko MealPlanConstants.AllowedMealNames — jeśli niepoprawna, zwraca oryginalną nazwę.
    /// </summary>
    public static string GetDisplayName(this MealPlan mealPlan)
    {
        if (mealPlan == null) throw new ArgumentNullException(nameof(mealPlan));
        var name = mealPlan.Name ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name)) return string.Empty;

        // Normalizacja: pierwsza litera wielka, reszta małe
        var normalized = char.ToUpperInvariant(name[0]) + (name.Length > 1 ? name.Substring(1).ToLowerInvariant() : string.Empty);

        // Jeśli lista dozwolonych nazw jest dostępna, zwróć tę z listy (dokładne dopasowanie ignorujące wielkość liter)
        if (MealNames.AllowedMealNames != null)
        {
            foreach (var allowed in MealNames.AllowedMealNames)
            {
                if (string.Equals(allowed, name, StringComparison.OrdinalIgnoreCase))
                    return allowed; // zachowaj oryginalną wersję z constants (np. "śniadanie")
            }
        }

        return normalized;
    }
}
