import { Alert, Box, Divider, Paper, Typography } from "@mui/material";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useDashboardContext } from "../layouts/DashboardLayout";
import type { MealPlanDay, MacroEntry, PlanMeal } from "../types/plan";
import PlanDayHeader from "../components/plan/PlanDayHeader";
import PlanMealList from "../components/plan/PlanMealList";
import PlanMacroSummary from "../components/plan/PlanMacroSummary";
import PlanAddRecipeModal, {
  type RecipeAddedPayload,
} from "../components/plan/PlanAddRecipeModal";
import { getRecipeById, type GeneratedRecipe } from "../services/recipeService";
import {
  getMealPlansForDate,
  type MealPlanDto,
} from "../services/mealPlanService";

const parseDateKey = (key?: string) => {
  if (!key?.startsWith("date-")) return null;
  return key.replace("date-", "");
};

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString("pl-PL", {
    weekday: "long",
    month: "long",
    day: "numeric",
  });

const defaultMealSlots = [
  { id: "meal-breakfast", label: "Śniadanie", time: "07:30" },
  { id: "meal-snack", label: "Drugie śniadanie", time: "11:00" },
  { id: "meal-lunch", label: "Obiad", time: "14:30" },
  { id: "meal-dinner", label: "Kolacja", time: "19:00" },
];

const englishToPolishMealName: Record<string, string> = {
  Breakfast: "Śniadanie",
  Snack: "Drugie śniadanie",
  Lunch: "Obiad",
  Dinner: "Kolacja",
};

const buildEmptyPlan = (isoDate: string): MealPlanDay => ({
  date: isoDate,
  consumedCalories: 0,
  targetCalories: 0,
  summary: {
    calorieTarget: 2000,
    macros: {
      protein: { target: 120, value: 0 },
      fat: { target: 70, value: 0 },
      carbs: { target: 260, value: 0 },
    },
  },
  meals: defaultMealSlots.map((slot) => ({
    id: slot.id,
    time: slot.time,
    type: slot.label,
    title: "",
    calories: 0,
    macros: { protein: 0, fat: 0, carbs: 0 },
    isPlaceholder: true,
  })),
});

const macroLabels: Record<
  keyof MealPlanDay["summary"]["macros"],
  string
> = {
  protein: "Białko",
  fat: "Tłuszcz",
  carbs: "Węglowodany",
};

type IngredientForDisplay = {
  productName: string;
  quantity?: number;
  unitName?: string | null;
};

const formatIngredientLabel = (ingredient: IngredientForDisplay) => {
  const quantity =
    typeof ingredient.quantity === "number"
      ? Math.round(ingredient.quantity * 10) / 10
      : undefined;
  const unit = ingredient.unitName?.trim();

  if (quantity && unit) {
    return `${ingredient.productName} (${quantity} ${unit})`;
  }

  if (quantity) {
    return `${ingredient.productName} (${quantity})`;
  }

  return ingredient.productName;
};

const defaultSlotLookup = defaultMealSlots.reduce<
  Record<string, (typeof defaultMealSlots)[number]>
>((acc, slot) => {
  acc[slot.label] = slot;
  return acc;
}, {});

const slotOrder = defaultMealSlots.map((slot) => slot.label);

const roundStat = (value: number) => Math.round(Number(value) || 0);

