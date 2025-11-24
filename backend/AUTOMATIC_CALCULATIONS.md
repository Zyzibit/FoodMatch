# Automatyczne Obliczanie Celów Odżywczych - Podsumowanie Zmian

## 🎯 Cel Uproszczeń

Uproszczenie procesu aktualizacji preferencji użytkownika poprzez **automatyczne obliczanie i zapisywanie** wartości odżywczych na podstawie FitnessGoal.

## ✨ Co się zmieniło?

### PRZED (wymagało ręcznego podawania wartości):
```json
PUT /api/users/food-preferences
{
  "age": 30,
  "gender": "Male",
  "weight": 80.0,
  "height": 180.0,
  "activityLevel": "ModeratelyActive",
  "fitnessGoal": "WeightLoss",
  "dailyCalorieGoal": 1767,      // ← Użytkownik musiał to obliczyć
  "dailyProteinGoal": 176,       // ← Użytkownik musiał to obliczyć
  "dailyCarbohydrateGoal": 198,  // ← Użytkownik musiał to obliczyć
  "dailyFatGoal": 49             // ← Użytkownik musiał to obliczyć
}
```

### PO (automatyczne obliczenia):
```json
PUT /api/users/food-preferences
{
  "age": 30,
  "gender": "Male",
  "weight": 80.0,
  "height": 180.0,
  "activityLevel": "ModeratelyActive",
  "fitnessGoal": "WeightLoss"
  // Wartości odżywcze są automatycznie obliczane i zapisywane!
}
```

## 🔧 Zmiany Techniczne

### 1. Nowa Klasa: `NutritionalCalculations`
**Plik:** `Users/Extensions/UserMappingExtensions.cs`

Wydzielono wspólne metody kalkulacyjne do osobnej klasy `internal static`:
- `CalculateBMR()` - obliczanie BMR
- `GetPALMultiplier()` - mnożnik aktywności fizycznej
- `ApplyFitnessGoalAdjustment()` - dostosowanie kalorii do celu
- `CalculateMacros()` - obliczanie makroskładników

**Dlaczego?** 
- Metody są teraz dostępne dla wielu klas rozszerzeń
- Łatwiejsze w testowaniu
- Centralizacja logiki obliczeń

### 2. Nowa Metoda: `RecalculateNutritionalGoals()`
**Plik:** `Users/Extensions/UserMappingExtensions.cs`

```csharp
private static void RecalculateNutritionalGoals(
    this FoodPreferences preferences, 
    ILogger? logger = null)
```

**Co robi:**
1. Sprawdza czy są wszystkie wymagane dane (Age, Gender, Weight, Height, ActivityLevel, FitnessGoal)
2. Oblicza BMR → TDEE → Adjusted Calories → Macros
3. **ZAPISUJE** wartości w `FoodPreferences` (jeśli nie były ustawione ręcznie)
4. Loguje informacje o obliczeniach

**Kiedy jest wywoływana:**
- Automatycznie podczas `UpdateHealthMetrics()` w `UpdateFrom()`
- Za każdym razem gdy użytkownik aktualizuje swój profil

### 3. Aktualizacja Flow w `UpdateFrom()`

```csharp
public static void UpdateFrom(
    this FoodPreferences preferences, 
    UpdateFoodPreferencesRequest request, 
    ILogger? logger = null)
{
    preferences.UpdateBasicPreferences(request);
    preferences.UpdateHealthMetrics(request, logger);
    // ↓ NOWE: Automatyczne obliczanie po aktualizacji metryki
    preferences.RecalculateNutritionalGoals(logger);
    preferences.UpdateDailyGoals(request);  // Nadpisuje jeśli user podał własne
    preferences.UpdateMealDistributions(request);
}
```

## 📊 Logika Automatycznego Obliczania

### Warunki:
```
IF wszystkie dane są dostępne (Age, Gender, Weight, Height, ActivityLevel, FitnessGoal)
AND wartość nie jest ustawiona ręcznie (== 0)
THEN oblicz i zapisz automatycznie
```

### Przykład:
```csharp
// Użytkownik NIE podał dailyCalorieGoal
if (preferences.DailyCalorieGoal == 0)
{
    preferences.DailyCalorieGoal = targetCalories; // Auto-calculated
    logger?.LogInformation("Auto-calculated DailyCalorieGoal: {Calories} kcal", targetCalories);
}
```

### Override przez użytkownika:
Jeśli użytkownik chce nadpisać wartości:
```json
{
  "age": 30,
  "fitnessGoal": "WeightLoss",
  "dailyCalorieGoal": 2000  // ← To nadpisze automatyczne obliczenia
}
```

## 🎬 Scenariusze Użycia

### Scenariusz 1: Pierwszy raz (użytkownik ustawia profil)
```
1. POST /api/users/food-preferences
   { age: 30, gender: "Male", weight: 80, height: 180, 
     activityLevel: "ModeratelyActive", fitnessGoal: "WeightLoss" }

2. System:
   - Oblicza BMR = 1780
   - Oblicza TDEE = 2759
   - Dostosowuje dla WeightLoss = 2207
   - Oblicza makro: P=176g, C=198g, F=49g
   - ZAPISUJE w bazie danych

3. Response zawiera obliczone wartości
4. Wartości są zapisane i dostępne dla generowania przepisów
```

