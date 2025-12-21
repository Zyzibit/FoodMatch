export type ProductSource = "OpenFoodFacts" | "AI" | "User";

export type SavedRecipeIngredient = {
  name: string;
  productId?: number | string;
  source?: ProductSource;
  isAdditional?: boolean;
  quantity?: number;
  unitName?: string;
  normalizedQuantityInGrams?: number;
  calories?: number;
  proteins?: number;
  carbohydrates?: number;
  fats?: number;
};

export type SavedRecipe = {
  id: string;
  title: string;
  description: string;
  instructions?: string;
  calories: number;
  macros: { protein: number; fat: number; carbs: number };
  tags?: string[];
  ingredients: SavedRecipeIngredient[];
  additionalProducts?: string[];
  createdAt?: string;
  isPublic?: boolean;
};
