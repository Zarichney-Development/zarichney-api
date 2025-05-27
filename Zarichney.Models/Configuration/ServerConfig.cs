using System.ComponentModel.DataAnnotations;

namespace Zarichney.Models.Configuration;

public class ServerConfig
{
    [Required]
    [Url]
    public string BaseUrl { get; init; } = string.Empty;
}