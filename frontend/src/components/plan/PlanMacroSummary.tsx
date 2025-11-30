import { Box, LinearProgress, Stack, Typography } from "@mui/material";
import type { Theme } from "@mui/material/styles";
import type { MacroEntry } from "../../types/plan";

type PlanMacroSummaryProps = {
  calorieTarget: number;
  macroEntries: MacroEntry[];
};

export default function PlanMacroSummary({
  calorieTarget,
  macroEntries,
}: PlanMacroSummaryProps) {
  const totalConsumed = macroEntries.reduce(
    (sum, entry) => sum + Math.max(0, entry.value),
    0
  );

  const entriesWithShare = macroEntries.map((entry) => {
    const percentOfTotal =
      totalConsumed > 0
        ? Math.round((Math.max(0, entry.value) / totalConsumed) * 100)
        : 0;
    const percentOfTarget = entry.target
      ? Math.min(100, Math.round((entry.value / entry.target) * 100))
      : 0;
    return {
      ...entry,
      percentOfTotal,
      percentOfTarget,
    };
  });

  const buildGradient = (theme: Theme) => {
    if (!totalConsumed) {
      return `conic-gradient(${theme.palette.grey[300]} 0deg 360deg)`;
    }

    const colorMap: Record<string, string> = {
      protein: theme.palette.primary.main,
      carbs: theme.palette.warning?.main || theme.palette.info.light,
      fat: theme.palette.success.main,
    };

    let currentAngle = 0;
    const segments = entriesWithShare
      .filter((entry) => entry.percentOfTotal > 0)
      .map((entry) => {
        const sweep = (entry.percentOfTotal / 100) * 360;
        const start = currentAngle;
        const end = start + sweep;
        currentAngle = end;
        const color = colorMap[entry.key] || theme.palette.grey[400];
        return `${color} ${start}deg ${end}deg`;
      });

    return segments.length
      ? `conic-gradient(${segments.join(", ")})`
      : `conic-gradient(${theme.palette.grey[300]} 0deg 360deg)`;
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
          }}
        >
          <Typography variant="body2">
            Cel energii:
            <br />
            <strong>{calorieTarget} kcal</strong>
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
                {entry.percentOfTarget}% celu · {entry.percentOfTotal}% dziennej
                energii
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
