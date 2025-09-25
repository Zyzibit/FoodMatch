namespace inzynierka.API.Shared.Model;
using inzynierka.API.Product.Model;
using inzynierka.API.User.Model;

public class UserProduct {
    public string UserId { get; set; }
    public User User { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
}