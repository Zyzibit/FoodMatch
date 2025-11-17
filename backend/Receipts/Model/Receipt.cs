﻿using System.ComponentModel.DataAnnotations;
using inzynierka.Users.Model;

namespace inzynierka.Receipts.Extensions.Model;

public class Receipt {
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; }
    public User User { get; set; }
    
    [Required]
    public ReceiptSource Source { get; set; } = ReceiptSource.User;
    
    public ICollection<ReceiptIngredient> Ingredients { get; set; }
    
    public List <string>? AdditionalProducts { get; set; }
    
    [Required]
    [MinLength(3)]
    public required string Title { get; set; }
    
    public string Description { get; set; }
    [Required]
    public required string Instructions { get; set; }
    
    public int Servings { get; set; }
    public int PreparationTimeMinutes { get; set; }
    
    [Required]
    public int TotalWeightGrams { get; set; }
    
    [Required]
    public decimal Calories { get; set; }
    [Required]
    public decimal Protein { get; set; }
    [Required]
    public decimal Carbohydrates { get; set; }
    [Required]
    public decimal Fats { get; set; }
    
    public DateTime CreatedAt { get; set; }
}
