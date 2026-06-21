using System.ComponentModel.DataAnnotations;

namespace inzynierka.Auth.Model.DTO;

public class TokenModel
{
    [Required]
    public string AccessToken { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}