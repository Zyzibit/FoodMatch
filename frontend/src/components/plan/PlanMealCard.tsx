import { Box, Button, Stack, Typography } from "@mui/material";
import { Edit } from "@mui/icons-material";
import type { PlanMeal } from "../../types/plan";

type PlanMealCardProps = {
  meal: PlanMeal;
  onEdit?: (meal: PlanMeal) => void;
  onExpand?: (meal: PlanMeal) => void;
};

export default function PlanMealCard({
  meal,
  onEdit,
  onExpand,
}: PlanMealCardProps) {
  return (
    <Box
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
          <Typography variant="body2" color="text.secondary" fontWeight={600}>
            {meal.time} · {meal.calories} kcal
          </Typography>
          <Typography variant="h6" fontWeight={800}>
            {meal.type}
          </Typography>
          <Typography variant="body2">{meal.title}</Typography>
          <Typography variant="body2" color="text.secondary" mt={0.5}>
            B: {meal.macros.protein}g&nbsp; T: {meal.macros.fat}g&nbsp; W:{" "}
            {meal.macros.carbs}g
          </Typography>
        </Box>
        <Stack spacing={1} alignItems="flex-end">
          <Button
            startIcon={<Edit />}
            size="small"
            variant="text"
            sx={{ textTransform: "none" }}
            onClick={() => onEdit?.(meal)}
          >
            Edytuj
          </Button>
          <Button
            size="small"
            variant="text"
            sx={{ textTransform: "none" }}
            onClick={() => onExpand?.(meal)}
          >
            Rozwiń
          </Button>
        </Stack>
      </Stack>
    </Box>
  );
}