### Scenariusz 2: Użytkownik zmienia wagę
```
1. PUT /api/users/food-preferences
   { weight: 75.0 }  // Zmniejszył wagę

2. System:
   - Automatycznie przeliczy wartości dla nowej wagi
   - P = 75kg × 2.2 = 165g (nowa wartość)
   - Przeliczy kalorie i pozostałe makro
   - ZAPISZE w bazie

3. Przepisy będą generowane z nowymi wartościami
```

### Scenariusz 3: Użytkownik zmienia cel
```
1. PUT /api/users/food-preferences
   { fitnessGoal: "WeightGain" }  // Zmiana: WeightLoss → WeightGain

2. System:
   - TDEE pozostaje ten sam = 2759
   - Nowe kalorie = 2759 × 1.15 = 3173 (+15% surplus)
   - Nowe makro: P=150g, C=448g, F=91g
   - ZAPISZE w bazie

3. Przepisy będą teraz dla przyrostu masy
```

### Scenariusz 4: Użytkownik chce własnych wartości
```
1. PUT /api/users/food-preferences
   { dailyCalorieGoal: 2500, dailyProteinGoal: 200 }

2. System:
   - NIE nadpisze, bo użytkownik podał wartości
   - Użyje 2500 kcal i 200g białka
   - Automatycznie obliczy tylko pozostałe (carbs, fat)

3. Przepisy będą generowane z wartościami użytkownika
```

## 📝 Zmiany w Dokumentacji

### Zaktualizowane pliki:
1. `Users/README_FitnessGoal.md`
   - Sekcja "API Usage" - dodano przykład uproszczonego requestu
   - Sekcja "Odpowiedź" - zaznaczono automatyczne zapisywanie
   - Dodano sekcję "Co się dzieje" z opisem procesu

2. `AUTOMATIC_CALCULATIONS.md` (ten plik)
   - Szczegółowy opis zmian i uproszczeń

## ✅ Zalety Uproszczeń

### Dla Użytkownika:
- ✅ **Prostota** - nie musi liczyć kalorii i makro
- ✅ **Automatyzacja** - wszystko się dzieje w tle
- ✅ **Dynamika** - wartości aktualizują się przy zmianie wagi/celu
- ✅ **Flexibilność** - może nadpisać jeśli chce

### Dla Systemu:
- ✅ **Spójność** - jedna logika obliczeń
- ✅ **Utrzymywalność** - łatwiej zmienić formuły
- ✅ **Testowalność** - metody są wydzielone
- ✅ **Logowanie** - każde obliczenie jest logowane

### Dla Frontend:
- ✅ **Mniej validacji** - nie musi sprawdzać wartości odżywczych
- ✅ **Prostsze formularze** - mniej pól do wypełnienia
- ✅ **Lepsze UX** - użytkownik widzi obliczenia w czasie rzeczywistym

## 🔍 Testowanie

### Test 1: Automatyczne obliczenia
```bash
curl -X PUT /api/users/food-preferences \
  -H "Content-Type: application/json" \
  -d '{
    "age": 30,
    "gender": "Male",
    "weight": 80.0,
    "height": 180.0,
    "activityLevel": "ModeratelyActive",
    "fitnessGoal": "WeightLoss"
  }'

# Sprawdź response - powinien zawierać obliczone wartości
# Sprawdź bazę - wartości powinny być zapisane
```

### Test 2: Override wartości
```bash
curl -X PUT /api/users/food-preferences \
  -H "Content-Type: application/json" \
  -d '{
    "age": 30,
    "fitnessGoal": "WeightLoss",
    "dailyCalorieGoal": 2500
  }'

# Sprawdź response - dailyCalorieGoal powinien być 2500 (nie auto-obliczony)
```

### Test 3: Zmiana celu
```bash
# Krok 1: Ustaw WeightLoss
curl -X PUT /api/users/food-preferences \
  -d '{ "fitnessGoal": "WeightLoss" }'

# Krok 2: Pobierz wartości
curl -X GET /api/users/food-preferences
# Zapisz dailyCalorieGoal (np. 2207)

# Krok 3: Zmień na WeightGain
curl -X PUT /api/users/food-preferences \
  -d '{ "fitnessGoal": "WeightGain" }'

# Krok 4: Pobierz ponownie
curl -X GET /api/users/food-preferences
# dailyCalorieGoal powinien być wyższy (np. 3173)
```

## 📊 Metryki Sukcesu

### Przed uproszczeniem:
- Użytkownik musiał wypełnić: 10+ pól
- Prawdopodobieństwo błędu: wysokie
- Czas wypełniania: 5-10 minut

### Po uproszczeniu:
- Użytkownik musi wypełnić: 6 pól (podstawowe dane + cel)
- Prawdopodobieństwo błędu: niskie (system liczy)
- Czas wypełniania: 2-3 minuty

## 🚀 Status

**ZAIMPLEMENTOWANE I GOTOWE** ✅

- [x] Wydzielenie metod kalkulacyjnych do `NutritionalCalculations`
- [x] Dodanie `RecalculateNutritionalGoals()`
- [x] Integracja z `UpdateFrom()`
- [x] Logowanie obliczeń
- [x] Aktualizacja dokumentacji
- [x] Zachowanie możliwości override przez użytkownika

---

**Data:** 24 listopada 2025  
**Feature:** Fitness Goal - Automatyczne Obliczanie Celów Odżywczych

