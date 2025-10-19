# Rozwiązanie błędu 429 (Too Many Requests) - OpenAI API

## 🔴 Problem
Błąd **429 Too Many Requests** oznacza, że przekroczono limit zapytań do OpenAI API.

## 🔍 Możliwe przyczyny:

### 1. **Rate Limit - zbyt wiele zapytań w krótkim czasie**
OpenAI ma limity liczby zapytań na minutę (RPM - Requests Per Minute):
- **Free tier**: 3 RPM
- **Tier 1**: 500 RPM  
- **Tier 2+**: wyższe limity

### 2. **Brak środków na koncie OpenAI**
- Konto może nie mieć ustawionej karty płatniczej
- Wyczerpany miesięczny limit darmowy ($5 dla nowych kont)

### 3. **Token limit (TPM - Tokens Per Minute)**
- Przekroczono limit tokenów na minutę
- Free tier: 40,000 TPM

## ✅ Rozwiązania:

### Rozwiązanie 1: Poczekaj i spróbuj ponownie
```bash
# Jeśli to tymczasowy rate limit, poczekaj 60 sekund i spróbuj ponownie
```

### Rozwiązanie 2: Sprawdź limity konta OpenAI
1. Przejdź do: https://platform.openai.com/account/limits
2. Sprawdź:
   - **Current usage** (obecne użycie)
   - **Rate limits** (limity zapytań)
   - **Usage tier** (poziom konta)

### Rozwiązanie 3: Sprawdź saldo konta
1. Przejdź do: https://platform.openai.com/account/billing/overview
2. Sprawdź czy:
   - Masz ustawioną kartę płatniczą
   - Masz wystarczające środki
   - Nie przekroczyłeś miesięcznego limitu

### Rozwiązanie 4: Użyj tańszego modelu
W pliku `appsettings.json` zmień model na:

```json
{
  "AI": {
    "Model": "gpt-3.5-turbo",  // zamiast "gpt-4o-mini"
    "Temperature": 0.7
  }
}
```

Modele od najtańszych do najdroższych:
- `gpt-3.5-turbo` - najtańszy, szybki
- `gpt-4o-mini` - tani, nowy model
- `gpt-4` - najdroższy, najlepszy

### Rozwiązanie 5: Zwiększ tier konta
Aby zwiększyć limity:
1. Doładuj konto OpenAI (min. $5)
2. Po pierwszej płatności, tier automatycznie wzrośnie po ~7 dniach
3. Wyższy tier = wyższe limity RPM/TPM

## 📊 Tabela limitów według tier:

| Tier | Wymóg | gpt-4o-mini RPM | gpt-4o-mini TPM |
|------|-------|-----------------|-----------------|
| Free | - | 3 | 40,000 |
| Tier 1 | $5+ paid | 500 | 200,000 |
| Tier 2 | $50+ paid + 7 dni | 5,000 | 2,000,000 |

## 🛠️ Co zostało naprawione w kodzie:

### 1. Lepsze logowanie błędów w `OpenAIClient.cs`:
```csharp
// Teraz pokazuje szczegóły limitu:
// - Ile zapytań pozostało
// - Kiedy limit się zresetuje
// - Link do sprawdzenia limitów
```

### 2. Przyjazne komunikaty błędów w `RecipeGeneratorService.cs`:
```csharp
// Użytkownik dostanie komunikat:
"Przekroczono limit zapytań do OpenAI API. 
Proszę spróbować ponownie za chwilę."
```

### 3. Osobna obsługa różnych błędów HTTP:
- 429 - Rate limit
- 401 - Nieprawidłowy klucz API
- 500 - Błąd serwera OpenAI

## 🧪 Testowanie po naprawie:

1. **Zrestartuj aplikację**
```bash
# Zatrzymaj aplikację (Ctrl+C)
# Uruchom ponownie
dotnet run
```

2. **Poczekaj 60 sekund** przed kolejną próbą

3. **Sprawdź logi** - teraz zobaczysz dokładniejsze informacje:
```
OpenAI API Rate Limit exceeded (429)
Remaining requests: 0
Rate limit resets at: 2025-01-19T12:30:00Z
```

## 🎯 Rekomendacje:

### Dla developmentu (testy):
```json
{
  "AI": {
    "Model": "gpt-3.5-turbo",
    "Temperature": 0.7
  }
}
```

### Dla produkcji:
```json
{
  "AI": {
    "Model": "gpt-4o-mini",
    "Temperature": 0.7
  }
}
```

## 📝 Następne kroki:

1. ✅ Sprawdź limity konta: https://platform.openai.com/account/limits
2. ✅ Sprawdź saldo: https://platform.openai.com/account/billing
3. ✅ Poczekaj 60 sekund między requestami podczas testów
4. ✅ Rozważ upgrade konta jeśli często używasz API

## 💡 Wskazówki:

- **Nie testuj zbyt często** - między requestami odczekaj min. 20 sekund
- **Używaj cache** - zapisuj wygenerowane przepisy w bazie danych
- **Monitoruj usage** - sprawdzaj zużycie tokenów na koncie OpenAI
- **Ustaw soft limit** - w ustawieniach OpenAI możesz ustawić miesięczny limit wydatków

## 🔗 Przydatne linki:

- Limity konta: https://platform.openai.com/account/limits
- Billing: https://platform.openai.com/account/billing
- Rate limits docs: https://platform.openai.com/docs/guides/rate-limits
- Pricing: https://openai.com/api/pricing/

