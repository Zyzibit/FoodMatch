using System.Text.Json.Serialization;

namespace inzynierka.Recipes.Requests;

public class SearchRecipesRequest
{
    [JsonPropertyName("searchTerm")]
    public string? SearchTerm { get; set; }
    
    [JsonPropertyName("limit")]
    public int Limit { get; set; } = 50;
    
    [JsonPropertyName("offset")]
    public int Offset { get; set; } = 0;
    
    [JsonPropertyName("isPublicOnly")]
    public bool IsPublicOnly { get; set; } = false;
    
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }
}

