using System.ComponentModel.DataAnnotations;
using inzynierka.API.Shared;
using inzynierka.API.Shared.Model;
using Microsoft.AspNetCore.Identity;

namespace inzynierka.API.User.Model;

public class User : IdentityUser {

    public string Name { get; set; } = string.Empty;
    public ICollection<UserProduct> UserProducts { get; set; }
    
}