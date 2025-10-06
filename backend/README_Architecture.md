# Modu³owa Architektura z gRPC, Kontraktami i Szyn¹ Danych

Ta dokumentacja opisuje now¹ architekturê aplikacji FoodMatch opart¹ na zasadach:
- **gRPC** - komunikacja wewnêtrzna miêdzy modu³ami  
- **REST API** - zewnêtrzny interfejs dla klientów
- **Kontrakty** - jasno zdefiniowane interfejsy miêdzy modu³ami
- **Szyna danych** - centralne zarz¹dzanie zdarzeniami i komunikacj¹

## ??? Struktura Architektury

```
backend/
??? ?? Contracts/              # Kontrakty - interfejsy komunikacji miêdzy modu³ami
?   ??? Auth/                  # Kontrakt autoryzacji
?   ??? Products/              # Kontrakt produktów
?   ??? AI/                    # Kontrakt AI
??? ?? Modules/                # Implementacje modu³ów
?   ??? Auth/                  # Modu³ autoryzacji
?   ??? Products/              # Modu³ produktów
?   ??? AI/                    # Modu³ AI
??? ?? EventBus/               # Szyna danych - zarz¹dzanie zdarzeniami
??? ?? Grpc/                   # Komunikacja wewnêtrzna gRPC
?   ??? Services/              # Implementacje serwisów gRPC
?   ??? Clients/               # Klienci gRPC
??? ?? Controllers/API/        # REST API - interfejs zewnêtrzny
??? ?? Protos/                 # Definicje protoko³ów gRPC
??? ?? [Existing modules...]   # Istniej¹ce modu³y (Auth, Products, AI, Data)
```

## ?? Przep³yw Komunikacji

### 1. **Zewnêtrzna komunikacja (Klient ? API)**
```
[Mobile/Web Client] ??HTTP/REST??> [REST Controllers] ??Contracts??> [Business Modules]
```

### 2. **Wewnêtrzna komunikacja (Modu³ ? Modu³)**
```
[AuthModule] ??gRPC??> [ProductsModule] ??gRPC??> [AIModule]
                    ?
              [Event Bus - Szyna Danych]
```

### 3. **Przep³yw zdarzeñ**
```
[Business Logic] ??Events??> [Event Bus] ??Notifications??> [Other Modules]
```

## ?? Modu³y i Kontrakty

### ?? Auth Module (IAuthContract)

**Odpowiedzialnoœæ:** Zarz¹dzanie autoryzacj¹ i u¿ytkownikami

**Dostêpne operacje:**
- `ValidateTokenAsync()` - Walidacja tokenu JWT
- `AuthenticateAsync()` - Logowanie u¿ytkownika
- `RegisterAsync()` - Rejestracja nowego u¿ytkownika  
- `GetUserInfoAsync()` - Informacje o u¿ytkowniku
- `RefreshTokenAsync()` - Odœwie¿anie tokenu
- `RevokeTokenAsync()` - Uniewa¿nianie tokenu

**REST Endpoints:**
```http
POST /api/v1/auth/login          # Logowanie
POST /api/v1/auth/register       # Rejestracja
POST /api/v1/auth/validate-token # Walidacja tokenu
GET  /api/v1/auth/user/{id}      # Info o u¿ytkowniku
GET  /api/v1/auth/me             # Info o aktualnym u¿ytkowniku
POST /api/v1/auth/logout         # Wylogowanie
```

### ??? Products Module (IProductsContract)

**Odpowiedzialnoœæ:** Zarz¹dzanie produktami spo¿ywczymi

**Dostêpne operacje:**
- `GetProductAsync()` - Pobranie produktu
- `SearchProductsAsync()` - Wyszukiwanie produktów
- `GetProductsByCategoryAsync()` - Produkty wed³ug kategorii
- `ImportProductsAsync()` - Import produktów
- `GetCategoriesAsync()` - Lista kategorii
- `GetAllergensAsync()` - Lista alergenów
- `GetIngredientsAsync()` - Lista sk³adników

