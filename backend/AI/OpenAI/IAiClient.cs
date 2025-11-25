using System.Text.Json;

namespace inzynierka.AI.OpenAI;

/// <summary>
/// Interfejs klienta OpenAI do komunikacji z API OpenAI z obsługą odpowiedzi w formacie JSON
/// </summary>
public interface IAiClient
{
    /// <summary>
    /// Wysyła prompt do OpenAI z wymuszeniem odpowiedzi w formacie JSON
    /// </summary>
    /// <param name="systemMessage">Wiadomość systemowa określająca kontekst i zadanie</param>
    /// <param name="userMessage">Wiadomość użytkownika z konkretnym zapytaniem</param>
    /// <returns>Odpowiedź w formacie JSON jako JsonElement lub null w przypadku błędu</returns>
    Task<JsonElement?> SendPromptForJsonAsync(string systemMessage, string userMessage);
}
