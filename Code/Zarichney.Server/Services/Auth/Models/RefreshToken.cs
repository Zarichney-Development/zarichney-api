using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zarichney.Services.Auth.Models;

public class RefreshToken
{
  [Key] public int Id { get; init; }

  [Required][MaxLength(128)] public string UserId { get; init; } = string.Empty;

  [Required][MaxLength(256)] public string Token { get; init; } = string.Empty;

  public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

  public DateTime ExpiresAt { get; init; }

  public bool IsUsed { get; set; }

  public bool IsRevoked { get; set; }

  [MaxLength(100)] public string? DeviceName { get; init; }

  [MaxLength(45)] public string? DeviceIp { get; init; }

  [MaxLength(512)] public string? UserAgent { get; init; }

  public DateTime? LastUsedAt { get; set; }

  [ForeignKey(nameof(UserId))] public ApplicationUser? User { get; init; }
}