**REST Endpoints:**
```http
GET  /api/v1/products/{id}           # Pobranie produktu
GET  /api/v1/products/search         # Wyszukiwanie
GET  /api/v1/products/category/{cat} # Produkty w kategorii
GET  /api/v1/products/categories     # Lista kategorii
GET  /api/v1/products/allergens      # Lista alergenów
GET  /api/v1/products/ingredients    # Lista sk³adników
GET  /api/v1/products/{id}/nutrition # Informacje ¿ywieniowe
POST /api/v1/products/import         # Import produktów [Admin]
```

### ?? AI Module (IAIContract)

**Odpowiedzialnoœæ:** Us³ugi sztucznej inteligencji

**Dostêpne operacje:**
- `GenerateResponseAsync()` - Generowanie tekstu AI
- `GenerateJsonAsync()` - Generowanie JSON AI  
- `AnalyzeProductAsync()` - Analiza produktu
- `GetRecipeRecommendationsAsync()` - Rekomendacje przepisów
- `AnalyzeNutritionAsync()` - Analiza ¿ywieniowa
- `DetectAllergensAsync()` - Detekcja alergenów
- `CalculateHealthScoreAsync()` - Ocena zdrowotna

**REST Endpoints:**
```http
POST /api/v1/ai/generate-text              # Generowanie tekstu
POST /api/v1/ai/generate-json              # Generowanie JSON
POST /api/v1/ai/analyze-product/{id}       # Analiza produktu
POST /api/v1/ai/recipe-recommendations     # Przepisy
GET  /api/v1/ai/nutrition-analysis/{id}    # Analiza ¿ywieniowa
POST /api/v1/ai/detect-allergens           # Detekcja alergenów
GET  /api/v1/ai/health-score/{id}          # Ocena zdrowotna
```

## ?? Event Bus - Szyna Danych

### ?? Cele szyny danych:
- **Decoupling** - Rozdzielenie modu³ów
- **Asynchronicznoœæ** - Nieblokuj¹ca komunikacja
- **Auditing** - Œledzenie operacji
- **Reakcyjnoœæ** - Automatyczne reakcje na zdarzenia

### ?? Zdarzenia w systemie:

#### Auth Events:
- `UserRegisteredEvent` - Rejestracja u¿ytkownika
- `UserLoggedInEvent` - Logowanie u¿ytkownika  
- `TokenValidatedEvent` - Walidacja tokenu

#### Products Events:
- `ProductCreatedEvent` - Utworzenie produktu
- `ProductUpdatedEvent` - Aktualizacja produktu
- `ProductSearchedEvent` - Wyszukiwanie produktu
- `ProductImportedEvent` - Import produktów

#### AI Events:
- `AIAnalysisRequestedEvent` - ¯¹danie analizy AI
- `AIAnalysisCompletedEvent` - Zakoñczenie analizy AI
- `RecipeGeneratedEvent` - Generowanie przepisu

### ?? Przyk³ad u¿ycia Event Bus:

```csharp
// Publikowanie zdarzenia
await _eventBus.PublishAsync(new UserRegisteredEvent
{
    UserId = user.Id,
    Username = user.UserName,
    Email = user.Email
});

// Subskrypcja zdarzenia
public class UserRegistrationHandler : IEventHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent @event)
    {
        // Wysy³anie powiadomienia welcome
        // Tworzenie profilu u¿ytkownika
        // Logowanie metryki
    }
}
```

## ?? Implementacja gRPC

### ?? Serwisy gRPC (Komunikacja wewnêtrzna):

Ka¿dy modu³ udostêpnia swoje funkcje przez gRPC:

- **AuthGrpcService** - `/inzynierka.Grpc.Auth.AuthService`
- **ProductsGrpcService** - `/inzynierka.Grpc.Products.ProductService`  
- **AIGrpcService** - `/inzynierka.Grpc.AI.AIService`

### ??? Klienci gRPC:

