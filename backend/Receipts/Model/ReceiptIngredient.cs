using inzynierka.Products.Model;

namespace inzynierka.Receipts.Model;

public class ReceiptIngredient {
    public int ReceiptId { get; set; }
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }
    
    
    public decimal? NormalizedQuantityInGrams { get; set; }
    
    public Unit Unit { get; set; }
    public Receipt Receipt { get; set; }
    public Product Product { get; set; }
    
}