export type FoodPreferences = {
  isVegan: boolean;
  isVegetarian: boolean;
  hasGlutenIntolerance: boolean;
  hasLactoseIntolerance: boolean;
  allergies: string[];
};

export type UpdateFoodPreferencesRequest = {
  isVegan?: boolean;
  isVegetarian?: boolean;
  hasGlutenIntolerance?: boolean;
  hasLactoseIntolerance?: boolean;
  allergies?: string[];
};
