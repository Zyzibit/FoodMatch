# Fitness Goal Feature - Dokumentacja

## Przegląd
Dodano funkcjonalność umożliwiającą użytkownikom wybór celu fitness (schudnięcie, utrzymanie wagi, przybranie na masie). System automatycznie oblicza dzienne zapotrzebowanie kaloryczne oraz makroskładniki (białko, węglowodany, tłuszcze) na podstawie wybranego celu.

## Cele Fitness (FitnessGoal)

### Dostępne cele:
- **WeightLoss** (Schudnięcie) - Deficyt kaloryczny -20%
- **Maintenance** (Utrzymanie wagi) - Kalorie na poziomie TDEE
- **WeightGain** (Przybranie na masie) - Nadwyżka kaloryczna +15%

## Zmiany w Modelu

### 1. Enum FitnessGoal
Lokalizacja: `Users/Model/User.cs`

```csharp
public enum FitnessGoal
{
    WeightLoss,      // Schudnięcie
    Maintenance,     // Utrzymanie wagi
    WeightGain       // Przybranie na masie
}
```

### 2. FoodPreferences
Dodano właściwość:
```csharp
public FitnessGoal? FitnessGoal { get; set; }
```

### 3. FoodPreferencesDto
Lokalizacja: `Users/Responses/FoodPreferencesDto.cs`

Dodano:
```csharp
public string? FitnessGoal { get; set; }
```

### 4. UpdateFoodPreferencesRequest
Lokalizacja: `Users/Requests/UpdateFoodPreferencesRequest.cs`

Dodano:
```csharp
public string? FitnessGoal { get; set; } // "WeightLoss", "Maintenance", "WeightGain"
```

### 5. DietaryPreferences (Recipes)
Lokalizacja: `Recipes/Model/RecipeModel/DietaryPreferences.cs`

Dodano:
```csharp
public string? FitnessGoal { get; set; }
```

## Logika Kalkulacji

### 1. Obliczanie BMR (Basal Metabolic Rate)
Formuła Mifflin-St Jeor:
- **Mężczyźni**: BMR = (10 × waga_kg) + (6.25 × wzrost_cm) - (5 × wiek) + 5
- **Kobiety**: BMR = (10 × waga_kg) + (6.25 × wzrost_cm) - (5 × wiek) - 161

### 2. Obliczanie TDEE (Total Daily Energy Expenditure)
TDEE = BMR × PAL (Physical Activity Level)

**Mnożniki PAL:**
- Sedentary (Siedzący tryb życia): 1.2
- LightlyActive (Lekko aktywny): 1.375
- ModeratelyActive (Umiarkowanie aktywny): 1.55
- VeryActive (Bardzo aktywny): 1.725
- ExtraActive (Ekstremalnie aktywny): 1.9

### 3. Dostosowanie do Celu (Fitness Goal Adjustment)
- **WeightLoss**: TDEE × 0.8 (-20% deficyt)
- **Maintenance**: TDEE × 1.0 (bez zmian)
- **WeightGain**: TDEE × 1.15 (+15% nadwyżka)

### 4. Obliczanie Makroskładników

#### Białko (Protein)
Na podstawie wagi ciała:
- **WeightLoss**: 2.2g/kg (wyższe dla zachowania masy mięśniowej)
- **Maintenance**: 1.8g/kg
- **WeightGain**: 2.0g/kg (wyższe dla budowy mięśni)

#### Tłuszcze (Fat)
Procent z całkowitych kalorii:
- **WeightLoss**: 25% kalorii
- **Maintenance**: 28% kalorii
- **WeightGain**: 25% kalorii

#### Węglowodany (Carbohydrates)
Reszta kalorii po odjęciu białka i tłuszczów:
- Carbs = (Total Calories - Protein Calories - Fat Calories) / 4

**Wartości kaloryczne:**
- Białko: 4 kcal/g
- Węglowodany: 4 kcal/g
- Tłuszcze: 9 kcal/g

## Automatyczne Obliczenia

### Kiedy następuje automatyczne przeliczenie?
System automatycznie oblicza wartości, gdy:
1. Użytkownik ustawi `FitnessGoal`
2. Dostępne są dane: `Age`, `Gender`, `Weight`, `Height`, `ActivityLevel`
3. Użytkownik nie ustawił własnych wartości dla makroskładników

### Priorytet wartości:
1. **Wartości użytkownika** - jeśli użytkownik ustawił własne `DailyCalorieGoal`, `DailyProteinGoal`, itd., są one używane
2. **Automatyczne obliczenia** - jeśli wartości użytkownika = 0, system używa automatycznych obliczeń

