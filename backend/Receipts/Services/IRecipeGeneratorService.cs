using inzynierka.Receipts.Requests;
using inzynierka.Receipts.Responses;

namespace inzynierka.Receipts.Services;

public interface IRecipeGeneratorService
{
    Task<GenerateRecipeResponse> GenerateRecipeAsync(GenerateRecipeRequest request);
}

