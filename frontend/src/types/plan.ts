export type PlanMeal = {
  id: string;
  time: string;
  type: string;
  title: string;
  calories: number;
  description?: string;
  products?: string[];
  macros: { protein: number; fat: number; carbs: number };
  isPlaceholder?: boolean;
  mealPlanId?: number;
  recipeId?: number;
};

export type MacroEntry = {
  key: string;
  label: string;
  value: number;
  target: number;
};

export type MealPlanDay = {
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