## Rozkład Posiłków

Wartości dzienne są rozdzielane między posiłki według procentów:

### Domyślny rozkład:
- **Breakfast (Śniadanie)**: 30% kalorii
- **Lunch (Obiad)**: 40% kalorii
- **Dinner (Kolacja)**: 25% kalorii
- **Snack (Przekąska)**: 5% kalorii

Każdy posiłek ma własne procenty dla białka, węglowodanów i tłuszczów, które można dostosować.

## API Usage

### Aktualizacja preferencji użytkownika (uproszczona - automatyczne obliczenia):
```http
PUT /api/users/food-preferences
Content-Type: application/json

{
  "age": 30,
  "gender": "Male",
  "weight": 80.0,
  "height": 180.0,
  "activityLevel": "ModeratelyActive",
  "fitnessGoal": "WeightLoss"
}
```

**Uwaga:** System automatycznie obliczy i zapisze wartości `DailyCalorieGoal`, `DailyProteinGoal`, `DailyCarbohydrateGoal` i `DailyFatGoal` w modelu użytkownika. Użytkownik **nie musi** podawać tych wartości ręcznie!

### Opcjonalnie - ręczne ustawienie wartości (override):
Jeśli użytkownik chce nadpisać automatyczne wartości:
```http
PUT /api/users/food-preferences
Content-Type: application/json

{
  "age": 30,
  "gender": "Male",
  "weight": 80.0,
  "height": 180.0,
  "activityLevel": "ModeratelyActive",
  "fitnessGoal": "WeightLoss",
  "dailyCalorieGoal": 2000,
  "dailyProteinGoal": 150,
  "dailyCarbohydrateGoal": 200,
  "dailyFatGoal": 55
}
```

### Odpowiedź zawiera automatycznie obliczone i ZAPISANE wartości:
```json
{
  "age": 30,
  "gender": "Male",
  "weight": 80.0,
  "height": 180.0,
  "activityLevel": "ModeratelyActive",
  "fitnessGoal": "WeightLoss",
  "calculatedBMR": 1780,
  "calculatedDailyCalories": 2209,
  "dailyCalorieGoal": 1767,     // ← AUTOMATYCZNIE OBLICZONE I ZAPISANE w bazie
  "dailyProteinGoal": 176,      // ← AUTOMATYCZNIE OBLICZONE I ZAPISANE w bazie
  "dailyCarbohydrateGoal": 198, // ← AUTOMATYCZNIE OBLICZONE I ZAPISANE w bazie
  "dailyFatGoal": 49,           // ← AUTOMATYCZNIE OBLICZONE I ZAPISANE w bazie
  "breakfast": {
    "caloriePercentage": 30,
    "caloriesGoal": 530,
    "proteinGoal": 44,
    "carbohydrateGoal": 59,
    "fatGoal": 15
  }
}
```

**Co się dzieje:**
1. System oblicza BMR i TDEE na podstawie danych użytkownika
2. Dostosowuje kalorie według FitnessGoal (-20% dla WeightLoss)
3. Oblicza optymalne makroskładniki
4. **ZAPISUJE** te wartości w `FoodPreferences` w bazie danych
5. Wartości są dostępne przy każdym kolejnym wywołaniu API i generowaniu przepisów
6. Użytkownik może nadpisać wartości w dowolnym momencie

## Konfiguracja Bazy Danych

Właściwość `FitnessGoal` jest zapisywana jako string w bazie danych:
```csharp
fp.Property(p => p.FitnessGoal)
    .IsRequired(false)
    .HasConversion<string>();
```

## Migracja

Aby zastosować zmiany w bazie danych, wykonaj:
```bash
dotnet ef migrations add AddFitnessGoalToFoodPreferences
dotnet ef database update
```

## Przykładowe Scenariusze

### Scenariusz 1: Użytkownik chce schudnąć
- Waga: 90kg, Wzrost: 175cm, Wiek: 28, Płeć: Male
- ActivityLevel: LightlyActive
- FitnessGoal: WeightLoss
- **Wynik**: ~1960 kcal/dzień, 198g białka, 220g węglowodanów, 54g tłuszczu

### Scenariusz 2: Użytkownik chce utrzymać wagę
- Waga: 70kg, Wzrost: 165cm, Wiek: 25, Płeć: Female
- ActivityLevel: ModeratelyActive
- FitnessGoal: Maintenance
- **Wynik**: ~2108 kcal/dzień, 126g białka, 278g węglowodanów, 66g tłuszczu

