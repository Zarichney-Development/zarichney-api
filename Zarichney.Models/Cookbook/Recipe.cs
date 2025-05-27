using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Zarichney.Models.Cookbook;

public class Recipe
{
    public string? Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public required string Title { get; set; }
    
    [Required]
    public required string Description { get; set; }
    
    [JsonPropertyName("prep_time")]
    public required string PrepTime { get; set; }
    
    [JsonPropertyName("cook_time")]
    public required string CookTime { get; set; }
    
    public required List<string> Ingredients { get; set; }
    public required List<string> Directions { get; set; }
}