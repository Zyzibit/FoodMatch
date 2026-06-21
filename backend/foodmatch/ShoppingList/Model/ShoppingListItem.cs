using System.ComponentModel.DataAnnotations;
using foodmatch.Products.Model;
using foodmatch.Units.Model;

namespace foodmatch.ShoppingList.Model;

public class ShoppingListItem
{
    public int Id { get; set; }
    
    [Required]
    public decimal Quantity { get; set; }
    public int? ProductId { get; set; }
   
    [Required]
    [MaxLength(255)]
    public string ProductName { get; set; } = string.Empty;
    
    [Required]
    public int UnitId { get; set; }
    
    public Product? Product { get; set; }
    
    public Unit Unit { get; set; } = null!;
}