export type PlanMeal = {
  id: string;
  time: string;
  type: string;
  title: string;
  calories: number;
  macros: { protein: number; fat: number; carbs: number };
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