const mergeMealPlansFromApi = (
  basePlan: MealPlanDay,
  apiMeals: MealPlanDto[]
): MealPlanDay => {
  if (!apiMeals.length) {
    return basePlan;
  }

  const apiMealsByLabel = new Map<string, MealPlanDto>();
  apiMeals.forEach((mealPlan) => {
    const label = englishToPolishMealName[mealPlan.name] ?? mealPlan.name;
    apiMealsByLabel.set(label, mealPlan);
  });

  const updatedMeals = basePlan.meals.map((meal) => {
    const apiMeal = apiMealsByLabel.get(meal.type);
    if (!apiMeal || !apiMeal.recipe) {
      return meal;
    }

    const recipe = apiMeal.recipe;
    return {
      ...meal,
      isPlaceholder: false,
      title: recipe.title,
      description: recipe.description,
      calories: roundStat(recipe.calories),
      macros: {
        protein: roundStat(recipe.proteins),
        fat: roundStat(recipe.fats),
        carbs: roundStat(recipe.carbohydrates),
      },
      mealPlanId: apiMeal.id,
      recipeId: recipe.id,
    };
  });

  const additionalMeals: PlanMeal[] = [];
  apiMealsByLabel.forEach((apiMeal, label) => {
    if (!apiMeal.recipe) {
      return;
    }
    const alreadyIncluded = updatedMeals.some((meal) => meal.type === label);
    if (alreadyIncluded) {
      return;
    }

    const slotInfo = defaultSlotLookup[label];
    let time = slotInfo?.time;
    if (!time) {
      const apiDate = new Date(apiMeal.date);
      time = Number.isNaN(apiDate.getTime())
        ? "00:00"
        : apiDate.toLocaleTimeString("pl-PL", {
            hour: "2-digit",
            minute: "2-digit",
          });
    }

    additionalMeals.push({
      id: `api-${apiMeal.id}`,
      type: label,
      time,
      title: apiMeal.recipe.title,
      description: apiMeal.recipe.description,
      calories: roundStat(apiMeal.recipe.calories),
      macros: {
        protein: roundStat(apiMeal.recipe.proteins),
        fat: roundStat(apiMeal.recipe.fats),
        carbs: roundStat(apiMeal.recipe.carbohydrates),
      },
      products: [],
      isPlaceholder: false,
      mealPlanId: apiMeal.id,
      recipeId: apiMeal.recipe.id,
    });
  });

  const mergedMeals = [...updatedMeals, ...additionalMeals];

  mergedMeals.sort((a, b) => {
    const indexA = slotOrder.indexOf(a.type);
    const indexB = slotOrder.indexOf(b.type);
    if (indexA === -1 && indexB === -1) {
      return a.time.localeCompare(b.time);
    }
    if (indexA === -1) return 1;
    if (indexB === -1) return -1;
    return indexA - indexB;
  });

  return {
    ...basePlan,
    meals: mergedMeals,
  };
};

