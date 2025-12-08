import { Alert, Box, Divider, Paper, Typography } from "@mui/material";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useDashboardContext } from "../layouts/DashboardLayout";
import type {
  MealPlanDay,
  MacroEntry,
  PlanMeal,
  PlanMealProduct,
} from "../types/plan";
import PlanDayHeader from "../components/plan/PlanDayHeader";
import PlanMealList from "../components/plan/PlanMealList";
import PlanMacroSummary from "../components/plan/PlanMacroSummary";
import PlanAddRecipeModal, {
  type RecipeAddedPayload,
} from "../components/plan/PlanAddRecipeModal";
import PlanPdfExportModal from "../components/plan/PlanPdfExportModal";
import { getRecipeById } from "../services/recipeService";
import {
  getMealPlansForDate,
  deleteMealPlan,
  type MealPlanDto,
} from "../services/mealPlanService";
import { getAllUnits } from "../services/unitService";
import userMeasurementsService from "../services/userMeasurementsService";

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

type MacroTargets = {
  calories: number;
  protein: number;
  fat: number;
  carbs: number;
};

const DEFAULT_MACRO_TARGETS: MacroTargets = {
  calories: 2000,
  protein: 120,
  fat: 70,
  carbs: 260,
};

const buildEmptyPlan = (
  isoDate: string,
  targets: MacroTargets
): MealPlanDay => ({
  date: isoDate,
  consumedCalories: 0,
  targetCalories: roundStat(targets.calories),
  summary: {
    calorieTarget: roundStat(targets.calories),
    macros: {
      protein: { target: roundStat(targets.protein), value: 0 },
      fat: { target: roundStat(targets.fat), value: 0 },
      carbs: { target: roundStat(targets.carbs), value: 0 },
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

const macroLabels: Record<keyof MealPlanDay["summary"]["macros"], string> = {
  protein: "Białko",
  fat: "Tłuszcz",
  carbs: "Węglowodany",
};

const DEFAULT_WEIGHT_UNIT = "g";

type ProductSource = "OpenFoodFacts" | "AI" | "User";

type IngredientForDisplay = {
  productId?: number | string;
  productName: string;
  quantity?: number;
  unitName?: string | null;
  unitId?: number;
  normalizedQuantityInGrams?: number;
  source?: ProductSource;
};

const formatIngredientQuantityLabel = (
  ingredient: IngredientForDisplay
): string | undefined => {
  const isValidQuantity = (value: unknown): value is number =>
    typeof value === "number" && !Number.isNaN(value);

  let unit = ingredient.unitName?.trim();
  let quantity = ingredient.quantity;

  if (
    !isValidQuantity(quantity) &&
    isValidQuantity(ingredient.normalizedQuantityInGrams)
  ) {
    quantity = ingredient.normalizedQuantityInGrams;
    if (!unit) {
      unit = DEFAULT_WEIGHT_UNIT;
    }
  }

  if (!isValidQuantity(quantity) && !unit) {
    return undefined;
  }

  if (!isValidQuantity(quantity)) {
    return unit;
  }

  const rounded = Math.round(quantity * 10) / 10;
  const formattedQuantity = Number.isInteger(rounded)
    ? rounded.toFixed(0)
    : rounded.toFixed(1);
  return unit ? `${formattedQuantity} ${unit}` : formattedQuantity;
};

const mapIngredientToProduct = (
  ingredient: IngredientForDisplay,
  index: number,
  unitNameOverride?: string
): PlanMealProduct => ({
  id: `${ingredient.productId ?? ingredient.productName}-${index}`,
  productId:
    typeof ingredient.productId === "number" ? ingredient.productId : undefined,
  name: ingredient.productName,
  quantityLabel: formatIngredientQuantityLabel({
    ...ingredient,
    unitName: unitNameOverride ?? ingredient.unitName,
  }),
  source: ingredient.source,
});

const defaultSlotLookup = defaultMealSlots.reduce<
  Record<string, (typeof defaultMealSlots)[number]>
>((acc, slot) => {
  acc[slot.label] = slot;
  return acc;
}, {});

const slotOrder = defaultMealSlots.map((slot) => slot.label);

const roundStat = (value: number) => Math.round(Number(value) || 0);

const withUpdatedSummary = (
  plan: MealPlanDay,
  targets: MacroTargets
): MealPlanDay => {
  const totals = plan.meals.reduce(
    (acc, meal) => {
      acc.calories += roundStat(meal.calories || 0);
      acc.protein += roundStat(meal.macros?.protein || 0);
      acc.fat += roundStat(meal.macros?.fat || 0);
      acc.carbs += roundStat(meal.macros?.carbs || 0);
      return acc;
    },
    { calories: 0, protein: 0, fat: 0, carbs: 0 }
  );

  return {
    ...plan,
    consumedCalories: totals.calories,
    targetCalories: roundStat(targets.calories),
    summary: {
      calorieTarget: roundStat(targets.calories),
      macros: {
        protein: { target: roundStat(targets.protein), value: totals.protein },
        fat: { target: roundStat(targets.fat), value: totals.fat },
        carbs: { target: roundStat(targets.carbs), value: totals.carbs },
      },
    },
  };
};

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
    buildEmptyPlan(selectedDate, DEFAULT_MACRO_TARGETS)
  );
  const [expandedMealId, setExpandedMealId] = useState<string | null>(null);
  const [mealForModal, setMealForModal] = useState<PlanMeal | null>(null);
  const [isPdfModalOpen, setIsPdfModalOpen] = useState(false);
  const [isPlanLoading, setIsPlanLoading] = useState(false);
  const [planLoadError, setPlanLoadError] = useState<string | null>(null);
  const unitNameCacheRef = useRef<Record<number, string>>({});
  const macroTargetsRef = useRef<MacroTargets>(DEFAULT_MACRO_TARGETS);

  const convertIngredientsToProducts = useCallback(
    (ingredients: IngredientForDisplay[]) =>
      ingredients.map((ingredient, index) => {
        const fallbackUnit =
          typeof ingredient.unitId === "number"
            ? unitNameCacheRef.current[ingredient.unitId]
            : undefined;
        return mapIngredientToProduct(ingredient, index, fallbackUnit);
      }),
    []
  );

  const ensureUnitsLoaded = useCallback(async () => {
    if (Object.keys(unitNameCacheRef.current).length) {
      return unitNameCacheRef.current;
    }
    try {
      const units = await getAllUnits();
      const map = units.reduce<Record<number, string>>((acc, unit) => {
        if (Number.isFinite(unit.unitId)) {
          acc[unit.unitId] = unit.name;
        }
        return acc;
      }, {});
      unitNameCacheRef.current = map;
    } catch (error) {
      console.error("Nie udało się pobrać listy jednostek", error);
    }
    return unitNameCacheRef.current;
  }, []);

  useEffect(() => {
    let isCancelled = false;

    const loadPlan = async () => {
      const emptyPlan = buildEmptyPlan(selectedDate, macroTargetsRef.current);
      setPlan(emptyPlan);
      setPlanLoadError(null);
      setIsPlanLoading(true);

      try {
        const apiMeals = await getMealPlansForDate(selectedDate);
        if (isCancelled) {
          return;
        }
        const mergedPlan = mergeMealPlansFromApi(emptyPlan, apiMeals);
        setPlan(withUpdatedSummary(mergedPlan, macroTargetsRef.current));
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
    let isCancelled = false;

    const loadNutritionTargets = async () => {
      try {
        const prefs = await userMeasurementsService.getPreferences();
        if (isCancelled) {
          return;
        }

        const normalizeGoal = (
          value?: number | null,
          fallback?: number
        ): number =>
          typeof value === "number" && Number.isFinite(value)
            ? value
            : (fallback ?? 0);

        const nextTargets: MacroTargets = {
          calories: normalizeGoal(
            prefs.dailyCalorieGoal ?? prefs.calculatedDailyCalories,
            DEFAULT_MACRO_TARGETS.calories
          ),
          protein: normalizeGoal(
            prefs.dailyProteinGoal,
            DEFAULT_MACRO_TARGETS.protein
          ),
          fat: normalizeGoal(prefs.dailyFatGoal, DEFAULT_MACRO_TARGETS.fat),
          carbs: normalizeGoal(
            prefs.dailyCarbohydrateGoal,
            DEFAULT_MACRO_TARGETS.carbs
          ),
        };

        macroTargetsRef.current = nextTargets;
        setPlan((prev) => withUpdatedSummary(prev, nextTargets));
      } catch (error) {
        console.error("Nie udało się obliczyć makr użytkownika", error);
      }
    };

    void loadNutritionTargets();

    return () => {
      isCancelled = true;
    };
  }, []);

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
        await ensureUnitsLoaded();
      } catch (error) {
        console.error("Failed to prepare units before loading recipe", error);
      }

      try {
        const recipe = await getRecipeById(recipeId);
        const products = convertIngredientsToProducts(recipe.ingredients);
        setPlan((prev) => ({
          ...prev,
          meals: prev.meals.map((planMeal) =>
            planMeal.id === mealId
              ? {
                  ...planMeal,
                  products,
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
    [convertIngredientsToProducts, ensureUnitsLoaded, setPlan]
  );

  const handleAddRecipe = (meal: PlanMeal) => {
    setMealForModal(meal);
  };

  const handleEditMeal = (meal: PlanMeal) => {
    setMealForModal(meal);
  };

  const handleDeleteMeal = async (meal: PlanMeal) => {
    if (!meal.mealPlanId) return;

    if (!confirm(`Czy na pewno chcesz usunąć "${meal.title}" z planu?`)) return;

    try {
      await deleteMealPlan(meal.mealPlanId);

      setPlan((prev) => {
        const updatedMeals = prev.meals.map((planMeal) => {
          if (planMeal.id !== meal.id) {
            return planMeal;
          }

          const defaultSlot = defaultSlotLookup[planMeal.type];
          return {
            id: planMeal.id,
            type: planMeal.type,
            time: defaultSlot?.time ?? "",
            isPlaceholder: true,
            title: "",
            description: "",
            calories: 0,
            macros: { protein: 0, fat: 0, carbs: 0 },
            products: [],
            instructions: "",
            isDetailsLoading: false,
            detailsError: null,
          };
        });

        return withUpdatedSummary(
          {
            ...prev,
            meals: updatedMeals,
          },
          macroTargetsRef.current
        );
      });
    } catch (error) {
      console.error("Failed to delete meal plan:", error);
      alert("Nie udało się usunąć posiłku z planu");
    }
  };

  const handleCloseModal = () => setMealForModal(null);

  const handleOpenPdfModal = () => setIsPdfModalOpen(true);
  const handleClosePdfModal = () => setIsPdfModalOpen(false);
  const handleRecipeAddedToPlan = (payload: RecipeAddedPayload) => {
    const products = convertIngredientsToProducts(payload.recipe.ingredients);
    setPlan((prev) => {
      const updatedMeals = prev.meals.map((planMeal) => {
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
          products,
          instructions: payload.recipe.instructions,
          isDetailsLoading: false,
          detailsError: null,
          mealPlanId: payload.mealPlanId,
          recipeId: payload.recipeId,
        };
      });

      return withUpdatedSummary(
        {
          ...prev,
          meals: updatedMeals,
        },
        macroTargetsRef.current
      );
    });
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
    { target: number; value: number },
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
          onExportPdf={handleOpenPdfModal}
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
          onEditMeal={handleEditMeal}
          onDeleteMeal={handleDeleteMeal}
          expandedMealId={expandedMealId}
          onExpandMeal={handleExpandMeal}
        />
      </Paper>

      <Paper elevation={1} sx={{ p: 3 }}>
        <PlanMacroSummary
          calorieTarget={plan.summary.calorieTarget}
          consumedCalories={plan.consumedCalories}
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

      <PlanPdfExportModal
        open={isPdfModalOpen}
        onClose={handleClosePdfModal}
        planData={plan}
        dateLabel={formatDate(plan.date)}
      />
    </Box>
  );
}
