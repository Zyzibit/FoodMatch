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

export type ProductSource = "OpenFoodFacts" | "AI" | "User";

export interface RecipeIngredient {
  productId: number;
  productName: string;
  unitId: number;
  unitName: string;
  quantity: number;
  normalizedQuantityInGrams: number;
  source?: ProductSource;
  estimatedCalories?: number;
  estimatedProteins?: number;
  estimatedCarbohydrates?: number;
  estimatedFats?: number;
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
  isPublic?: boolean;
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
  code?: string;
  source?: ProductSource;
  calories?: number;
  proteins?: number;
  carbohydrates?: number;
  fats?: number;
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
  isPublic?: boolean;
  createdAt?: string;
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
  unitId: parseOptionalNumber(data?.unitId ?? data?.UnitId),
  unitName: data?.unitName ?? data?.UnitName,
  quantity: parseNumber(data?.quantity ?? data?.Quantity),
  normalizedQuantityInGrams: parseOptionalNumber(
    data?.normalizedQuantityInGrams ?? data?.NormalizedQuantityInGrams
  ),
  code: data?.code ?? data?.Code,
  source: data?.source ?? data?.Source,
  calories: parseOptionalNumber(data?.calories ?? data?.Calories),
  proteins: parseOptionalNumber(data?.proteins ?? data?.Proteins ?? data?.protein ?? data?.Protein),
  carbohydrates: parseOptionalNumber(
    data?.carbohydrates ?? data?.Carbohydrates
  ),
  fats: parseOptionalNumber(data?.fats ?? data?.Fats),
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
    ? (data?.ingredients ?? data?.Ingredients).map(parseRecipeDetailsIngredient)
    : [],
  additionalProducts: Array.isArray(
    data?.additionalProducts ?? data?.AdditionalProducts
  )
    ? (data?.additionalProducts ?? data?.AdditionalProducts)
    : [],
  isPublic: data?.isPublic ?? data?.IsPublic ?? false,
  createdAt: data?.createdAt ?? data?.CreatedAt,
});

const parseGeneratedRecipe = (data: any): GeneratedRecipe => ({
  title: data?.title ?? data?.Title ?? "",
  description: data?.description ?? data?.Description ?? "",
  instructions: data?.instructions ?? data?.Instructions ?? "",
  preparationTimeMinutes: parseNumber(
    data?.preparationTimeMinutes ?? data?.PreparationTimeMinutes
  ),
  totalWeightGrams: parseNumber(
    data?.totalWeightGrams ?? data?.TotalWeightGrams
  ),
  calories: parseNumber(data?.calories ?? data?.Calories),
  proteins: parseNumber(data?.proteins ?? data?.Proteins),
  carbohydrates: parseNumber(data?.carbohydrates ?? data?.Carbohydrates),
  fats: parseNumber(data?.fats ?? data?.Fats),
  ingredients: Array.isArray(data?.ingredients ?? data?.Ingredients)
    ? (data?.ingredients ?? data?.Ingredients).map((ing: any) => ({
        productId: parseNumber(ing?.productId ?? ing?.ProductId),
        productName: ing?.productName ?? ing?.ProductName ?? "",
        unitId: parseNumber(ing?.unitId ?? ing?.UnitId),
        unitName: ing?.unitName ?? ing?.UnitName ?? "",
        quantity: parseNumber(ing?.quantity ?? ing?.Quantity),
        normalizedQuantityInGrams: parseNumber(
          ing?.normalizedQuantityInGrams ?? ing?.NormalizedQuantityInGrams
        ),
        source: ing?.source ?? ing?.Source,
        estimatedCalories: parseNumber(
          ing?.estimatedCalories ?? ing?.EstimatedCalories ?? ing?.calories ?? ing?.Calories
        ),
        estimatedProteins: parseNumber(
          ing?.estimatedProteins ?? ing?.EstimatedProteins ?? ing?.protein ?? ing?.Protein
        ),
        estimatedCarbohydrates: parseNumber(
          ing?.estimatedCarbohydrates ?? ing?.EstimatedCarbohydrates ?? ing?.carbohydrates ?? ing?.Carbohydrates
        ),
        estimatedFats: parseNumber(ing?.estimatedFats ?? ing?.EstimatedFats ?? ing?.fats ?? ing?.Fats),
      }))
    : [],
  additionalProducts: Array.isArray(
    data?.additionalProducts ?? data?.AdditionalProducts
  )
    ? (data?.additionalProducts ?? data?.AdditionalProducts)
    : [],
});

export const generateRecipePreview = async (
  request: GenerateRecipeRequest
): Promise<GeneratedRecipe> => {
  const response = await fetch(`${API_BASE_URL}/recipes/generate-preview`, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include",
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Failed to generate recipe" }));
    throw new Error(error.message || "Failed to generate recipe");
  }

  const data = await response.json();
  return parseGeneratedRecipe(data.recipe);
};

