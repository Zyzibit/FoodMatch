# Integracja FitnessGoal z Generowaniem Przepisów

## Przegląd Przepływu Danych

Funkcjonalność FitnessGoal jest w pełni zintegrowana z systemem generowania przepisów i planów posiłków. Oto jak dane przepływają przez system:

```
User FoodPreferences (z FitnessGoal)
    ↓
FoodPreferencesDto (automatyczne obliczenia)
    ↓
DietaryPreferences (merge z requestem)
    ↓
AI Prompt (z kontekstem celu)
    ↓
Wygenerowany przepis (dopasowany do celu)
```

## Szczegółowy Przepływ

### 1. Pobieranie Preferencji Użytkownika

```csharp
// RecipeService.cs
var userPreferences = await _userService.GetUserFoodPreferencesAsync(userId);
```

**Co się dzieje:**
- Pobiera `FoodPreferences` użytkownika z bazy danych
- `ToDto()` automatycznie:
  - Oblicza BMR i TDEE
  - Dostosowuje kalorie według FitnessGoal
  - Oblicza makroskładniki (białko, węglowodany, tłuszcze)
  - Rozkłada wartości na poszczególne posiłki

### 2. Merge z Requestem

```csharp
// RecipeService.cs
var preferences = request.Preferences.MergeWithUserPreferences(userPreferences);
preferences.ApplyMealTypeGoals(request.MealType, userPreferences);
```

**Co się dzieje:**
- `MergeWithUserPreferences()` łączy:
  - Preferencje dietetyczne (vegan, gluten-free, etc.)
  - **FitnessGoal** (przenosi z user preferences do DietaryPreferences)
  - Dzienne cele kaloryczne i makroskładniki
  - Niechciane składniki i alergeny
  
- `ApplyMealTypeGoals()` ustawia:
  - `TargetMealCalories` - kalorie dla konkretnego posiłku (np. 30% z dnia dla śniadania)
  - `TargetMealProtein` - białko dla tego posiłku
  - `TargetMealCarbohydrates` - węglowodany
  - `TargetMealFat` - tłuszcze

### 3. Przygotowanie Danych dla AI

```csharp
// RecipeGeneratorService.cs
if (!string.IsNullOrEmpty(request.Preferences.FitnessGoal))
{
    data["fitnessGoal"] = request.Preferences.FitnessGoal;
}

if (request.Preferences.TargetMealCalories.HasValue)
{
    data["targetMealCalories"] = request.Preferences.TargetMealCalories.Value;
}
// ... podobnie dla protein, carbs, fat
```

**Co AI otrzymuje:**
- Kontekst celu: `"fitnessGoal": "WeightLoss"`
- Docelowe wartości dla posiłku:
  - `targetMealCalories`: np. 530 kcal (30% z 1767 kcal dziennie)
  - `targetMealProtein`: np. 44g (30% z 176g dziennie)
  - `targetMealCarbohydrates`: np. 59g
  - `targetMealFat`: np. 15g

### 4. Prompt dla AI

W `prompt_config.json`, sekcja "PREFERENCJE UŻYTKOWNIKA":
```json
{
  "field": "fitnessGoal",
  "template": "  -CEL FITNESS: {value} (WeightLoss=Schudnięcie, Maintenance=Utrzymanie wagi, WeightGain=Przybranie na masie)"
}
```

W sekcji "CELE DIETETYCZNE DLA TEGO POSIŁKU":
```json
{ "field": "targetMealCalories", "template": "  -Docelowe KALORIE tego posiłku: {value} kcal" },
{ "field": "targetMealProtein", "template": " -Docelowe BIAŁKO tego posiłku: {value}g" },
{ "field": "targetMealCarbohydrates", "template": " -Docelowe WĘGLOWODANY tego posiłku: {value}g" },
{ "field": "targetMealFat", "template": " -Docelowe TŁUSZCZE tego posiłku: {value}g)" }
```

**Co to oznacza:**
AI wie:
- Jaki jest cel użytkownika (schudnięcie/utrzymanie/masa)
- Dokładnie ile kalorii i makroskładników powinien zawierać przepis
- Może dostosować przepis pod kątem typu celu (np. więcej białka dla schudnięcia)

## Przykładowy Scenariusz

### Użytkownik chce schudnąć:

**1. Ustawienia użytkownika:**
```json
{
  "age": 30,
  "gender": "Male",
  "weight": 80.0,
  "height": 180.0,
  "activityLevel": "ModeratelyActive",
  "fitnessGoal": "WeightLoss"
}
```

**2. Automatyczne obliczenia:**
```
BMR = 1780 kcal
TDEE = 1780 × 1.55 = 2759 kcal
Cel kaloryczny = 2759 × 0.8 = 2207 kcal (deficyt -20%)
Białko = 80kg × 2.2g = 176g (wysokie dla zachowania mięśni)
Tłuszcze = 2207 × 0.25 / 9 = 61g
Węglowodany = (2207 - 704 - 549) / 4 = 239g
```

