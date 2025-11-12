# Meal Plan API – stan obecny backendu

Po przejrzeniu backendu (`backend/`), poniżej zebrałem faktycznie dostępne endpointy, które można wykorzystać do budowy widoku planu dnia (screen z 1800 kcal), oraz luki, które trzeba będzie dopisać.

---

## 1. Dane użytkownika i cele makro

| Endpoint | Opis | Dane zwracane |
| --- | --- | --- |
| `GET /api/v1/users/me` | Profil użytkownika (Id, Name, Email…). | `UserDto` – brak pól żywieniowych. |
| `GET /api/v1/users/preferences` | Preferencje/cele żywieniowe bieżącego użytkownika. | `FoodPreferencesDto` = `{ dailyCalorieGoal?, dailyProteinGoal?, dailyCarbohydrateGoal?, dailyFatGoal?, ... }`. |
| `POST /api/v1/users/preferences` | Aktualizacja preferencji (np. zmiana celu kalorii). | Brak payloadu w odpowiedzi (`{ message }`). |

**Wniosek:** wartości z panelu „Zapotrzebowanie kaloryczne: 2000” + paski progresu trzeba póki co brać z `GET /api/v1/users/preferences`. Nie istnieje dedykowany endpoint na „oblicz zapotrzebowanie” – musimy zapisać wynik w preferencjach.

---

## 2. Przepisy/posiłki użytkownika (Receipts)

Controller: `backend/Receipts/API/ReceiptsController.cs`, ścieżka bazowa `api/v1/receipts`.

| Endpoint | Cel |
| --- | --- |
| `GET /api/v1/receipts/me?limit=&offset=` | Lista przepisów zalogowanego użytkownika. Odpowiedź: `{ receipts: ReceiptDto[], totalCount, limit, offset, hasMore }`. `ReceiptDto` zawiera m.in. `Title`, `Description`, `Ingredients`, `Calories`, makra na 100 g, `CreatedAt`. |
| `GET /api/v1/receipts/{id}` | Szczegóły jednego przepisu – można użyć przy „Rozwiń”. |
| `POST /api/v1/receipts` | Ręczne dodanie przepisu (`CreateReceiptRequest`). |
| `POST /api/v1/receipts/generate-with-ai` | Generowanie przepisu przez AI na bazie listy `ProductIds` oraz preferencji (`GenerateRecipeWithAIRequest`). Zwraca `{ success, receiptId }`. |

**Istotne ograniczenia względem makiety:**
1. `ReceiptDto` nie ma informacji o dacie przypisania do planu ani o typie posiłku (Śniadanie/Obiad) czy godzinie. Jest tylko `CreatedAt`.
2. Makra podawane są „per 100 g”, nie ma pola „B: 10g, W: 60g…” dla całego posiłku; musimy je wyliczać z porcji.
3. Brak endpointów `PUT`/`DELETE` dla przepisów (nie da się jeszcze edytować/usuwać przez API).

---

## 3. AI / rekomendacje

W module `backend/AI/API/AIController.cs` mamy kilka uniwersalnych endpointów (prefiks `api/v1/ai`), m.in.:

- `POST /generate-text` – generowanie dowolnej odpowiedzi.
- `POST /generate-json` – generowanie JSON wg schematu.
- `POST /recipe-recommendations` – rekomendacje przepisów na podstawie składników i filtrów dietetycznych.
- `POST /analyze-product/{productId}`, `GET /nutrition-analysis/{productId}` – analizy produktów.

Na potrzeby guzika „Generuj plan (AI)” realnie wykorzystujemy już istniejący `POST /api/v1/receipts/generate-with-ai` (bo osadza wynik jako `Receipt` i zwraca `receiptId`). W razie potrzeby można też sięgnąć po `/api/v1/ai/recipe-recommendations` i samodzielnie zapisać wynik.

---

## 4. Produkty, jednostki, składniki

Do formularzy edycji posiłku przydadzą się:

- `GET /api/v1/products/search` + inne końcówki `ProductController` (kategorie, alergeny, pojedynczy produkt, dane żywieniowe).
- `GET /api/v1/units` / `GET /api/v1/units/{id}` – słownik jednostek.

Te endpointy są kompletne i już teraz umożliwiają zasilenie selektorów produktów/składników.

---

## 5. Czego *brakuje*, by zbudować widok dnia z makiety?

