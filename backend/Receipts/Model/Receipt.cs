using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using inzynierka.Users.Model;

namespace inzynierka.Receipts.Model;

public class Receipt {
    public int Id { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    
    [Required]
    public bool IsAiGenerated { get; set; }
    
    public ICollection<ReceiptIngredient> Ingredients { get; set; }
    
    public List <string>? AdditionalProducts { get; set; }
    
    [Required]
    [MinLength(3)]
    public required string Title { get; set; }
    
    public string Description { get; set; }
    [Required]
    public required string Instructions { get; set; }
    
    public int Servings { get; set; }
    public int PreparationTimeMinutes { get; set; }
    
    [Required]
    public int TotalWeightGrams { get; set; }
    
    [Required]
    public int Calories { get; set; }
    [Required]
    public int Protein { get; set; }
    [Required]
    public int Carbohydrates { get; set; }
    [Required]
    public int Fats { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
