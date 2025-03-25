using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zarichney.Server.Auth;

public class ApiKey
{
  [Key]
  public int Id { get; set; }

  [Required]
  [MaxLength(64)]
  public string KeyValue { get; set; } = string.Empty;

  [Required]
  [MaxLength(100)]
  public string Name { get; set; } = string.Empty;

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  public DateTime? ExpiresAt { get; set; }

  public bool IsActive { get; set; } = true;

  [MaxLength(255)]
  public string? Description { get; set; }

  [Required]
  [ForeignKey("User")]
  public string UserId { get; set; } = string.Empty;

  public virtual ApplicationUser? User { get; set; }
}