1. **Plan dzienny (kalendarz):** w backendzie nie istnieje żaden `MealPlan`/`Schedule` ani endpoint w stylu `GET /api/v1/meal-plans/{date}`. Obecne API operuje tylko na luźnych przepisach. Do odwzorowania ekranu potrzebujemy nowej warstwy, która:
   - Łączy przepisy (ReceiptId) z datą, godziną i typem posiłku.
   - Przechowuje kolejność posiłków oraz ich zaplanowane kalorie.
   - Zwraca agregaty (suma kcal, makro) dla konkretnego dnia.

2. **Agregacja makr dla dnia:** aktualnie makra trzeba by liczyć na froncie, sumując `ReceiptDto` i dane z porcji. Wygodniej byłoby mieć endpoint w stylu `/api/v1/meal-plans/{date}/summary`, który policzy to po stronie serwera.

3. **Edycja planu:** nie ma możliwości „Edytuj/Rozwiń” w kontekście planu (tylko w kontekście recepty). Po dodaniu encji planu przydadzą się:
   - `PUT /api/v1/meal-plans/{date}/meals/{mealId}` (zmiana godziny, kolejności, porcji).
   - `DELETE /api/v1/meal-plans/{date}/meals/{mealId}` (usuwanie).

4. **Integracja przycisku AI:** `POST /api/v1/receipts/generate-with-ai` zwraca pojedynczy przepis. Żeby zrealizować makietę („Generuj plan (AI)”), trzeba będzie w backendzie dorzucić logikę, która:
   - generuje wiele przepisów i przypisuje je do daty,
   - albo pozwala automatycznie wypełnić plan danymi z istniejących recept (np. `/api/v1/meal-plans/{date}/generate`).

---

## 6. Rekomendowany przepływ na dziś (dopóki nie ma modułu planów)

1. Po wejściu na `/app/plan`:
   - `GET /api/v1/users/preferences` → odczyt `dailyCalorieGoal` i celów makro (nagłówki + paski).
   - `GET /api/v1/receipts/me?limit=...` → tymczasowo traktujemy listę recept jako „posiłki dnia” (np. grupując po `CreatedAt`). To tylko obejście do czasu, aż powstanie moduł planów.

2. Klik „Rozwiń” posiłek:
   - `GET /api/v1/receipts/{id}` – pełne dane, składniki, instrukcje.

3. Klik „Generuj plan (AI)”:
   - `POST /api/v1/receipts/generate-with-ai` (body `GenerateRecipeWithAIRequest`). Po sukcesie odświeżamy listę `GET /receipts/me`.

4. Formularze edycyjne:
   - `GET /api/v1/products/search`, `GET /api/v1/units` itd. do wyboru składników.
   - Docelowo trzeba będzie dopisać `PUT`/`DELETE` dla `/api/v1/receipts/{id}` (brak na backendzie).

---

## 7. Następne kroki backendowe

1. **Nowy moduł MealPlans (Controller + Service + Repo)** z encjami:
   - `MealPlanDay { Date, UserId, TargetCalories, ConsumedCalories }`
   - `MealPlanEntry { MealPlanDayId, ReceiptId, MealType, Time, Notes, Order }`

2. **Endpointy wymagane przez UI:**
   - `GET /api/v1/meal-plans/{date}` → zwraca strukturę dokładnie jak na makiecie.
   - `POST /api/v1/meal-plans/{date}/entries` → dodawanie posiłku (manualnie lub z AI).
   - `PUT /api/v1/meal-plans/{date}/entries/{entryId}` → edycja (godzina, nota, przypisanie do innej recepty).
   - `DELETE /api/v1/meal-plans/{date}/entries/{entryId}` → usuwanie.
   - `POST /api/v1/meal-plans/{date}/generate` → opcjonalnie, generuje cały dzień z AI (mogą wewnętrznie korzystać z istniejącego `GenerateRecipeWithAIRequest`).

3. **Rozszerzenie `ReceiptDto` lub stworzenie nowego DTO** z makrami per porcję, żeby front nie musiał przeliczać ręcznie.

---

### Podsumowanie

- **Obecnie** backend oferuje: profil, preferencje (z celami makro), CRUD-ish na przepisach (listowanie, tworzenie, szczegóły), generowanie przepisu AI, dane produktów/jednostek.
- **Brakuje**: czegokolwiek, co mapuje przepisy do dni/posiłków. Aby w pełni odwzorować widok dnia z makiety, trzeba dopisać moduł planu posiłków oraz uzupełnić brakujące operacje (`PUT/DELETE` przepisów, agregaty makro).
- Ten dokument można traktować jako checklistę przy planowaniu prac backendowych potrzebnych do wdrożenia kalendarza posiłków.
