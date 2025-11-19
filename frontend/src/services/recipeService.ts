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

export interface RecipeDetailsIngredient {
  productId: number;
  productName: string;
  unitId?: number;
  unitName?: string;
  quantity: number;
  normalizedQuantityInGrams?: number;
}

export interface RecipeDetails {
  id: number;
  title: string;
  description: string;
  instructions: string;
  preparationTimeMinutes?: number;
  totalWeightGrams?: number;
  calories: number;
  proteins: number;
  carbohydrates: number;
  fats: number;
  ingredients: RecipeDetailsIngredient[];
  additionalProducts?: string[];
}

const parseNumber = (value: any): number => {
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : 0;
};

const parseOptionalNumber = (value: any): number | undefined => {
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : undefined;
};

const parseRecipeDetailsIngredient = (data: any): RecipeDetailsIngredient => ({
  productId: parseNumber(data?.productId ?? data?.ProductId),
  productName: data?.productName ?? data?.ProductName ?? "",
  unitId: data?.unitId ?? data?.UnitId,
  unitName: data?.unitName ?? data?.UnitName,
  quantity: parseNumber(data?.quantity ?? data?.Quantity),
  normalizedQuantityInGrams: parseOptionalNumber(
    data?.normalizedQuantityInGrams ?? data?.NormalizedQuantityInGrams
  ),
});

const parseRecipeDetails = (data: any): RecipeDetails => ({
  id: parseNumber(data?.id ?? data?.Id),
  title: data?.title ?? data?.Title ?? "",
  description: data?.description ?? data?.Description ?? "",
  instructions: data?.instructions ?? data?.Instructions ?? "",
  preparationTimeMinutes: parseOptionalNumber(
    data?.preparationTimeMinutes ?? data?.PreparationTimeMinutes
  ),
  totalWeightGrams: parseOptionalNumber(
    data?.totalWeightGrams ?? data?.TotalWeightGrams
  ),
  calories: parseNumber(data?.calories ?? data?.Calories),
  proteins: parseNumber(data?.proteins ?? data?.Proteins),
  carbohydrates: parseNumber(data?.carbohydrates ?? data?.Carbohydrates),
  fats: parseNumber(data?.fats ?? data?.Fats),
  ingredients: Array.isArray(data?.ingredients ?? data?.Ingredients)
    ? (data?.ingredients ?? data?.Ingredients).map(
        parseRecipeDetailsIngredient
      )
    : [],
  additionalProducts: Array.isArray(
    data?.additionalProducts ?? data?.AdditionalProducts
  )
    ? data?.additionalProducts ?? data?.AdditionalProducts
    : [],
});

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

export const getRecipeById = async (recipeId: number): Promise<RecipeDetails> => {
  const response = await fetch(`${API_BASE_URL}/recipes/${recipeId}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się pobrać przepisu" }));
    throw new Error(error.message || "Nie udało się pobrać przepisu");
  }

  const data = await response.json();
  return parseRecipeDetails(data);
};
