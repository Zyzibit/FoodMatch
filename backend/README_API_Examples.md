# ?? Przyk³ady U¿ycia API - FoodMatch

Ta dokumentacja zawiera praktyczne przyk³ady u¿ycia nowego REST API aplikacji FoodMatch.

## ?? Spis treœci
- [Autoryzacja](#-autoryzacja)
- [Produkty](#-produkty)  
- [Sztuczna Inteligencja](#-sztuczna-inteligencja)
- [Przep³ywy biznesowe](#-przep³ywy-biznesowe)

---

## ?? Autoryzacja

### Rejestracja nowego u¿ytkownika

```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "username": "jan.kowalski",
  "email": "jan@example.com", 
  "password": "SecurePassword123!"
}
```

**OdpowiedŸ:**
```json
{
  "success": true,
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-01-12T22:30:00Z",
  "user": {
    "userId": "12345",
    "username": "jan.kowalski",
    "email": "jan@example.com",
    "roles": ["User"]
  }
}
```

### Logowanie

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "jan.kowalski",
  "password": "SecurePassword123!"
}
```

### Walidacja tokenu

```http
POST /api/v1/auth/validate-token
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Informacje o aktualnym u¿ytkowniku

```http
GET /api/v1/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## ??? Produkty

### Wyszukiwanie produktów

#### Podstawowe wyszukiwanie
```http
GET /api/v1/products/search?query=mleko&limit=10&offset=0
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Wyszukiwanie z filtrami
```http
GET /api/v1/products/search?query=jogurt&categories=dairy,organic&allergens=lactose&limit=20
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**OdpowiedŸ:**
```json
{
  "products": [
    {
      "id": "prod123",
      "name": "Jogurt naturalny",
      "brand": "Danone",
      "barcode": "1234567890123",
      "imageUrl": "https://example.com/image.jpg",
      "categories": ["dairy", "organic"],
      "ingredients": ["mleko", "kultury bakterii"],
      "allergens": ["lactose"],
      "countries": ["Poland"],
      "nutritionGrade": "A",
      "ecoScoreGrade": "B"
    }
  ],
  "totalCount": 156,
  "hasMore": true
}
```

### Szczegó³y produktu

```http
GET /api/v1/products/prod123
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Produkty wed³ug kategorii

```http
GET /api/v1/products/category/dairy?limit=15&offset=0
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Listy pomocnicze

```http
# Lista kategorii
GET /api/v1/products/categories

# Lista alergenów  
GET /api/v1/products/allergens

# Lista sk³adników
GET /api/v1/products/ingredients
```

### Informacje ¿ywieniowe

```http
GET /api/v1/products/prod123/nutrition
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**OdpowiedŸ:**
```json
{
  "energy": 250.5,
  "fat": 12.3,
  "saturatedFat": 8.1,
  "carbohydrates": 15.2,
  "sugars": 14.8,
  "fiber": 0.0,
  "proteins": 8.9,
  "salt": 0.12,
  "sodium": 0.048
}
```

---

## ?? Sztuczna Inteligencja

### Generowanie tekstu AI

```http
POST /api/v1/ai/generate-text
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "messages": [
    {
      "role": "system",
      "content": "Jesteœ ekspertem ¿ywieniowym."
    },
    {
      "role": "user", 
      "content": "Jakie s¹ korzyœci zdrowotne z jedzenia jogurtu?"
    }
  ],
  "model": "gpt-4",
  "temperature": 0.7,
  "maxTokens": 500,
  "language": "pl"
}
```

### Analiza produktu

```http
POST /api/v1/ai/analyze-product/prod123
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "analysisType": "Nutritional"
}
```

**Typy analizy:**
- `Nutritional` - Analiza ¿ywieniowa
- `Allergens` - Analiza alergenów  
- `Ingredients` - Analiza sk³adników
- `Environmental` - Wp³yw na œrodowisko
- `Health` - Ocena zdrowotna
- `Dietary` - Kompatybilnoœæ z dietami

**OdpowiedŸ:**
```json
{
  "success": true,
  "analysis": "Ten jogurt jest doskona³ym Ÿród³em probiotyków...",
  "confidenceScore": 0.92,
  "tags": ["probiotic", "high-protein", "low-sugar"],
  "metadata": {
    "processing_time_ms": 1234,
    "model_version": "v2.1",
    "analysis_date": "2025-01-12T22:15:00Z"
  }
}
```

### Rekomendacje przepisów

```http
POST /api/v1/ai/recipe-recommendations
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "ingredients": ["jogurt", "owoce", "miód", "orzechy"],
  "preferences": {
    "isVegetarian": true,
    "isVegan": false,
    "isGlutenFree": true,
    "isLactoseFree": false,
    "allergies": ["nuts"],
    "dislikedIngredients": ["cebula"],
    "cuisineType": "mediterranean",
    "maxCalories": 500
  }
}
```

**OdpowiedŸ:**
```json
{
  "success": true,
  "recommendations": [
    {
      "name": "Œniadaniowa miska z jogurtem",
      "description": "Lekka i zdrowa miska œniadaniowa",
      "ingredients": ["jogurt grecki", "miód", "owoce sezonowe"],
      "instructions": [
        "Umieœæ jogurt w misce",
        "Dodaj pokrojone owoce", 
        "Polej miodem"
      ],
      "prepTimeMinutes": 5,
      "cookTimeMinutes": 0,
      "servings": 1,
      "matchScore": 0.95
    }
  ]
}
```

### Analiza ¿ywieniowa AI

```http
GET /api/v1/ai/nutrition-analysis/prod123
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Detekcja alergenów

```http
POST /api/v1/ai/detect-allergens
Content-Type: application/json
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

{
  "ingredients": ["wheat flour", "eggs", "milk", "nuts"]
}
```

**OdpowiedŸ:**
```json
{
  "success": true,
  "detectedAllergens": [
    {
      "name": "gluten",
      "confidence": 0.98,
      "source": "wheat flour",
      "severity": "high"
    },
    {
      "name": "lactose",
      "confidence": 0.95,
      "source": "milk",
      "severity": "medium"
    }
  ]
}
```

### Ocena zdrowotna

```http
GET /api/v1/ai/health-score/prod123
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**OdpowiedŸ:**
```json
{
  "success": true,
  "score": 8.2,
  "grade": "A-",
  "positiveAspects": [
    "Wysoka zawartoœæ bia³ka",
    "Probiotyki wspomagaj¹ce trawienie",
    "Niski indeks glikemiczny"
  ],
  "negativeAspects": [
    "Zawiera laktozê",
    "Relatywnie wysoka zawartoœæ t³uszczów nasyconych"
  ]
}
```

---

## ?? Przep³ywy biznesowe

### Scenariusz 1: Nowy u¿ytkownik szuka zdrowych produktów

```javascript
// 1. Rejestracja
const registerResponse = await fetch('/api/v1/auth/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'healthyUser',
    email: 'user@example.com',
    password: 'SecurePass123!'
  })
});

const { accessToken } = await registerResponse.json();

// 2. Wyszukanie zdrowych produktów
const searchResponse = await fetch('/api/v1/products/search?query=zdrowy&limit=10', {
  headers: { 'Authorization': `Bearer ${accessToken}` }
});

const { products } = await searchResponse.json();

// 3. Analiza wybranego produktu
const analysisResponse = await fetch(`/api/v1/ai/analyze-product/${products[0].id}`, {
  method: 'POST',
  headers: { 
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({ analysisType: 'Health' })
});

const analysis = await analysisResponse.json();
```

### Scenariusz 2: Planowanie posi³ku z ograniczeniami dietetycznymi

```javascript
// 1. Logowanie
const loginResponse = await fetch('/api/v1/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'veganUser',
    password: 'password123'
  })
});

const { accessToken } = await loginResponse.json();

// 2. Wyszukanie produktów wegañskich bez glutenu
const veganProducts = await fetch('/api/v1/products/search?categories=vegan,gluten-free&limit=20', {
  headers: { 'Authorization': `Bearer ${accessToken}` }
});

// 3. Sprawdzenie alergenów w sk³adnikach
const allergenCheck = await fetch('/api/v1/ai/detect-allergens', {
  method: 'POST',
  headers: { 
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    ingredients: ['tofu', 'quinoa', 'vegetables', 'olive oil']
  })
});

// 4. Generowanie przepisów
const recipes = await fetch('/api/v1/ai/recipe-recommendations', {
  method: 'POST', 
  headers: { 
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    ingredients: ['tofu', 'quinoa', 'vegetables'],
    preferences: {
      isVegan: true,
      isGlutenFree: true,
      allergies: ['nuts'],
      maxCalories: 600
    }
  })
});
```

### Scenariusz 3: Administrator importuje nowe produkty

```javascript
// 1. Logowanie jako admin
const adminLogin = await fetch('/api/v1/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    username: 'admin',
    password: 'adminSecurePass'
  })
});

const { accessToken } = await adminLogin.json();

// 2. Import produktów
const importResponse = await fetch('/api/v1/products/import', {
  method: 'POST',
  headers: { 
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    filePath: '/data/new_products.jsonl',
    batchSize: 1000
  })
});

const importResult = await importResponse.json();
console.log(`Zaimportowano ${importResult.importedCount} produktów`);
```

---

## ?? Testowanie API z CURL

### Przyk³ady CURL:

```bash
# Rejestracja
curl -X POST "http://localhost:5000/api/v1/auth/register" \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"Test123!"}'

# Wyszukiwanie produktów
curl -X GET "http://localhost:5000/api/v1/products/search?query=mleko&limit=5" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

# Analiza produktu
curl -X POST "http://localhost:5000/api/v1/ai/analyze-product/prod123" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{"analysisType":"Nutritional"}'
```

---

## ?? Obs³uga b³êdów

### Typowe kody b³êdów:

- **400 Bad Request** - Nieprawid³owe dane wejœciowe
- **401 Unauthorized** - Brak lub nieprawid³owy token
- **403 Forbidden** - Brak uprawnieñ
- **404 Not Found** - Zasób nie istnieje
- **500 Internal Server Error** - B³¹d serwera

### Przyk³ad odpowiedzi b³êdu:

```json
{
  "success": false,
  "message": "Product not found",
  "errorCode": "PRODUCT_NOT_FOUND",
  "timestamp": "2025-01-12T22:30:00Z"
}
```

---

## ?? Limity i ograniczenia

- **Rate limiting:** 1000 ¿¹dañ/minutê na u¿ytkownika
- **Max search results:** 100 produktów na zapytanie
- **AI token limit:** 4096 tokenów na ¿¹danie
- **File upload:** Maksymalnie 10MB dla importu

---

## ?? Best Practices

1. **Zawsze u¿ywaj HTTPS w produkcji**
2. **Przechowuj tokeny bezpiecznie (nie w localStorage)**  
3. **Implementuj retry logic dla ¿¹dañ**
4. **Cachuj odpowiedzi gdy to mo¿liwe**
5. **Waliduj dane po stronie klienta**
6. **U¿ywaj proper HTTP status codes**
7. **Loguj b³êdy i metryki**

Ta dokumentacja pomo¿e Ci efektywnie wykorzystaæ nowe API FoodMatch! ??