export default function PlanPage() {
  const { activeTab } = useDashboardContext();
  const selectedDate = useMemo(
    () => parseDateKey(activeTab) ?? new Date().toISOString().slice(0, 10),
    [activeTab]
  );

  const [plan, setPlan] = useState<MealPlanDay>(() =>
    buildEmptyPlan(selectedDate)
  );
  const [expandedMealId, setExpandedMealId] = useState<string | null>(null);
  const [mealForModal, setMealForModal] = useState<PlanMeal | null>(null);
  const [isPlanLoading, setIsPlanLoading] = useState(false);
  const [planLoadError, setPlanLoadError] = useState<string | null>(null);

  useEffect(() => {
    let isCancelled = false;

    const loadPlan = async () => {
      const emptyPlan = buildEmptyPlan(selectedDate);
      setPlan(emptyPlan);
      setPlanLoadError(null);
      setIsPlanLoading(true);

      try {
        const apiMeals = await getMealPlansForDate(selectedDate);
        if (isCancelled) {
          return;
        }
        const mergedPlan = mergeMealPlansFromApi(emptyPlan, apiMeals);
        setPlan(mergedPlan);
      } catch (error) {
        if (isCancelled) {
          return;
        }
        setPlanLoadError(
          error instanceof Error
            ? error.message
            : "Nie udało się pobrać planu posiłków."
        );
      } finally {
        if (!isCancelled) {
          setIsPlanLoading(false);
        }
      }
    };

    void loadPlan();

    return () => {
      isCancelled = true;
    };
  }, [selectedDate]);

  useEffect(() => {
    setExpandedMealId(null);
  }, [selectedDate]);

  const fetchMealDetails = useCallback(
    async (mealId: string, recipeId: number) => {
      setPlan((prev) => ({
        ...prev,
        meals: prev.meals.map((planMeal) =>
          planMeal.id === mealId
            ? { ...planMeal, isDetailsLoading: true, detailsError: null }
            : planMeal
        ),
      }));

      try {
        const recipe = await getRecipeById(recipeId);
        setPlan((prev) => ({
          ...prev,
          meals: prev.meals.map((planMeal) =>
            planMeal.id === mealId
              ? {
                  ...planMeal,
                  products: recipe.ingredients.map((ingredient) =>
                    formatIngredientLabel(ingredient)
                  ),
                  instructions: recipe.instructions,
                  isDetailsLoading: false,
                  detailsError: null,
                }
              : planMeal
          ),
        }));
      } catch (error) {
        console.error("Failed to load recipe details", error);
        setPlan((prev) => ({
          ...prev,
          meals: prev.meals.map((planMeal) =>
            planMeal.id === mealId
              ? {
                  ...planMeal,
                  isDetailsLoading: false,
                  detailsError:
                    error instanceof Error
                      ? error.message
                      : "Nie udało się pobrać szczegółów przepisu.",
                }
              : planMeal
          ),
        }));
      }
    },
    [setPlan]
  );

  const handleAddRecipe = (meal: PlanMeal) => {
    setMealForModal(meal);
  };

  const handleCloseModal = () => setMealForModal(null);

  const handleRecipeAddedToPlan = (payload: RecipeAddedPayload) => {
    setPlan((prev) => ({
      ...prev,
      meals: prev.meals.map((planMeal) => {
        if (planMeal.id !== payload.meal.id) {
          return planMeal;
        }

        return {
          ...planMeal,
          isPlaceholder: false,
          title: payload.recipe.title,
          description: payload.recipe.description,
          calories: roundStat(payload.recipe.calories),
          macros: {
            protein: roundStat(payload.recipe.proteins),
            fat: roundStat(payload.recipe.fats),
            carbs: roundStat(payload.recipe.carbohydrates),
          },
          products: payload.recipe.ingredients.map((ingredient) =>
            formatIngredientLabel(ingredient)
          ),
          instructions: payload.recipe.instructions,
          isDetailsLoading: false,
          detailsError: null,
          mealPlanId: payload.mealPlanId,
          recipeId: payload.recipeId,
        };
      }),
    }));
    setMealForModal(null);
  };

  const handleExpandMeal = (meal: PlanMeal) => {
    const isExpanding = expandedMealId !== meal.id;
    setExpandedMealId(isExpanding ? meal.id : null);

    if (
      !isExpanding ||
      meal.isPlaceholder ||
      !meal.recipeId ||
      meal.isDetailsLoading
    ) {
      return;
    }

    const hasProducts = Boolean(meal.products?.length);
    const hasInstructions = Boolean(meal.instructions);
    const shouldRefetch = Boolean(meal.detailsError);

    if (!hasProducts || !hasInstructions || shouldRefetch) {
      void fetchMealDetails(meal.id, meal.recipeId);
    }
  };

  const macroEntries = Object.entries(plan.summary.macros) as [
    keyof MealPlanDay["summary"]["macros"],
    { target: number; value: number }
  ][];
  const macroData: MacroEntry[] = macroEntries.map(([key, value]) => ({
    key,
    label: macroLabels[key],
    target: value.target,
    value: value.value,
  }));

  return (
    <Box
      sx={{
        width: "100%",
        display: "grid",
        gap: 3,
        gridTemplateColumns: { md: "2fr 1fr", xs: "1fr" },
      }}
    >
      <Paper elevation={1} sx={{ p: 3 }}>
        <PlanDayHeader
          consumedCalories={plan.consumedCalories}
          dateLabel={formatDate(plan.date)}
        />

        <Typography
          variant="subtitle2"
          color="text.secondary"
          sx={{ mt: 2, mb: 1 }}
        >
          Zapotrzebowanie kaloryczne: {plan.summary.calorieTarget} kcal
        </Typography>

        <Divider sx={{ mb: 2 }} />

        {planLoadError && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {planLoadError}
          </Alert>
        )}

        {isPlanLoading && (
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Ładowanie planu...
          </Typography>
        )}

        <PlanMealList
          meals={plan.meals}
          onAddRecipe={handleAddRecipe}
          expandedMealId={expandedMealId}
          onExpandMeal={handleExpandMeal}
        />
      </Paper>

      <Paper elevation={1} sx={{ p: 3 }}>
        <PlanMacroSummary
          calorieTarget={plan.summary.calorieTarget}
          macroEntries={macroData}
        />
      </Paper>

      <PlanAddRecipeModal
        open={Boolean(mealForModal)}
        meal={mealForModal}
        planDate={plan.date}
        onClose={handleCloseModal}
        onRecipeAdded={handleRecipeAddedToPlan}
      />
    </Box>
  );
}
