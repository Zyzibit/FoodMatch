import { API_BASE_URL } from "../config";
import { getAuthHeaders } from "./apiClient";

export interface DietaryPreferencesPayload {
  isVegetarian?: boolean;
  isVegan?: boolean;
  isGlutenFree?: boolean;
  isLactoseFree?: boolean;
  allergies?: string[];
  dislikedIngredients?: string[];
  dailyCalorieGoal?: number;
  dailyProteinGoal?: number;
  dailyCarbohydrateGoal?: number;
  dailyFatGoal?: number;
  mealType?: string;
  targetMealCalories?: number;
  targetMealProtein?: number;
  targetMealCarbohydrates?: number;
  targetMealFat?: number;
}

export interface GenerateRecipeRequest {
  productIds?: number[];
  availableIngredients?: string[];
  preferences?: DietaryPreferencesPayload;
  cuisineType?: string;
  maxPreparationTimeMinutes?: number;
  additionalInstructions?: string;
  mealType?: string;
}

export interface RecipeIngredient {
  productId: number;
  productName: string;
  unitId: number;
  unitName: string;
  quantity: number;
  normalizedQuantityInGrams: number;
}

export interface GeneratedRecipe {
  title: string;
  description: string;
  instructions: string;
  preparationTimeMinutes: number;
  totalWeightGrams: number;
  calories: number;
  proteins: number;
  carbohydrates: number;
  fats: number;
  ingredients: RecipeIngredient[];
  additionalProducts: string[];
}

export interface SaveGeneratedRecipeRequest {
  title: string;
  description: string;
  instructions: string;
  preparationTimeMinutes: number;
  totalWeightGrams: number;
  calories: number;
  proteins: number;
  carbohydrates: number;
  fats: number;
  ingredients: {
    productId: number;
    unitId: number;
    quantity: number;
    normalizedQuantityInGrams: number;
  }[];
  additionalProducts: string[];
}

export const generateRecipePreview = async (
  request: GenerateRecipeRequest
): Promise<GeneratedRecipe> => {
  const response = await fetch(`${API_BASE_URL}/recipes/generate-preview`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Failed to generate recipe" }));
    throw new Error(error.message || "Failed to generate recipe");
  }

  const data = await response.json();
  return data.recipe;
};

export const saveGeneratedRecipe = async (
  request: SaveGeneratedRecipeRequest
): Promise<{ recipeId: string }> => {
  const response = await fetch(`${API_BASE_URL}/recipes/save-generated`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Failed to save recipe" }));
    console.error("Backend error:", error);
    throw new Error(error.message || "Failed to save recipe");
  }

  const data = await response.json();
  return { recipeId: data.recipeId };
};
