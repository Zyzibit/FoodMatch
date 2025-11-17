using inzynierka.Receipts.Extensions.Requests;
using inzynierka.Receipts.Extensions.Responses;

namespace inzynierka.Receipts.Extensions.Services;

public interface IRecipeGeneratorService
{
    Task<GenerateRecipeResponse> GenerateRecipeAsync(GenerateRecipeRequest request);
}

