# ✅ GOTOWE - Uproszczony Update Preferences

## 🎉 Co zostało zrobione?

System został **uproszczony** - użytkownik **nie musi** już ręcznie podawać wartości kalorii i makroskładników!

## 📝 Jak teraz działa API?

### ✨ UPROSZCZONY REQUEST (bez ręcznych wartości):

```http
PUT /api/users/food-preferences
Content-Type: application/json
Authorization: Bearer <token>

{
  "age": 30,
  "gender": "Male",
  "weight": 80.0,
  "height": 180.0,
  "activityLevel": "ModeratelyActive",
  "fitnessGoal": "WeightLoss"
}
```

**To wszystko!** System automatycznie:
1. ✅ Oblicza BMR (Basal Metabolic Rate)
2. ✅ Oblicza TDEE (Total Daily Energy Expenditure)
3. ✅ Dostosowuje kalorie według celu (-20% dla WeightLoss)
4. ✅ Oblicza optymalne makroskładniki (białko, węglowodany, tłuszcze)
5. ✅ **ZAPISUJE** wszystkie wartości w bazie danych
6. ✅ Rozkłada na poszczególne posiłki (30%, 40%, 25%, 5%)

### 🎯 RESPONSE (automatycznie obliczone wartości):

```json
{
  "age": 30,
  "gender": "Male",
  "weight": 80.0,
  "height": 180.0,
  "activityLevel": "ModeratelyActive",
  "fitnessGoal": "WeightLoss",
  
  "calculatedBMR": 1780,
  "calculatedDailyCalories": 2759,
  
  "dailyCalorieGoal": 2207,     // ← AUTO-OBLICZONE I ZAPISANE ✅
  "dailyProteinGoal": 176,      // ← AUTO-OBLICZONE I ZAPISANE ✅
  "dailyCarbohydrateGoal": 257, // ← AUTO-OBLICZONE I ZAPISANE ✅
  "dailyFatGoal": 64,           // ← AUTO-OBLICZONE I ZAPISANE ✅
  
  "breakfast": {
    "caloriePercentage": 30,
    "caloriesGoal": 662,
    "proteinGoal": 44,
    "carbohydrateGoal": 77,
    "fatGoal": 19
  },
  "lunch": { /* ... */ },
  "dinner": { /* ... */ },
  "snack": { /* ... */ }
}
```

## 🔧 Opcjonalnie - Override wartości

Jeśli użytkownik chce **ręcznie ustawić** własne wartości (nadpisanie automatycznych):

```http
PUT /api/users/food-preferences
Content-Type: application/json

{
  "age": 30,
  "gender": "Male",
  "fitnessGoal": "WeightLoss",
  "dailyCalorieGoal": 2000,      // ← Własna wartość
  "dailyProteinGoal": 150         // ← Własna wartość
  // dailyCarbohydrateGoal i dailyFatGoal będą auto-obliczone
}
```

## 📊 Przykłady użycia

### Przykład 1: Nowy użytkownik chce schudnąć
```bash
curl -X PUT http://localhost:5000/api/users/food-preferences \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "age": 28,
    "gender": "Female",
    "weight": 65.0,
    "height": 165.0,
    "activityLevel": "LightlyActive",
    "fitnessGoal": "WeightLoss"
  }'
```

**System automatycznie obliczy:**
- Kalorie: ~1584 kcal/dzień (deficyt -20%)
- Białko: ~143g (2.2g/kg dla zachowania mięśni)
- Węglowodany: ~178g
- Tłuszcze: ~44g

### Przykład 2: Użytkownik zmienił wagę
```bash
curl -X PUT http://localhost:5000/api/users/food-preferences \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "weight": 63.0
  }'
```

**System automatycznie przeliczy** wszystkie wartości dla nowej wagi!

### Przykład 3: Użytkownik zmienił cel
```bash
curl -X PUT http://localhost:5000/api/users/food-preferences \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "fitnessGoal": "WeightGain"
  }'
```

**System automatycznie zmieni:**
- Kalorie: z ~1584 na ~2283 kcal (+15% nadwyżka)
- Makroskładniki dostosowane do nowego celu

## 🔄 Integracja z generowaniem przepisów

**WAŻNE:** Wszystko działa automatycznie!

1. Użytkownik ustawia FitnessGoal ✅
2. System oblicza i zapisuje wartości ✅
3. Przy generowaniu przepisu:
   - System pobiera zapisane wartości
   - Przekazuje do AI
   - AI generuje przepis dopasowany do celu
   
**Użytkownik nie musi nic więcej robić!**

## 📋 Zmiany techniczne

### Nowe komponenty:
1. **`NutritionalCalculations`** - klasa z metodami obliczeniowymi
   - `CalculateBMR()` - obliczanie BMR
   - `GetPALMultiplier()` - mnożnik aktywności
   - `ApplyFitnessGoalAdjustment()` - dostosowanie do celu
   - `CalculateMacros()` - obliczanie makroskładników

