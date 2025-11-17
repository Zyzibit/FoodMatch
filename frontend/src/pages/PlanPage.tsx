import { Box, Divider, Paper, Typography } from "@mui/material";
import { useEffect, useMemo, useState } from "react";
import { useDashboardContext } from "../layouts/DashboardLayout";
import type { MealPlanDay, MacroEntry, PlanMeal } from "../types/plan";
import PlanDayHeader from "../components/plan/PlanDayHeader";
import PlanMealList from "../components/plan/PlanMealList";
import PlanMacroSummary from "../components/plan/PlanMacroSummary";
import PlanAddRecipeModal from "../components/plan/PlanAddRecipeModal";

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

  useEffect(() => {
    setPlan(buildEmptyPlan(selectedDate));
  }, [selectedDate]);

  useEffect(() => {
    setExpandedMealId(null);
  }, [selectedDate]);

  const handleAddRecipe = (meal: PlanMeal) => {
    setMealForModal(meal);
  };

  const handleCloseModal = () => setMealForModal(null);

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

        <PlanMealList
          meals={plan.meals}
          onAddRecipe={handleAddRecipe}
          expandedMealId={expandedMealId}
          onExpandMeal={(meal) =>
            setExpandedMealId((current) =>
              current === meal.id ? null : meal.id
            )
          }
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
        onClose={handleCloseModal}
      />
    </Box>
  );
}
