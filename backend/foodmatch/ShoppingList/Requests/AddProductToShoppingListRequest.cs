using System.ComponentModel.DataAnnotations;
namespace inzynierka.ShoppingList.Requests;

public class AddProductToShoppingListRequest
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    [Required]
    public decimal Quantity { get; init; }
    
    public int? ProductId { get; init; }
    
    [Required(ErrorMessage = "Product name is required")]
    [MaxLength(255)]
    public string ProductName { get; init; } = string.Empty;
    
    [Required(ErrorMessage = "Unit is required")]
    public int UnitId { get; init; }
}

