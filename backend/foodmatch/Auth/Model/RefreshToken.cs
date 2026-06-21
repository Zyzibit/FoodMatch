using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using inzynierka.Users.Model;

namespace inzynierka.Auth.Model;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
    
    [Required]
    public DateTime ExpiryDate { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? RevokedAt { get; set; }
    
    public string? DeviceId { get; set; }
    
    public string? UserAgent { get; set; }
    
    public string? IpAddress { get; set; }
    
    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiryDate;
    
    public bool IsRevoked => RevokedAt != null;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
}