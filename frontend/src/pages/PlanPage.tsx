import { Box, Divider, Paper, Typography } from "@mui/material";
import { useEffect, useMemo, useState } from "react";
import { useDashboardContext } from "../layouts/DashboardLayout";
import type { MealPlanDay, PlanMeal, MacroEntry } from "../types/plan";
import PlanDayHeader from "../components/plan/PlanDayHeader";
import PlanMealList from "../components/plan/PlanMealList";
import PlanMacroSummary from "../components/plan/PlanMacroSummary";

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

const buildMockPlan = (isoDate: string): MealPlanDay => ({
  date: isoDate,
  consumedCalories: 1800,
  targetCalories: 1800,
  summary: {
    calorieTarget: 2000,
    macros: {
      protein: { target: 120, value: 95 },
      fat: { target: 70, value: 50 },
      carbs: { target: 260, value: 210 },
    },
  },
  meals: [
    {
      id: "meal-1",
      time: "08:00",
      type: "Śniadanie",
      title: "Płatki owsiane z owocami",
      calories: 365,
      macros: { protein: 10, fat: 6, carbs: 60 },
    },
    {
      id: "meal-2",
      time: "11:30",
      type: "Drugie śniadanie",
      title: "Jogurt z granolą",
      calories: 320,
      macros: { protein: 14, fat: 8, carbs: 45 },
    },
    {
      id: "meal-3",
      time: "14:30",
      type: "Obiad",
      title: "Pierś z kurczaka z warzywami",
      calories: 520,
      macros: { protein: 42, fat: 18, carbs: 48 },
    },
    {
      id: "meal-4",
      time: "19:00",
      type: "Kolacja",
      title: "Sałatka z halloumi",
      calories: 595,
      macros: { protein: 22, fat: 30, carbs: 35 },
    },
  ],
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
  const [plan, setPlan] = useState<MealPlanDay | null>(null);

  const selectedDate = useMemo(
    () => parseDateKey(activeTab) ?? new Date().toISOString().slice(0, 10),
    [activeTab]
  );

  useEffect(() => {
    // TODO: zastąpić mock wywołaniem GET /api/v1/meal-plans/{selectedDate}
    setPlan(buildMockPlan(selectedDate));
  }, [selectedDate]);

  if (!plan) {
    return (
      <Paper elevation={1} sx={{ p: 3, width: "100%", maxWidth: 1100 }}>
        <Typography variant="h5" fontWeight={800}>
          Ładowanie planu…
        </Typography>
      </Paper>
    );
  }

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

        <PlanMealList meals={plan.meals} />
      </Paper>

      <Paper elevation={1} sx={{ p: 3 }}>
        <PlanMacroSummary
          calorieTarget={plan.summary.calorieTarget}
          macroEntries={macroData}
        />
      </Paper>
    </Box>
  );
}
