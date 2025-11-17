import { Stack } from "@mui/material";
import type { PlanMeal } from "../../types/plan";
import PlanMealCard from "./PlanMealCard";

type PlanMealListProps = {
  meals: PlanMeal[];
  onEditMeal?: (meal: PlanMeal) => void;
  onAddRecipe?: (meal: PlanMeal) => void;
  onExpandMeal?: (meal: PlanMeal) => void;
  expandedMealId?: string | null;
};

export default function PlanMealList({
  meals,
  onEditMeal,
  onAddRecipe,
  onExpandMeal,
  expandedMealId,
}: PlanMealListProps) {
  return (
    <Stack spacing={2}>
      {meals.map((meal) => (
        <PlanMealCard
          key={meal.id}
          meal={meal}
          onEdit={onEditMeal}
          onAddRecipe={onAddRecipe}
          onExpand={onExpandMeal}
          isExpanded={meal.id === expandedMealId}
        />
      ))}
    </Stack>
  );
}
