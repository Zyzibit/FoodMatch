using inzynierka.ShoppingList.Responses;

namespace inzynierka.ShoppingList.Extensions;

public static class ShoppingListExtensions
{
    public static ShoppingListResponse ToResponse(this Model.ShoppingList shoppingList)
    {
        return new ShoppingListResponse
        {
            Id = shoppingList.Id,
            UserId = shoppingList.UserId,
            Items = shoppingList.Items.Select(i => i.ToResponse()).ToList()
        };
    }

    public static ShoppingListItemResponse ToResponse(this ShoppingListItem item)
    {
        return new ShoppingListItemResponse
        {
            Id = item.Id,
            Quantity = item.Quantity,
            ProductId = item.ProductId,
            ProductName = item.Product.ProductName,
            ProductCode = item.Product.Code,
            ImageUrl = item.Product.ImageUrl,
            Brands = item.Product.Brands,
            Source = item.Product.Source.ToString()
        };
    }
}

