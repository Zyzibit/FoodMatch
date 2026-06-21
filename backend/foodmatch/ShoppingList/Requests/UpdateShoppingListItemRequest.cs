using System.ComponentModel.DataAnnotations;

namespace inzynierka.ShoppingList.Requests;

public class UpdateShoppingListItemRequest
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public decimal Quantity { get; init; }
    
    [Required(ErrorMessage = "Unit is required")]
    public int UnitId { get; init; }
}

