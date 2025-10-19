# Instrukcja testowania modułu Receipt w Postman

## Krok 1: Zaimportuj pliki do Postman

1. Otwórz Postman
2. Kliknij **Import**
3. Wybierz pliki:
   - `Receipts.postman_collection.json`
   - `Receipts.postman_environment.json`

## Krok 2: Ustaw środowisko

1. W prawym górnym rogu wybierz środowisko **"Receipts Environment"**
2. Kliknij ikonę oka aby zobaczyć zmienne

## Krok 3: Uzyskaj token dostępu (AccessToken)

### Opcja A: Użyj endpointu logowania z modułu Auth

Musisz najpierw się zalogować, aby uzyskać `accessToken`:

**Request:**
```
POST {{baseUrl}}/api/v1/auth/login
Content-Type: application/json

{
  "email": "twoj@email.com",
  "password": "twojeHaslo"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "...",
  "userId": "..."
}
```

### Opcja B: Ręcznie ustaw token w zmiennej środowiskowej

1. Kliknij ikonę oka (obok wyboru środowiska)
2. Znajdź zmienną `accessToken`
3. Kliknij **Edit**
4. Wklej token w pole `CURRENT VALUE`
5. Zapisz

## Krok 4: Testuj endpointy

### Endpointy NIE wymagające autoryzacji:
- ✅ `GET /api/v1/receipts` - Pobierz wszystkie przepisy
- ✅ `GET /api/v1/receipts/{id}` - Pobierz przepis po ID

### Endpointy WYMAGAJĄCE autoryzacji (token):
- 🔒 `POST /api/v1/receipts` - Utwórz przepis ręcznie
- 🔒 `POST /api/v1/receipts/generate-with-ai` - Wygeneruj przepis przez AI
- 🔒 `GET /api/v1/receipts/me` - Pobierz moje przepisy

## Krok 5: Testowanie generowania przepisu przez AI

1. Upewnij się, że masz ustawiony `accessToken`
2. Wybierz request **"Generate Recipe with AI"**
3. Sprawdź header `Authorization: Bearer {{accessToken}}`
4. **WAŻNE:** Teraz zamiast nazw składników podajesz **ID produktów z bazy danych**
5. Kliknij **Send**

### Przykładowe body dla AI (NOWY FORMAT):

**Podstawowy przepis z produktami z bazy:**
```json
{
  "productIds": [1, 2, 3, 4, 5],
  "cuisineType": "włoska",
  "desiredServings": 4,
  "maxPreparationTimeMinutes": 45,
  "additionalInstructions": "Preferuję proste przepisy"
}
```

**Przepis wegański:**
```json
{
  "productIds": [10, 15, 20, 25],
  "preferences": {
    "isVegetarian": true,
    "isVegan": true,
    "maxCalories": 500
  },
  "cuisineType": "azjatycka",
  "desiredServings": 2
}
```

**Przepis bezglutenowy z alergiami:**
```json
{
  "productIds": [30, 35, 40, 45, 50],
  "preferences": {
    "isGlutenFree": true,
    "allergies": ["orzechy", "soja"],
    "dislikedIngredients": ["grzyby"]
  },
  "cuisineType": "polska",
  "desiredServings": 4,
  "maxPreparationTimeMinutes": 60
}
```

### 🎯 Co się zmieni w odpowiedzi:

**PRZED (stara wersja):**
```json
{
  "ingredients": [],  // ❌ Puste!
  "additionalProducts": [
    "makaron (400,0 g)",
    "jajka (4,0 szt.)"
  ]
}
```

**PO (nowa wersja):**
```json
{
  "ingredients": [  // ✅ Wypełnione z ID produktów!
    {
      "productId": 1,
      "unitId": 1,
      "quantity": 400
    },
    {
      "productId": 2,
      "unitId": 1,
      "quantity": 4
    }
  ],
  "additionalProducts": [
    "makaron (400,0 g)",
    "jajka (4,0 szt.)"
  ]
}
```

### 📝 Jak to działa:

1. **Użytkownik wybiera produkty w UI** - dostaje listę ID (np. [1, 2, 3])
2. **Backend pobiera produkty z bazy** - po ID
3. **AI generuje przepis** - używając nazw produktów
4. **Receipt zapisuje się z `ingredients`** - zawierającymi ID produktów i ilości z AI
5. **Frontend może wyświetlić pełne info** - bo ma ID produktów

## Troubleshooting

### Błąd 401 Unauthorized
- ❌ Brak tokenu lub token wygasł
- ✅ Zaloguj się ponownie i zaktualizuj `accessToken` w zmiennych środowiskowych

### Błąd 500 Internal Server Error
- ❌ Problem z połączeniem do OpenAI API
- ✅ Sprawdź konfigurację `AI:ApiKey` w `appsettings.json`

### Błąd 400 Bad Request
- ❌ Nieprawidłowe dane w request body
- ✅ Sprawdź format JSON i wymagane pola

## Automatyczne zapisywanie ID przepisu

Kolekcja automatycznie zapisuje ID utworzonego przepisu w zmiennej `lastReceiptId`.
Możesz od razu użyć endpointu **"Get Receipt by ID"** aby zobaczyć szczegóły.

## Konfiguracja AI (appsettings.json)

Upewnij się, że masz skonfigurowane:

```json
{
  "AI": {
    "ApiKey": "twój-openai-api-key",
    "ApiLink": "https://api.openai.com/v1/chat/completions",
    "Model": "gpt-4",
    "Temperature": "0.7"
  }
}
```
