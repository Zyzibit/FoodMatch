import { Stack } from "@mui/material";
import type { PlanMeal } from "../../types/plan";
import PlanMealCard from "./PlanMealCard";

type PlanMealListProps = {
  meals: PlanMeal[];
  onEditMeal?: (meal: PlanMeal) => void;
  onExpandMeal?: (meal: PlanMeal) => void;
};

export default function PlanMealList({
  meals,
  onEditMeal,
  onExpandMeal,
}: PlanMealListProps) {
  return (
    <Stack spacing={2}>
      {meals.map((meal) => (
        <PlanMealCard
          key={meal.id}
          meal={meal}
          onEdit={onEditMeal}
          onExpand={onExpandMeal}
        />
      ))}
    </Stack>
  );
}
