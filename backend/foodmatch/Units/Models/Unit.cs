using System.ComponentModel.DataAnnotations;

namespace inzynierka.Units.Models;

public class Unit
{
    [Key] public int UnitId { get; set; }

    [Required] [MinLength(1)] public required string Name { get; set; }

    [Required] public string Description { get; set; } = string.Empty;

    [Required] public string PromptDescription { get; set; } = string.Empty;
}