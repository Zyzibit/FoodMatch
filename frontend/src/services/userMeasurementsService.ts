import { API_BASE_URL } from "../config";

export interface UpdateFoodPreferencesRequest {
  age?: number;
  gender?: "Male" | "Female";
  weight?: number; // kg
  height?: number; // cm
  activityLevel?:
    | "Sedentary"
    | "LightlyActive"
    | "ModeratelyActive"
    | "VeryActive"
    | "ExtraActive";

  // Opcjonalne preferencje żywieniowe
  isVegan?: boolean;
  isVegetarian?: boolean;
  hasGlutenIntolerance?: boolean;
  hasLactoseIntolerance?: boolean;
  allergies?: string[];

  // Cele dzienne
  dailyProteinGoal?: number;
  dailyCarbohydrateGoal?: number;
  dailyFatGoal?: number;
  dailyCalorieGoal?: number;
}

export interface FoodPreferencesResponse {
  age?: number;
  gender?: string;
  weight?: number;
  height?: number;
  activityLevel?: string;

  isVegan?: boolean;
  isVegetarian?: boolean;
  hasGlutenIntolerance?: boolean;
  hasLactoseIntolerance?: boolean;
  allergies?: string[];

  dailyProteinGoal?: number;
  dailyCarbohydrateGoal?: number;
  dailyFatGoal?: number;
  dailyCalorieGoal?: number;

  // Obliczone wartości z backendu
  calculatedBMR?: number;
  calculatedDailyCalories?: number;
}

class UserMeasurementsService {
  private baseUrl: string;

  constructor() {
    this.baseUrl = API_BASE_URL;
  }

  async updatePreferences(
    request: UpdateFoodPreferencesRequest
  ): Promise<{ message: string }> {
    const response = await fetch(`${this.baseUrl}/users/preferences`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      credentials: "include",
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      const error = await response
        .json()
        .catch(() => ({ message: "Nie udało się zapisać pomiarów" }));
      throw new Error(error.message || "Nie udało się zapisać pomiarów");
    }

    return await response.json();
  }

  async getPreferences(): Promise<FoodPreferencesResponse> {
    const response = await fetch(`${this.baseUrl}/users/preferences`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
      credentials: "include",
    });

    if (!response.ok) {
      const error = await response
        .json()
        .catch(() => ({ message: "Nie udało się pobrać pomiarów" }));
      throw new Error(error.message || "Nie udało się pobrać pomiarów");
    }

    return await response.json();
  }

  async hasMeasurements(): Promise<boolean> {
    try {
      const prefs = await this.getPreferences();
      // Sprawdź czy użytkownik ma wypełnione podstawowe pomiary
      return !!(prefs.age && prefs.weight && prefs.height);
    } catch {
      return false;
    }
  }
}

export const userMeasurementsService = new UserMeasurementsService();
export default userMeasurementsService;
