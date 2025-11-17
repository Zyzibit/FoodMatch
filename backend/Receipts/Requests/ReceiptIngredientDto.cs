namespace inzynierka.Receipts.Extensions.Requests;

public class ReceiptIngredientDto
{
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public decimal NormalizedQuantityInGrams { get; set; }
    public decimal Quantity { get; set; }
}