
using Microsoft.AspNetCore.Identity;

namespace inzynierka.Products.Model;

public class User : IdentityUser {

    public string Name { get; set; } = string.Empty;
    
}