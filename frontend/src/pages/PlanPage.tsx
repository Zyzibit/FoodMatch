import {
  Box,
  Button,
  Divider,
  IconButton,
  LinearProgress,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import {
  CalendarMonth,
  ChevronLeft,
  ChevronRight,
  Edit,
} from "@mui/icons-material";
import { useEffect, useMemo, useState } from "react";
import { useDashboardContext } from "../layouts/DashboardLayout";

type PlanMeal = {
  id: string;
  time: string;
  type: string;
  title: string;
  calories: number;
  macros: { protein: number; fat: number; carbs: number };
};

type MealPlanDay = {
  date: string;
  consumedCalories: number;
  targetCalories: number;
  summary: {
    calorieTarget: number;
    macros: {
      protein: { target: number; value: number };
      fat: { target: number; value: number };
      carbs: { target: number; value: number };
    };
  };
  meals: PlanMeal[];
};

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

  return (
    <Box sx={{ width: "100%", display: "grid", gap: 3, gridTemplateColumns: { md: "2fr 1fr", xs: "1fr" } }}>
      <Paper elevation={1} sx={{ p: 3 }}>
        <Stack
          direction={{ xs: "column", sm: "row" }}
          alignItems={{ xs: "flex-start", sm: "center" }}
          justifyContent="space-between"
          spacing={2}
        >
          <Box>
            <Typography variant="h3" fontWeight={800} sx={{ lineHeight: 1 }}>
              {plan.consumedCalories} KCAL
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {formatDate(plan.date)}
            </Typography>
          </Box>
          <Stack direction="row" spacing={1}>
            <IconButton size="small" aria-label="Poprzedni dzień">
              <ChevronLeft />
            </IconButton>
            <IconButton size="small" aria-label="Wybierz dzień">
              <CalendarMonth />
            </IconButton>
            <IconButton size="small" aria-label="Następny dzień">
              <ChevronRight />
            </IconButton>
          </Stack>
        </Stack>

        <Typography
          variant="subtitle2"
          color="text.secondary"
          sx={{ mt: 2, mb: 1 }}
        >
          Zapotrzebowanie kaloryczne: {plan.summary.calorieTarget} kcal
        </Typography>

        <Divider sx={{ mb: 2 }} />

        <Stack spacing={2}>
          {plan.meals.map((meal) => (
            <Box
              key={meal.id}
              sx={{
                borderBottom: (theme) => `1px solid ${theme.palette.grey[200]}`,
                pb: 1.5,
              }}
            >
              <Stack
                direction="row"
                justifyContent="space-between"
                flexWrap="wrap"
                spacing={1}
              >
                <Box>
                  <Typography
                    variant="body2"
                    color="text.secondary"
                    fontWeight={600}
                  >
                    {meal.time} · {meal.calories} kcal
                  </Typography>
                  <Typography variant="h6" fontWeight={800}>
                    {meal.type}
                  </Typography>
                  <Typography variant="body2">{meal.title}</Typography>
                  <Typography variant="body2" color="text.secondary" mt={0.5}>
                    B: {meal.macros.protein}g&nbsp; T: {meal.macros.fat}g&nbsp;
                    W: {meal.macros.carbs}g
                  </Typography>
                </Box>
                <Stack spacing={1} alignItems="flex-end">
                  <Button
                    startIcon={<Edit />}
                    size="small"
                    variant="text"
                    sx={{ textTransform: "none" }}
                  >
                    Edytuj
                  </Button>
                  <Button size="small" variant="text" sx={{ textTransform: "none" }}>
                    Rozwiń
                  </Button>
                </Stack>
              </Stack>
            </Box>
          ))}
        </Stack>
      </Paper>

      <Paper elevation={1} sx={{ p: 3 }}>
        <Typography variant="subtitle1" fontWeight={700} gutterBottom>
          Podsumowanie makro
        </Typography>

        <Box
          sx={{
            width: 180,
            height: 180,
            borderRadius: "50%",
            mx: "auto",
            mb: 3,
            background: (theme) =>
              `conic-gradient(${theme.palette.secondary.main} 0deg 120deg, ${theme.palette.info.light} 120deg 240deg, ${theme.palette.grey[300]} 240deg 360deg)`,
            position: "relative",
          }}
        >
          <Box
            sx={{
              position: "absolute",
              top: 20,
              left: 20,
              right: 20,
              bottom: 20,
              borderRadius: "50%",
              bgcolor: "background.paper",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
            }}
          >
            <Typography variant="body2" align="center">
              Białko 30% <br /> Tłuszcz 25% <br /> Węgle 45%
            </Typography>
          </Box>
        </Box>

        <Stack spacing={2} mb={3}>
          {macroEntries.map(([key, macro]) => {
            const percent = Math.min(
              100,
              Math.round((macro.value / macro.target) * 100)
            );
            return (
              <Box key={key}>
                <Stack
                  direction="row"
                  justifyContent="space-between"
                  alignItems="center"
                >
                  <Typography variant="body2" fontWeight={600}>
                    {macroLabels[key]}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {percent}% ({macro.value} / {macro.target} g)
                  </Typography>
                </Stack>
                <LinearProgress
                  variant="determinate"
                  value={percent}
                  sx={{ height: 8, borderRadius: 4, mt: 0.5 }}
                />
              </Box>
            );
          })}
        </Stack>

        <Button
          variant="contained"
          color="secondary"
          fullWidth
          size="large"
          sx={{ borderRadius: 2, textTransform: "none", fontWeight: 700 }}
        >
          Generuj plan (AI)
        </Button>
      </Paper>
    </Box>
  );
}