### Scenariusz 3: Użytkownik chce przybrać na masie
- Waga: 75kg, Wzrost: 180cm, Wiek: 22, Płeć: Male
- ActivityLevel: VeryActive
- FitnessGoal: WeightGain
- **Wynik**: ~3260 kcal/dzień, 150g białka, 448g węglowodanów, 91g tłuszczu

## Integracja z Generowaniem Przepisów

### Jak FitnessGoal wpływa na przepisy?

1. **Automatyczne przekazywanie wartości**:
   - `FitnessGoal` jest automatycznie kopiowany z `FoodPreferencesDto` do `DietaryPreferences`
   - Przy każdym generowaniu przepisu, system merguje preferencje użytkownika z requestem

2. **Przekazywanie do AI (RecipeGeneratorService)**:
   - FitnessGoal jest dodawany do danych promptu jako `fitnessGoal`
   - AI otrzymuje informację: "CEL FITNESS: WeightLoss (WeightLoss=Schudnięcie, Maintenance=Utrzymanie wagi, WeightGain=Przybranie na masie)"
   - Wartości kalorii i makroskładników dla posiłku są obliczone automatycznie na podstawie FitnessGoal

3. **Automatyczne cele dla posiłku**:
   - Funkcja `ApplyMealTypeGoals` używa wartości obliczonych na podstawie FitnessGoal
   - Dla każdego typu posiłku (Breakfast, Lunch, Dinner, Snack) system oblicza:
     - `TargetMealCalories`
     - `TargetMealProtein`
     - `TargetMealCarbohydrates`
     - `TargetMealFat`

### Przykładowy Flow Generowania Przepisu:

```
1. Użytkownik ustawia FitnessGoal = "WeightLoss"
   └─> System oblicza: DailyCalories = 1767 kcal

2. Użytkownik generuje przepis na śniadanie (Breakfast)
   └─> System oblicza cele dla śniadania (30% dziennych wartości):
       - TargetMealCalories = 530 kcal
       - TargetMealProtein = 44g
       - TargetMealCarbohydrates = 59g
       - TargetMealFat = 15g

3. RecipeGeneratorService tworzy prompt z:
   - fitnessGoal: "WeightLoss"
   - targetMealCalories: 530
   - targetMealProtein: 44
   - targetMealCarbohydrates: 59
   - targetMealFat: 15

4. AI generuje przepis zgodny z tymi wartościami
```

## Uwagi Implementacyjne

1. **Walidacja**: System nie wymusza ustawienia FitnessGoal - jest opcjonalny
2. **Nadpisywanie**: Użytkownik może zawsze ręcznie nadpisać automatyczne wartości
3. **Flexibilność**: Makroskładniki są sugestią, nie sztywnym wymogiem
4. **Integracja z przepisami**: FitnessGoal jest przekazywany do DietaryPreferences dla lepszego dopasowania przepisów
5. **AI Context**: AI ma pełną świadomość celu użytkownika i dobiera składniki odpowiednio

## Pełny Przykład Użycia (End-to-End)

### Krok 1: Użytkownik ustawia swój profil i cel
```http
PUT /api/users/food-preferences
Content-Type: application/json
Authorization: Bearer <token>

{
  "age": 28,
  "gender": "Male",
  "weight": 85.0,
  "height": 178.0,
  "activityLevel": "ModeratelyActive",
  "fitnessGoal": "WeightLoss",
  "isVegetarian": false,
  "allergies": ["orzechy"]
}
```

**Odpowiedź systemu:**
```json
{
  "age": 28,
  "gender": "Male",
  "weight": 85.0,
  "height": 178.0,
  "activityLevel": "ModeratelyActive",
  "fitnessGoal": "WeightLoss",
  "calculatedBMR": 1848,
  "calculatedDailyCalories": 2864,
  "dailyCalorieGoal": 2291,  // TDEE × 0.8
  "dailyProteinGoal": 187,    // 2.2g/kg × 85kg
  "dailyCarbohydrateGoal": 257,
  "dailyFatGoal": 64,
  "breakfast": {
    "caloriePercentage": 30,
    "caloriesGoal": 687,       // 30% z 2291
    "proteinGoal": 47,         // 30% z 187
    "carbohydrateGoal": 77,
    "fatGoal": 19
  },
  "lunch": { /* ... */ },
  "dinner": { /* ... */ },
  "snack": { /* ... */ }
}
```

### Krok 2: Użytkownik generuje przepis na śniadanie
```http
POST /api/recipes/generate-preview
Content-Type: application/json
Authorization: Bearer <token>

{
  "mealType": "Breakfast",
  "availableIngredients": ["jajka", "pomidory", "pieczywo pełnoziarniste"],
  "cuisineType": "Polska",
  "maxPreparationTimeMinutes": 20
}
```

