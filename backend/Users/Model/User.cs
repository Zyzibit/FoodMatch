using Microsoft.AspNetCore.Identity;

namespace inzynierka.Users.Model;

public class User : IdentityUser {

    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
}