namespace inzynierka.Products.Contracts.Models;

public class NutritionInfo
{
    public double? Energy { get; set; }
    public double? Fat { get; set; }
    public double? SaturatedFat { get; set; }
    public double? Carbohydrates { get; set; }
    public double? Sugars { get; set; }
    public double? Fiber { get; set; }
    public double? Proteins { get; set; }
    public double? Salt { get; set; }
    public double? Sodium { get; set; }
    public decimal? EstimatedCalories { get; set; }
    public decimal? EstimatedProteins { get; set; }
    public decimal? EstimatedCarbohydrates { get; set; }
    public decimal? EstimatedFats { get; set; }
}