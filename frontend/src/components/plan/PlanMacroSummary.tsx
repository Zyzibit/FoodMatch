import { Box, LinearProgress, Stack, Typography } from "@mui/material";
import type { MacroEntry } from "../../types/plan";

type PlanMacroSummaryProps = {
  calorieTarget: number;
  macroEntries: MacroEntry[];
};

export default function PlanMacroSummary({
  calorieTarget,
  macroEntries,
}: PlanMacroSummaryProps) {
  return (
    <Stack spacing={4} alignItems="center">
      <Typography variant="subtitle1" fontWeight={700}>
        Podsumowanie makro
      </Typography>

      <Box
        sx={{
          width: 220,
          height: 220,
          borderRadius: "50%",
          background: (theme) =>
            `conic-gradient(${theme.palette.secondary.main} 0deg 120deg, ${theme.palette.info.light || theme.palette.primary.light} 120deg 240deg, ${theme.palette.grey[300]} 240deg 360deg)`,
          position: "relative",
        }}
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
          <Typography variant="body2">Cel: {calorieTarget} kcal</Typography>
        </Box>
      </Box>

      <Stack spacing={2} sx={{ width: "100%" }}>
        {macroEntries.map((entry) => {
          const percent = Math.min(
            100,
            Math.round((entry.value / entry.target) * 100)
          );
          return (
            <Box key={entry.key}>
              <Stack
                direction="row"
                justifyContent="space-between"
                alignItems="center"
              >
                <Typography variant="body2" fontWeight={600}>
                  {entry.label}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {percent}% ({entry.value} / {entry.target} g)
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
    </Stack>
  );
}
