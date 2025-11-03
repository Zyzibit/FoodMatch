using inzynierka.AI.OpenAI.Model;
using inzynierka.Receipts.Requests;

namespace inzynierka.Receipts.Services;

public interface IRecipePromptBuilder
{
    Task<List<OpenAIMessage>> BuildMessagesAsync(GenerateRecipeRequest request);
}

