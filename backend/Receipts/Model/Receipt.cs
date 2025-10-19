using System.ComponentModel.DataAnnotations.Schema;
using inzynierka.Users.Model;

namespace inzynierka.Receipts.Model;

public class Receipt {
    public int Id { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }
    
    public bool IsAiGenerated { get; set; }
    
    public ICollection<ReceiptIngredient> Ingredients { get; set; }
    public string Title { get; set; }
    
    public string Description { get; set; }
    public string Instructions { get; set; }
    
    public int Servings { get; set; }
    public int PreparationTimeMinutes { get; set; }
    
    public int Calories { get; set; }
    public int Protein { get; set; }
    public int Carbohydrates { get; set; }
    public int Fats { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

