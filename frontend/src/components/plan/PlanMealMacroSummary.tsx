import { Box, Stack, Typography } from "@mui/material";
import type { PlanMeal } from "../../types/plan";

type PlanMealMacroSummaryProps = {
  macros: PlanMeal["macros"];
  variant?: "compact" | "detailed";
};

const macroConfig: {
  key: keyof PlanMeal["macros"];
  label: string;
  short: string;
}[] = [
  { key: "protein", label: "Białko", short: "B" },
  { key: "fat", label: "Tłuszcz", short: "T" },
  { key: "carbs", label: "Węglowodany", short: "W" },
];

export default function PlanMealMacroSummary({
  macros,
  variant = "compact",
}: PlanMealMacroSummaryProps) {
  if (variant === "compact") {
    return (
      <Typography variant="body2" color="text.secondary" mt={0.5}>
        B: {macros.protein}g&nbsp; T: {macros.fat}g&nbsp; W: {macros.carbs}g
      </Typography>
    );
  }

  return (
    <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5} mt={1.5}>
      {macroConfig.map(({ key, label, short }) => (
        <Box
          key={key}
          sx={(theme) => ({
            flex: 1,
            borderRadius: 2,
            p: 1,
            backgroundColor: theme.palette.action.hover,
          })}
        >
          <Typography variant="overline" color="text.secondary">
            {label}
          </Typography>
          <Typography variant="h6" fontWeight={700}>
            {macros[key]} g
          </Typography>
          <Typography variant="caption" color="text.secondary">
            ({short})
          </Typography>
        </Box>
      ))}
    </Stack>
  );
}
