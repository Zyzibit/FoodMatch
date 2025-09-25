using System.ComponentModel.DataAnnotations;

namespace inzynierka.API.User.Model.DTO;

public class SignupModel
{
    [Required]
    [MaxLength(40)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Password { get; set; } = string.Empty;
}