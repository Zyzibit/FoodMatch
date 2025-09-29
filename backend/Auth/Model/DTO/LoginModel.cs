using System.ComponentModel.DataAnnotations;

namespace inzynierka.Auth.Model.DTO;

public class LoginModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}