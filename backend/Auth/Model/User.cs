
using Microsoft.AspNetCore.Identity;

namespace inzynierka.Auth.Model;

public class User : IdentityUser {

    public string Name { get; set; } = string.Empty;
    
}