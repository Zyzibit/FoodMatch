import { Box, LinearProgress, Stack, Typography } from "@mui/material";
import type { Theme } from "@mui/material/styles";
import type { MacroEntry } from "../../types/plan";

type PlanMacroSummaryProps = {
  calorieTarget: number;
  consumedCalories: number;
  macroEntries: MacroEntry[];
};

export default function PlanMacroSummary({
  calorieTarget,
  consumedCalories,
  macroEntries,
}: PlanMacroSummaryProps) {
  // Oblicz procentowe wypełnienie celu kalorycznego na podstawie rzeczywistych kalorii
  const caloriePercentage =
    calorieTarget > 0
      ? Math.min(100, Math.round((consumedCalories / calorieTarget) * 100))
      : 0;

  const entriesWithShare = macroEntries.map((entry) => {
    const percentOfTarget = entry.target
      ? Math.min(100, Math.round((entry.value / entry.target) * 100))
      : 0;
    return {
      ...entry,
      percentOfTarget,
    };
  });

  const buildGradient = (theme: Theme) => {
    // Jeśli brak spożytych kalorii - cały wykres szary (pusty)
    if (!consumedCalories || caloriePercentage === 0) {
      return `conic-gradient(${theme.palette.grey[300]} 0deg 360deg)`;
    }

    // Oblicz kąt końcowy na podstawie procentu celu kalorycznego
    const totalAngle = (caloriePercentage / 100) * 360;

    // Wykres pokazuje tylko kalorie jednym kolorem (primary)
    const calorieColor = theme.palette.primary.main;

    // Dodaj szarą część reprezentującą nieosiągnięty cel
    if (caloriePercentage < 100) {
      return `conic-gradient(${calorieColor} 0deg ${totalAngle}deg, ${theme.palette.grey[300]} ${totalAngle}deg 360deg)`;
    }

    // Jeśli osiągnięto 100% - cały wykres w kolorze primary
    return `conic-gradient(${calorieColor} 0deg 360deg)`;
  };

  return (
    <Stack spacing={4} alignItems="center">
      <Typography variant="subtitle1" fontWeight={700}>
        Podsumowanie makro
      </Typography>

      <Box
        sx={(theme) => ({
          width: 220,
          height: 220,
          borderRadius: "50%",
          background: buildGradient(theme),
          position: "relative",
          boxShadow: `0 0 12px ${theme.palette.grey[200]}`,
        })}
      >
        <Box
          sx={{
            position: "absolute",
            top: 28,
            left: 28,
            right: 28,
            bottom: 28,
            borderRadius: "50%",
            bgcolor: "background.paper",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            textAlign: "center",
            px: 2,
            flexDirection: "column",
          }}
        >
          <Typography variant="h5" fontWeight={700} color="primary">
            {caloriePercentage}%
          </Typography>
          <Typography variant="caption" color="text.secondary">
            {consumedCalories} / {calorieTarget} kcal
          </Typography>
        </Box>
      </Box>

      <Stack spacing={2} sx={{ width: "100%" }}>
        {entriesWithShare.map((entry) => (
          <Box key={entry.key}>
            <Stack direction="row" justifyContent="space-between">
              <Typography variant="body2" fontWeight={600}>
                {entry.label}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {entry.percentOfTarget}% celu
              </Typography>
            </Stack>
            <Typography
              variant="caption"
              color="text.secondary"
              sx={{ display: "block", mb: 0.5 }}
            >
              {entry.value} / {entry.target} g
            </Typography>
            <LinearProgress
              variant="determinate"
              value={entry.percentOfTarget}
              sx={{ height: 8, borderRadius: 4 }}
            />
          </Box>
        ))}
      </Stack>
    </Stack>
  );
}
