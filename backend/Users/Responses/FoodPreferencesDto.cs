namespace inzynierka.Users.Responses;

public class FoodPreferencesDto {
    public bool? IsVegan { get; set; }
    public bool? IsVegetarian { get; set; }
    public bool? HasGlutenIntolerance { get; set; }
    public bool? HasLactoseIntolerance { get; set; }
    public List <string>? Allergies { get; set; }
    
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public string? ActivityLevel { get; set; }
    
    // Cele dzienne
    public int? DailyProteinGoal { get; set; }
    public int? DailyCarbohydrateGoal { get; set; }
    public int? DailyFatGoal { get; set; }
    public int? DailyCalorieGoal { get; set; }
    
    // Procentowe rozkłady dla kalorii
    public int? BreakfastCaloriePercentage { get; set; }
    public int? LunchCaloriePercentage { get; set; }
    public int? DinnerCaloriePercentage { get; set; }
    public int? SnackCaloriePercentage { get; set; }
    
    // Procentowe rozkłady dla białka
    public int? BreakfastProteinPercentage { get; set; }
    public int? LunchProteinPercentage { get; set; }
    public int? DinnerProteinPercentage { get; set; }
    public int? SnackProteinPercentage { get; set; }
    
    // Procentowe rozkłady dla węglowodanów
    public int? BreakfastCarbohydratePercentage { get; set; }
    public int? LunchCarbohydratePercentage { get; set; }
    public int? DinnerCarbohydratePercentage { get; set; }
    public int? SnackCarbohydratePercentage { get; set; }
    
    // Procentowe rozkłady dla tłuszczów
    public int? BreakfastFatPercentage { get; set; }
    public int? LunchFatPercentage { get; set; }
    public int? DinnerFatPercentage { get; set; }
    public int? SnackFatPercentage { get; set; }
    
    // Obliczone wartości
    public int? CalculatedBMR { get; set; }
    public int? CalculatedDailyCalories { get; set; }
    
    // Cele kaloryczne dla poszczególnych posiłków
    public int? BreakfastCalories { get; set; }
    public int? LunchCalories { get; set; }
    public int? DinnerCalories { get; set; }
    public int? SnackCalories { get; set; }
    
    // Cele białka dla poszczególnych posiłków
    public int? BreakfastProteinGoal { get; set; }
    public int? LunchProteinGoal { get; set; }
    public int? DinnerProteinGoal { get; set; }
    public int? SnackProteinGoal { get; set; }
    
    // Cele węglowodanów dla poszczególnych posiłków
    public int? BreakfastCarbohydrateGoal { get; set; }
    public int? LunchCarbohydrateGoal { get; set; }
    public int? DinnerCarbohydrateGoal { get; set; }
    public int? SnackCarbohydrateGoal { get; set; }
    
    // Cele tłuszczów dla poszczególnych posiłków
    public int? BreakfastFatGoal { get; set; }
    public int? LunchFatGoal { get; set; }
    public int? DinnerFatGoal { get; set; }
    public int? SnackFatGoal { get; set; }
}