**3. Request na przepis na śniadanie:**
```json
{
  "mealType": "Breakfast",
  "productIds": [1, 2, 3],
  "preferences": {}  // może być puste, system użyje user preferences
}
```

**4. Wartości przekazane do AI:**
```json
{
  "fitnessGoal": "WeightLoss",
  "mealType": "Breakfast",
  "targetMealCalories": 662,    // 30% z 2207
  "targetMealProtein": 44,      // 25% z 176g (rozkład Breakfast)
  "targetMealCarbohydrates": 72, // 30% z 239g
  "targetMealFat": 18            // 30% z 61g
}
```

**5. Wygenerowany przepis:**
AI stworzy przepis śniadaniowy z:
- ~662 kcal
- ~44g białka (wysoka zawartość białka dla celu WeightLoss)
- ~72g węglowodanów
- ~18g tłuszczów
- Składnikami dopasowanymi do schudnięcia (więcej warzyw, chude białka, zdrowe tłuszcze)

## Kluczowe Punkty Integracji

### ✅ Co działa automatycznie:

1. **Obliczenia wartości odżywczych** - gdy użytkownik ustawi FitnessGoal
2. **Transfer FitnessGoal** - z User → FoodPreferencesDto → DietaryPreferences
3. **Rozkład na posiłki** - automatyczny podział dziennych wartości
4. **Kontekst dla AI** - FitnessGoal jest przekazywany w prompcie

### ✅ Korzyści dla użytkownika:

1. **Automatyzacja** - nie musi ręcznie liczyć kalorii i makro
2. **Spójność** - wszystkie przepisy dopasowane do celu
3. **Personalizacja** - AI bierze pod uwagę cel przy generowaniu
4. **Flexibilność** - może nadpisać wartości ręcznie jeśli chce

### ✅ Co AI może zrobić z tą informacją:

1. **Dobór składników:**
   - WeightLoss: więcej warzyw, chude białka, mniej tłuszczów
   - WeightGain: więcej kalorycznych składników, orzechy, awokado
   - Maintenance: zrównoważone proporcje

2. **Rozmiar porcji:**
   - Dostosowanie ilości składników do dokładnych wartości docelowych

3. **Styl przygotowania:**
   - WeightLoss: grillowanie, gotowanie na parze
   - WeightGain: smażenie, dodawanie sosów

4. **Tekstura prompta:**
   - AI widzi cel i może go uwzględnić w opisie przepisu

## Testowanie Integracji

### Test 1: Zmiana celu
```
1. Ustaw FitnessGoal = "WeightLoss"
2. Wygeneruj przepis na śniadanie
3. Sprawdź wartości: powinny być ~20% niższe niż TDEE
4. Zmień na "WeightGain"
5. Wygeneruj ten sam typ posiłku
6. Wartości powinny być ~15% wyższe niż TDEE
```

### Test 2: Różne posiłki
```
1. Ustaw FitnessGoal = "WeightLoss"
2. Wygeneruj Breakfast (30% kalorii)
3. Wygeneruj Lunch (40% kalorii)
4. Suma powinna odpowiadać 70% dziennego celu
```

### Test 3: Override wartości
```
1. Ustaw FitnessGoal = "Maintenance"
2. W request ustaw własne targetMealCalories
3. AI powinno użyć wartości z requestu, nie z automatycznych obliczeń
```

## Troubleshooting

### Problem: AI nie uwzględnia FitnessGoal
**Rozwiązanie:** Sprawdź czy:
- FitnessGoal jest ustawiony w User.FoodPreferences
- MergeWithUserPreferences() jest wywołane
- PreparePromptDataAsync() dodaje fitnessGoal do danych

### Problem: Wartości makro nie sumują się do kalorii
**Rozwiązanie:**
- Sprawdź CalculateMacros() - używa 4 kcal/g dla białka i węglowodanów, 9 kcal/g dla tłuszczy
- AI może lekko odbiegać od wartości - to normalne

### Problem: Wszystkie posiłki mają te same wartości
**Rozwiązanie:**
- Sprawdź czy ApplyMealTypeGoals() jest wywołane
- Sprawdź rozkład procentowy w MealNutritionDistribution (Breakfast, Lunch, Dinner, Snack)

## Podsumowanie

Integracja jest **kompletna i automatyczna**:
- ✅ FitnessGoal przenosi się przez cały system
- ✅ Wartości są automatycznie obliczane
- ✅ AI otrzymuje pełny kontekst
- ✅ Przepisy są dopasowane do celu użytkownika

Użytkownik musi tylko:
1. Ustawić swoje dane (wiek, waga, wzrost, aktywność)
2. Wybrać cel (WeightLoss/Maintenance/WeightGain)
3. System reszta robi automatycznie!