**Co się dzieje w systemie:**

1. System pobiera `FoodPreferencesDto` użytkownika
2. Merguje preferencje:
   ```csharp
   var preferences = request.Preferences.MergeWithUserPreferences(userPreferences);
   // FitnessGoal: "WeightLoss"
   // DailyCalorieGoal: 2291
   // DailyProteinGoal: 187
   // ...
   ```

3. Aplikuje cele dla śniadania:
   ```csharp
   preferences.ApplyMealTypeGoals("Breakfast", userPreferences);
   // TargetMealCalories: 687
   // TargetMealProtein: 47
   // TargetMealCarbohydrates: 77
   // TargetMealFat: 19
   ```

4. Wysyła do AI prompt z:
   - CEL FITNESS: WeightLoss
   - Docelowe KALORIE tego posiłku: 687 kcal
   - Docelowe BIAŁKO tego posiłku: 47g
   - Docelowe WĘGLOWODANY tego posiłku: 77g
   - Docelowe TŁUSZCZE tego posiłku: 19g
   - ALERGENY - BEZWZGLĘDNIE NIE UŻYWAJ: orzechy

5. AI generuje przepis spełniający te wymagania

**Odpowiedź:**
```json
{
  "success": true,
  "recipe": {
    "title": "Jajecznica z pomidorami na pieczywie pełnoziarnistym",
    "estimatedCalories": 685,
    "estimatedProtein": 48,
    "estimatedCarbohydrates": 76,
    "estimatedFats": 18,
    "ingredients": [ /* ... */ ],
    "instructions": "..."
  }
}
```

### Krok 3: Użytkownik dodaje przepis do planu posiłków
```http
POST /api/meal-plans
Content-Type: application/json
Authorization: Bearer <token>

{
  "date": "2025-11-25",
  "mealName": "Breakfast",
  "recipeId": 123
}
```

## Sprawdzenie Poprawności Implementacji

### Checklist dla deweloperów:

✅ **Model (Users/Model/User.cs)**
- [x] Dodano enum `FitnessGoal`
- [x] Dodano właściwość `FitnessGoal?` do `FoodPreferences`

✅ **DTO (Users/Responses/FoodPreferencesDto.cs)**
- [x] Dodano `string? FitnessGoal`

✅ **Request (Users/Requests/UpdateFoodPreferencesRequest.cs)**
- [x] Dodano `string? FitnessGoal`

✅ **Extensions (Users/Extensions/UserMappingExtensions.cs)**
- [x] Dodano `ApplyFitnessGoalAdjustment()` - dostosowanie kalorii
- [x] Dodano `CalculateMacros()` - obliczanie makroskładników
- [x] Mapowanie FitnessGoal w `ToDto()`
- [x] Parsowanie FitnessGoal w `UpdateHealthMetrics()`

✅ **Database Configuration (Data/Configuration/FoodPreferencesConfiguration.cs)**
- [x] Dodano konfigurację dla `FitnessGoal` (nullable string)

✅ **Recipe Integration (Recipes/Model/RecipeModel/DietaryPreferences.cs)**
- [x] Dodano `string? FitnessGoal`

✅ **Recipe Extensions (Recipes/Extensions/DietaryPreferencesExtensions.cs)**
- [x] Dodano mapowanie FitnessGoal w `MergeWithUserPreferences()`

✅ **Recipe Extensions (Users/Extensions/FoodPreferencesExtensions.cs)**
- [x] Dodano mapowanie FitnessGoal w `ToDietaryPreferences()`

✅ **Recipe Generator (Recipes/Services/RecipeGeneratorService.cs)**
- [x] Dodano FitnessGoal do danych promptu

✅ **Prompt Config (Recipes/Config/prompt_config.json)**
- [x] Dodano field `fitnessGoal` z tłumaczeniem dla AI

### Do wykonania przez użytkownika:
- [ ] Uruchomić migrację bazy danych
- [ ] Przetestować flow end-to-end
- [ ] Zweryfikować obliczenia na przykładowych danych

## Rozszerzenia w Przyszłości

Możliwe ulepszenia:
- Dodanie różnych strategii kalkulacji makro (np. keto, high-carb)
- Progresywne dostosowywanie kalorii na podstawie postępów
- Integracja z trackingiem wagi użytkownika
- Rekomendacje zmian celu na podstawie postępów
- Inteligentne sugestie zmiany FitnessGoal po osiągnięciu plateau
- Różne profile makroskładników w zależności od preferencji (np. wysokobiałkowy, ketogeniczny)