export const saveGeneratedRecipe = async (
  request: SaveGeneratedRecipeRequest
): Promise<{ recipeId: string }> => {
  const response = await fetch(`${API_BASE_URL}/recipes/save-generated`, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include",
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

export const getRecipeById = async (
  recipeId: number
): Promise<RecipeDetails> => {
  const response = await fetch(`${API_BASE_URL}/recipes/${recipeId}`, {
    method: "GET",
    headers: getAuthHeaders(),
    credentials: "include",
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

export interface RecipeListResult {
  recipes: RecipeDetails[];
  totalCount: number;
  limit: number;
  offset: number;
  hasMore: boolean;
}

export const getUserRecipes = async (
  limit: number = 50,
  offset: number = 0
): Promise<RecipeListResult> => {
  const response = await fetch(
    `${API_BASE_URL}/recipes/me?limit=${limit}&offset=${offset}`,
    {
      method: "GET",
      headers: getAuthHeaders(),
      credentials: "include",
    }
  );

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się pobrać przepisów" }));
    throw new Error(error.message || "Nie udało się pobrać przepisów");
  }

  const data = await response.json();
  return {
    recipes: Array.isArray(data.recipes)
      ? data.recipes.map(parseRecipeDetails)
      : [],
    totalCount: data.totalCount || 0,
    limit: data.limit || limit,
    offset: data.offset || offset,
    hasMore: data.hasMore || false,
  };
};

export const getCommunityRecipes = async (
  limit: number = 50,
  offset: number = 0
): Promise<RecipeListResult> => {
  const response = await fetch(
    `${API_BASE_URL}/recipes/community?limit=${limit}&offset=${offset}`,
    {
      method: "GET",
      headers: getAuthHeaders(),
      credentials: "include",
    }
  );

  if (!response.ok) {
    const error = await response.json().catch(() => ({
      message: "Nie udało się pobrać przepisów społeczności",
    }));
    throw new Error(
      error.message || "Nie udało się pobrać przepisów społeczności"
    );
  }

  const data = await response.json();
  return {
    recipes: Array.isArray(data.recipes)
      ? data.recipes.map(parseRecipeDetails)
      : [],
    totalCount: data.totalCount || 0,
    limit: data.limit || limit,
    offset: data.offset || offset,
    hasMore: data.hasMore || false,
  };
};

export const copyRecipeToAccount = async (
  recipeId: number
): Promise<{ recipeId: string }> => {
  const response = await fetch(`${API_BASE_URL}/recipes/${recipeId}/copy`, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include",
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się skopiować przepisu" }));
    throw new Error(error.message || "Nie udało się skopiować przepisu");
  }

  const data = await response.json();
  return { recipeId: data.recipeId };
};

export const shareRecipe = async (recipeId: number): Promise<void> => {
  const response = await fetch(`${API_BASE_URL}/recipes/${recipeId}/share`, {
    method: "PATCH",
    headers: getAuthHeaders(),
    credentials: "include",
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się udostępnić przepisu" }));
    throw new Error(error.message || "Nie udało się udostępnić przepisu");
  }
};

export const deleteRecipe = async (recipeId: number): Promise<void> => {
  const response = await fetch(`${API_BASE_URL}/recipes/${recipeId}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
    credentials: "include",
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się usunąć przepisu" }));
    throw new Error(error.message || "Nie udało się usunąć przepisu");
  }
};

export interface CreateRecipeRequest {
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
  additionalProducts?: string[];
}

export const createRecipe = async (
  request: CreateRecipeRequest
): Promise<{ success: boolean; recipeId: number }> => {
  const response = await fetch(`${API_BASE_URL}/recipes`, {
    method: "POST",
    headers: {
      ...getAuthHeaders(),
      "Content-Type": "application/json",
    },
    credentials: "include",
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się utworzyć przepisu" }));
    throw new Error(error.message || "Nie udało się utworzyć przepisu");
  }

  return response.json();
};

export const searchRecipes = async (
  query: string,
  limit: number = 50,
  offset: number = 0
): Promise<RecipeListResult> => {
  const params = new URLSearchParams({
    searchTerm: query,
    limit: limit.toString(),
    offset: offset.toString(),
  });

  const response = await fetch(
    `${API_BASE_URL}/recipes/search?${params.toString()}`,
    {
      method: "GET",
      headers: getAuthHeaders(),
      credentials: "include",
    }
  );

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się wyszukać przepisów" }));
    throw new Error(error.message || "Nie udało się wyszukać przepisów");
  }

  return response.json();
};

