import { API_BASE_URL } from "../config";
import { getAuthHeaders } from "./apiClient";

export interface MealPlanRecipeDto {
  id: number;
  title: string;
  description: string;
  calories: number;
  proteins: number;
  carbohydrates: number;
  fats: number;
  preparationTimeMinutes: number;
}

export interface MealPlanDto {
  id: number;
  name: string;
  date: string;
  recipe?: MealPlanRecipeDto;
}

export interface CreateMealPlanRequest {
  mealName: string;
  date: string;
  recipeId: number;
}

export interface CreateMealPlanResponse {
  mealPlanId: number;
}

const normalizeDateParam = (date: string): string => {
  if (!date) {
    return new Date().toISOString();
  }
  const normalized =
    date.includes("T") || date.includes("t") ? date : `${date}T00:00:00`;
  const parsed = new Date(normalized);
  if (Number.isNaN(parsed.getTime())) {
    return new Date().toISOString();
  }
  return parsed.toISOString();
};

const parseMealPlanRecipe = (data: any | null | undefined): MealPlanRecipeDto | undefined => {
  if (!data) return undefined;
  return {
    id: Number(data.id ?? data.Id ?? 0),
    title: data.title ?? data.Title ?? "",
    description: data.description ?? data.Description ?? "",
    calories: Number(data.calories ?? data.Calories ?? 0),
    proteins: Number(data.proteins ?? data.Proteins ?? 0),
    carbohydrates: Number(data.carbohydrates ?? data.Carbohydrates ?? 0),
    fats: Number(data.fats ?? data.Fats ?? 0),
    preparationTimeMinutes: Number(
      data.preparationTimeMinutes ?? data.PreparationTimeMinutes ?? 0
    ),
  };
};

const parseMealPlanDto = (data: any): MealPlanDto => ({
  id: Number(data.id ?? data.Id ?? 0),
  name: data.name ?? data.Name ?? "",
  date: data.date ?? data.Date ?? "",
  recipe: parseMealPlanRecipe(data.recipe ?? data.Recipe),
});

export const createMealPlan = async (
  request: CreateMealPlanRequest
): Promise<CreateMealPlanResponse> => {
  const response = await fetch(`${API_BASE_URL}/mealplans`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(request),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się dodać posiłku do planu" }));
    throw new Error(error.message || "Nie udało się dodać posiłku do planu");
  }

  const data = await response.json();
  return { mealPlanId: data.mealPlanId };
};

export const getMealPlansForDate = async (
  date: string
): Promise<MealPlanDto[]> => {
  const params = new URLSearchParams({
    date: normalizeDateParam(date),
  });

  const response = await fetch(`${API_BASE_URL}/mealplans?${params}`, {
    method: "GET",
    headers: getAuthHeaders(),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się pobrać planu posiłków" }));
    throw new Error(error.message || "Nie udało się pobrać planu posiłków");
  }

  const data = await response.json();
  const mealPlans = Array.isArray(data.mealPlans) ? data.mealPlans : data.MealPlans;

  if (!mealPlans || !Array.isArray(mealPlans)) {
    return [];
  }

  return mealPlans.map(parseMealPlanDto);
};
