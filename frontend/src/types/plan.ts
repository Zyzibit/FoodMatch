export type ProductSource = "OpenFoodFacts" | "AI" | "User";

export type PlanMealProduct = {
  id: string;
  productId?: number;
  name: string;
  quantityLabel?: string;
  code?: string;
  source?: ProductSource;
  calories?: number;
  proteins?: number;
  carbohydrates?: number;
  fats?: number;
  normalizedQuantityInGrams?: number;
};

export type PlanMeal = {
  id: string;
  time: string;
  type: string;
  title: string;
  calories: number;
  description?: string;
  products?: PlanMealProduct[];
  instructions?: string;
  isDetailsLoading?: boolean;
  detailsError?: string | null;
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
