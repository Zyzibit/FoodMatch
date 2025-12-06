export type ProductSource = "OpenFoodFacts" | "AI" | "User";

export type SavedRecipeIngredient = {
  name: string;
  productId?: number | string;
  source?: ProductSource;
};

export type SavedRecipe = {
  id: string;
  title: string;
  description: string;
  calories: number;
  macros: { protein: number; fat: number; carbs: number };
  tags?: string[];
  ingredients: SavedRecipeIngredient[];
  createdAt?: string;
  isPublic?: boolean;
};
