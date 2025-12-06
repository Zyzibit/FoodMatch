using System.ComponentModel.DataAnnotations;
namespace inzynierka.ShoppingList.Requests;

public class AddProductToShoppingListRequest
{
    [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    [Required]
    public decimal Quantity { get; set; }

    [Required] public int ProductId { get; set; }

}

