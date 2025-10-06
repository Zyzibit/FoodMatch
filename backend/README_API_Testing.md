# ?? Testowanie Kontrolerów - Przewodnik

## ?? Dostêpne Endpointy dla Testów

### ??? **Products API** (`/api/v1/products`)

#### **1. Pobranie produktu po ID**
```http
GET /api/v1/products/{productId}
```
**Przyk³ad:**
```
GET /api/v1/products/1
```

#### **2. Wyszukiwanie produktów**
```http
GET /api/v1/products/search?query=milk&limit=10&offset=0
GET /api/v1/products/search?query=bread&categories=bakery&limit=5
GET /api/v1/products/search?brand=nestle&allergens=gluten,lactose
```

#### **3. Produkty wed³ug kategorii**
```http
GET /api/v1/products/category/dairy?limit=10&offset=0
GET /api/v1/products/category/beverages
```

#### **4. Listy pomocnicze**
```http
GET /api/v1/products/categories
GET /api/v1/products/allergens  
GET /api/v1/products/ingredients
```

#### **5. Informacje ¿ywieniowe**
```http
GET /api/v1/products/1/nutrition
```

#### **6. Import produktów** (wymagana autoryzacja Admin)
```http
POST /api/v1/products/import
Content-Type: application/json

{
  "filePath": "path/to/products.jsonl",
  "batchSize": 1000
}
```

### ?? **Auth API** (`/api/v1/auth`)

#### **1. Rejestracja**
```http
POST /api/v1/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com", 
  "password": "Password123!"
}
```

#### **2. Logowanie**
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "Password123!"
}
```

#### **3. Walidacja tokenu**
```http
POST /api/v1/auth/validate-token
Content-Type: application/json

"your-jwt-token-here"
```

#### **4. Informacje o u¿ytkowniku**
```http
GET /api/v1/auth/user/123
Authorization: Bearer your-jwt-token
```

#### **5. Moje informacje**
```http
GET /api/v1/auth/me
Authorization: Bearer your-jwt-token
```

### ?? **AI API** (`/api/v1/ai`)

#### **1. Generowanie tekstu**
```http
POST /api/v1/ai/generate-text
Authorization: Bearer your-jwt-token
Content-Type: application/json

{
  "messages": [
    {
      "role": "user",
      "content": "Opisz korzyœci zdrowotne jab³ek"
    }
  ],
  "model": "gpt-3.5-turbo",
  "temperature": 0.7
}
```

#### **2. Analiza produktu**
```http
POST /api/v1/ai/analyze-product/1
Authorization: Bearer your-jwt-token
Content-Type: application/json

{
  "analysisType": "Nutritional"
}
```

#### **3. Rekomendacje przepisów**
```http
POST /api/v1/ai/recipe-recommendations
Authorization: Bearer your-jwt-token
Content-Type: application/json

{
  "ingredients": ["milk", "flour", "eggs"],
  "preferences": {
    "isVegetarian": true,
    "maxCalories": 500
  }
}
```

#### **4. Detekcja alergenów**
```http
POST /api/v1/ai/detect-allergens
Authorization: Bearer your-jwt-token
Content-Type: application/json

{
  "ingredients": ["wheat flour", "milk", "eggs"]
}
```

---

## ?? **Sposoby Testowania**

### **1. ?? Swagger UI (Naj³atwiejsze)**
```bash
# Uruchom aplikacjê
dotnet run

# PrzejdŸ do:
https://localhost:5001/swagger
```

### **2. ?? Postman/Insomnia**
- Zaimportuj powy¿sze endpointy
- Ustaw `Authorization: Bearer {token}` dla chronionych endpointów

### **3. ?? curl**
```bash
# Test rejestracji
curl -X POST "https://localhost:5001/api/v1/auth/register" \
     -H "Content-Type: application/json" \
     -d '{"username":"testuser","email":"test@example.com","password":"Password123!"}'

# Test wyszukiwania produktów
curl -X GET "https://localhost:5001/api/v1/products/search?query=milk&limit=5"

