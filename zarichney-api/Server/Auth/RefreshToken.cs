using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zarichney.Server.Auth;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Token { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; }
    
    public bool IsUsed { get; set; } = false;
    
    public bool IsRevoked { get; set; } = false;
    
    // New properties for multiple device support
    public string? DeviceName { get; set; }
    
    public string? DeviceIp { get; set; }
    
    public string? UserAgent { get; set; }
    
    public DateTime? LastUsedAt { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }
}