using System.ComponentModel.DataAnnotations;

namespace inzynierka.Receipts.Contracts.Models;

public class CreateUnitRequest
{
    [Required(ErrorMessage = "Unit name is required")]
    [MinLength(1, ErrorMessage = "Unit name must be at least 1 character")]
    [MaxLength(50, ErrorMessage = "Unit name can be maximum 50 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Unit description is required")]
    [MaxLength(200, ErrorMessage = "Unit description can be maximum 200 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prompt description is required")]
    [MaxLength(500, ErrorMessage = "Prompt description can be maximum 500 characters")]
    public string PromptDescription { get; set; } = string.Empty;
}
