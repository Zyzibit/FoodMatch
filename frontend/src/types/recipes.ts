export type SavedRecipe = {
  id: string;
  title: string;
  description: string;
  calories: number;
  macros: { protein: number; fat: number; carbs: number };
  tags?: string[];
  ingredients: string[];
  createdAt?: string;
};