# Test z autoryzacj¹
curl -X GET "https://localhost:5001/api/v1/auth/me" \
     -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### **4. ?? PowerShell (Windows)**
```powershell
# Test rejestracji
$body = @{
    username = "testuser"
    email = "test@example.com"
    password = "Password123!"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/v1/auth/register" `
                  -Method POST `
                  -Body $body `
                  -ContentType "application/json"

# Test wyszukiwania
Invoke-RestMethod -Uri "https://localhost:5001/api/v1/products/search?query=milk"
```

### **5. ?? Browser (GET endpointy)**
```
# Bezpoœrednio w przegl¹darce:
https://localhost:5001/api/v1/products/categories
https://localhost:5001/api/v1/products/search?query=bread
https://localhost:5001/api/v1/products/1
```

---

## ?? **Scenariusze Testowe**

### **?? Scenariusz 1: Pe³ny cykl u¿ytkownika**
1. **Rejestracja** ? `POST /api/v1/auth/register`
2. **Logowanie** ? `POST /api/v1/auth/login` (otrzymaj token)
3. **Wyszukaj produkty** ? `GET /api/v1/products/search?query=milk`
4. **Pobierz szczegó³y produktu** ? `GET /api/v1/products/{id}`
5. **Analiza AI** ? `POST /api/v1/ai/analyze-product/{id}` (z tokenem)

### **?? Scenariusz 2: Test bez autoryzacji**
1. **Kategorie** ? `GET /api/v1/products/categories`
2. **Alergeny** ? `GET /api/v1/products/allergens`
3. **Wyszukiwanie** ? `GET /api/v1/products/search?query=bread`

### **?? Scenariusz 3: Test z autoryzacj¹**
1. **Zaloguj siê** ? Otrzymaj token
2. **Moje dane** ? `GET /api/v1/auth/me`
3. **Generuj tekst AI** ? `POST /api/v1/ai/generate-text`
4. **Przepisy** ? `POST /api/v1/ai/recipe-recommendations`

---

## ?? **Uwagi Testowe**

### **?? Autoryzacja:**
- Niektóre endpointy wymagaj¹ tokenu JWT
- Token otrzymujesz po logowaniu
- Dodaj `Authorization: Bearer {token}` do nag³ówka

### **?? Dane testowe:**
- Baza danych jest resetowana przy starcie (`EnsureDeleted()`)
- SprawdŸ `DbSeeder.cs` dla danych pocz¹tkowych
- Mo¿esz dodaæ w³asne dane testowe

### **?? HTTPS:**
- Aplikacja dzia³a na HTTPS (port 5001)
- W produkcji upewnij siê, ¿e certyfikaty s¹ prawid³owe

---

## ?? **Przyk³ad Kompletnego Testu**

```bash
#!/bin/bash
# Kompletny skrypt testowy

# 1. Rejestracja
echo "=== REJESTRACJA ==="
curl -X POST "https://localhost:5001/api/v1/auth/register" \
     -H "Content-Type: application/json" \
     -d '{"username":"testuser123","email":"test123@example.com","password":"Password123!"}' \
     -k -s | jq

# 2. Logowanie  
echo -e "\n=== LOGOWANIE ==="
TOKEN=$(curl -X POST "https://localhost:5001/api/v1/auth/login" \
             -H "Content-Type: application/json" \
             -d '{"username":"testuser123","password":"Password123!"}' \
             -k -s | jq -r '.accessToken')

echo "Token: $TOKEN"

# 3. Test produktów
echo -e "\n=== WYSZUKIWANIE PRODUKTÓW ==="
curl -X GET "https://localhost:5001/api/v1/products/search?query=milk&limit=3" \
     -k -s | jq

# 4. Test z autoryzacj¹
echo -e "\n=== MOJE DANE ==="
curl -X GET "https://localhost:5001/api/v1/auth/me" \
     -H "Authorization: Bearer $TOKEN" \
     -k -s | jq
```

Wybierz metodê, która Ci najbardziej odpowiada i zacznij testowaæ! ??