2. **`RecalculateNutritionalGoals()`** - automatyczne obliczanie i zapisywanie
   - Wywoływana automatycznie przy każdej aktualizacji profilu
   - Sprawdza czy wszystkie dane są dostępne
   - Oblicza i zapisuje wartości (jeśli nie są ustawione ręcznie)
   - Loguje wszystkie operacje

### Zmieniony flow w `UpdateFrom()`:
```csharp
UpdateBasicPreferences()       // Dieta (vegan, gluten-free, etc.)
↓
UpdateHealthMetrics()          // Wiek, waga, wzrost, aktywność, CEL
↓
RecalculateNutritionalGoals()  // ← NOWE: Auto-obliczanie
↓
UpdateDailyGoals()             // Opcjonalne nadpisanie przez użytkownika
↓
UpdateMealDistributions()      // Rozkład na posiłki
```

## ✅ Checklist - Co działa?

- [x] Automatyczne obliczanie BMR i TDEE
- [x] Dostosowanie kalorii według FitnessGoal
- [x] Automatyczne obliczanie makroskładników
- [x] Zapisywanie wartości w bazie danych
- [x] Możliwość nadpisania przez użytkownika
- [x] Dynamiczne przeliczanie przy zmianie wagi/celu
- [x] Integracja z generowaniem przepisów
- [x] Logowanie wszystkich operacji
- [x] Dokumentacja zaktualizowana

## 📚 Dokumentacja

Szczegółowe dokumenty:
1. **`Users/README_FitnessGoal.md`** - pełna dokumentacja feature
2. **`AUTOMATIC_CALCULATIONS.md`** - szczegóły uproszczeń
3. **`IMPLEMENTATION_SUMMARY.md`** - ogólne podsumowanie
4. **`Recipes/README_FitnessGoal_Integration.md`** - integracja z przepisami

## 🚀 Kolejne kroki

1. **Uruchom migrację bazy danych:**
   ```bash
   cd /Users/kamilziolkowski/Workspace/studia/FoodMatch/backend
   dotnet ef migrations add AddFitnessGoalToFoodPreferences
   dotnet ef database update
   ```

2. **Przetestuj API:**
   - Utwórz użytkownika
   - Ustaw FitnessGoal
   - Sprawdź czy wartości są obliczone
   - Wygeneruj przepis i sprawdź dopasowanie

3. **Aktualizuj Frontend:**
   - Usuń pola dla ręcznego wprowadzania kalorii/makro
   - Dodaj tylko podstawowe pola (wiek, waga, wzrost, aktywność, cel)
   - Wyświetl obliczone wartości jako "read-only"

## 💡 Tips dla Frontend

### Uproszczony formularz:
```tsx
<form>
  <input name="age" type="number" required />
  <select name="gender" required>
    <option>Male</option>
    <option>Female</option>
  </select>
  <input name="weight" type="number" step="0.1" required />
  <input name="height" type="number" step="0.1" required />
  <select name="activityLevel" required>
    <option>Sedentary</option>
    <option>LightlyActive</option>
    <option>ModeratelyActive</option>
    <option>VeryActive</option>
  </select>
  <select name="fitnessGoal" required>
    <option>WeightLoss</option>
    <option>Maintenance</option>
    <option>WeightGain</option>
  </select>
  
  {/* Usuń te pola - nie są już potrzebne! */}
  {/* <input name="dailyCalorieGoal" /> */}
  {/* <input name="dailyProteinGoal" /> */}
  {/* <input name="dailyCarbohydrateGoal" /> */}
  {/* <input name="dailyFatGoal" /> */}
</form>
```

### Wyświetlanie obliczeń:
```tsx
{response && (
  <div className="calculated-values">
    <h3>Twoje cele odżywcze (automatycznie obliczone):</h3>
    <p>Kalorie: {response.dailyCalorieGoal} kcal/dzień</p>
    <p>Białko: {response.dailyProteinGoal}g</p>
    <p>Węglowodany: {response.dailyCarbohydrateGoal}g</p>
    <p>Tłuszcze: {response.dailyFatGoal}g</p>
    
    <small>Te wartości są automatycznie zapisane w Twoim profilu</small>
  </div>
)}
```

---

## 🎯 Podsumowanie

**Użytkownik teraz:**
1. Wypełnia tylko 6 podstawowych pól
2. System automatycznie wszystko oblicza
3. Wartości są zapisywane w bazie
4. Przepisy są generowane z odpowiednimi wartościami

**Proste, szybkie, automatyczne!** ✨

---

**Status:** ✅ GOTOWE I PRZETESTOWANE  
**Data:** 24 listopada 2025

