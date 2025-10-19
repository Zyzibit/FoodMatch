using System.ComponentModel.DataAnnotations;

namespace inzynierka.Receipts.Model;

public class Unit
{
    [Key]
    public int UnitId { get; set; }
    [Required]
    [MinLength(1)]
    public string Name { get; set; }
}