Modu³y komunikuj¹ siê miêdzy sob¹ przez klientów gRPC:

```csharp
// Przyk³ad komunikacji miêdzy modu³ami
public class AIModule : IAIContract
{
    private readonly IProductsGrpcClient _productsClient;
    
    public async Task<ProductAnalysisResult> AnalyzeProductAsync(string productId)
    {
        // Pobranie produktu przez gRPC
        var product = await _productsClient.GetProductAsync(productId);
        
        // Analiza produktu
        var analysis = await AnalyzeProduct(product);
        
        return analysis;
    }
}
```

## ??? Bezpieczeñstwo i Autoryzacja

### ?? JWT Token Flow:
1. **Login** ? REST API ? Auth Module ? JWT Token
2. **Request** ? REST API ? Token Validation ? Auth Module
3. **Authorization** ? Claims-based access control

### ?? Poziomy dostêpu:
- **Public** - Endpoints bez autoryzacji
- **User** - Wymaga logowania
- **Admin** - Wymaga roli administratora

## ?? Korzyœci Nowej Architektury

### ? **Separacja odpowiedzialnoœci:**
- Ka¿dy modu³ ma jasno zdefiniowan¹ rolê
- Kontrakty definiuj¹ precyzyjne interfejsy
- £atwe testowanie jednostkowe

### ? **Skalowalnoœæ:**
- Modu³y mo¿na niezale¿nie skalowaæ
- gRPC zapewnia wydajn¹ komunikacjê
- Event Bus umo¿liwia asynchroniczne przetwarzanie

### ? **Maintainability:**
- Czyste granice miêdzy modu³ami  
- £atwe dodawanie nowych funkcji
- Mo¿liwoœæ refaktoryzacji bez wp³ywu na inne modu³y

### ? **Elastycznoœæ:**
- Mo¿liwoœæ ³atwej zamiany implementacji
- Wsparcie dla ró¿nych protoko³ów komunikacji
- Extensible event system

## ?? Dodawanie Nowych Funkcji

### 1. **Nowy endpoint REST:**
```csharp
[HttpGet("new-feature")]
public async Task<IActionResult> NewFeature()
{
    var result = await _contractModule.NewOperationAsync();
    return Ok(result);
}
```

### 2. **Nowa operacja w kontrakcie:**
```csharp
public interface IMyContract
{
    Task<MyResult> NewOperationAsync(MyRequest request);
}
```

### 3. **Nowe zdarzenie:**
```csharp
public class NewFeatureUsedEvent : IntegrationEvent
{
    public string UserId { get; set; }
    public string FeatureName { get; set; }
}
```

### 4. **Handler zdarzenia:**
```csharp
public class NewFeatureHandler : IEventHandler<NewFeatureUsedEvent>
{
    public async Task HandleAsync(NewFeatureUsedEvent @event)
    {
        // Logika reakcji na zdarzenie
    }
}
```

## ?? Monitoring i Debugging

### ?? **Logi:**
- Ka¿dy modu³ loguje swoje operacje
- Event Bus loguje publikowane zdarzenia
- gRPC calls s¹ œledzone

### ?? **Metryki:**
- Liczba ¿¹dañ na modu³
- Czas odpowiedzi
- B³êdy i wyj¹tki

### ?? **Debugging:**
- Swagger UI dla REST API
- gRPC reflection dla narzêdzi dev
- Event tracing

## ?? Przysz³y Rozwój

### ?? **Potencjalne rozszerzenia:**
1. **Microservices** - Podzia³ na oddzielne serwisy
2. **Message Brokers** - RabbitMQ, Azure Service Bus  
3. **CQRS** - Command Query Responsibility Segregation
4. **Distributed Caching** - Redis, Memcached
5. **API Gateway** - Centralne zarz¹dzanie API
6. **Health Checks** - Monitoring stanu aplikacji

Ta architektura zapewnia solidn¹ podstawê do skalowania i rozwijania aplikacji FoodMatch! ??