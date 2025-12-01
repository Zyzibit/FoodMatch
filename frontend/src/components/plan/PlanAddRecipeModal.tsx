import { Add, AutoAwesome, Close, Delete, MenuBook } from "@mui/icons-material";
import {
  Alert,
  Autocomplete,
  Box,
  Button,
  Checkbox,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  FormControlLabel,
  IconButton,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import {
  useCallback,
  useEffect,
  useState,
  useMemo,
  type ReactNode,
} from "react";
import { allergenOptions } from "../../constants/allergens";
import type { PlanMeal } from "../../types/plan";
import {
  generateRecipePreview,
  saveGeneratedRecipe,
  getUserRecipes,
  type GeneratedRecipe,
  type GenerateRecipeRequest,
  type RecipeDetails,
} from "../../services/recipeService";
import { searchProducts, type ProductDto } from "../../services/productService";
import { createMealPlan } from "../../services/mealPlanService";
import userMeasurementsService from "../../services/userMeasurementsService";

type AddMode = "recipes" | "ai";

export type RecipeAddedPayload = {
  meal: PlanMeal;
  recipeId: number;
  mealPlanId: number;
  recipe: GeneratedRecipe;
};

type PlanAddRecipeModalProps = {
  open: boolean;
  onClose: () => void;
  meal?: PlanMeal | null;
  planDate: string;
  onRecipeAdded?: (payload: RecipeAddedPayload) => void;
};

type AiProduct = {
  id: string;
  name: string;
  productId?: number;
};

interface FoodPreferences {
  isVegan: boolean;
  isVegetarian: boolean;
  hasGlutenIntolerance: boolean;
  hasLactoseIntolerance: boolean;
  allergies: string[];
}

const cuisineOptions = [
  "Polska",
  "Włoska",
  "Azjatycka",
  "Meksykańska",
  "Francuska",
  "Grecka",
  "Indyjska",
  "Japońska",
  "Hiszpańska",
  "Amerykańska",
];

const modeOptions: {
  key: AddMode;
  label: string;
  icon: ReactNode;
}[] = [
  {
    key: "recipes",
    label: "Wybierz z przepisów",
    icon: <MenuBook fontSize="small" />,
  },
  {
    key: "ai",
    label: "Wygeneruj z AI",
    icon: <AutoAwesome fontSize="small" />,
  },
];

const MODAL_BODY_HEIGHT = 600;

const mealTypeMap: Record<string, string> = {
  Śniadanie: "breakfast",
  "Drugie śniadanie": "snack",
  Obiad: "lunch",
  Kolacja: "dinner",
};

const mealPlanNameMap: Record<string, string> = {
  Śniadanie: "Breakfast",
  "Drugie śniadanie": "Snack",
  Obiad: "Lunch",
  Kolacja: "Dinner",
};

export default function PlanAddRecipeModal({
  open,
  onClose,
  meal,
  planDate,
  onRecipeAdded,
}: PlanAddRecipeModalProps) {
  const [mode, setMode] = useState<AddMode>("recipes");
  const [selectedRecipe, setSelectedRecipe] = useState<number | null>(null);
  const [userRecipes, setUserRecipes] = useState<RecipeDetails[]>([]);
  const [isLoadingRecipes, setIsLoadingRecipes] = useState(false);
  const [recipesError, setRecipesError] = useState<string | null>(null);
  const [productInputValue, setProductInputValue] = useState("");
  const [selectedProductOption, setSelectedProductOption] =
    useState<ProductDto | null>(null);
  const [products, setProducts] = useState<AiProduct[]>([]);
  const [preferences, setPreferences] = useState<FoodPreferences>({
    isVegan: false,
    isVegetarian: false,
    hasGlutenIntolerance: false,
    hasLactoseIntolerance: false,
    allergies: [],
  });
  const [customAllergen, setCustomAllergen] = useState("");
  const [cuisine, setCuisine] = useState<string>("");
  const [preparationTime, setPreparationTime] = useState<string>("");
  const [servings, setServings] = useState<string>("");
  const [productSuggestions, setProductSuggestions] = useState<ProductDto[]>(
    []
  );
  const [isLoadingProducts, setIsLoadingProducts] = useState(false);
  const [generatedRecipe, setGeneratedRecipe] =
    useState<GeneratedRecipe | null>(null);
  const [isGeneratingRecipe, setIsGeneratingRecipe] = useState(false);
  const [generationError, setGenerationError] = useState<string | null>(null);
  const [savedRecipeId, setSavedRecipeId] = useState<number | null>(null);
  const [isSavingRecipe, setIsSavingRecipe] = useState(false);
  const [saveRecipeError, setSaveRecipeError] = useState<string | null>(null);
  const [isAddingToPlan, setIsAddingToPlan] = useState(false);
  const [addToPlanError, setAddToPlanError] = useState<string | null>(null);

  const handleSearchProducts = useCallback(async (query: string) => {
    if (!query || query.trim().length < 2) {
      setProductSuggestions([]);
      return;
    }

    setIsLoadingProducts(true);
    try {
      const results = await searchProducts(query, 10);
      setProductSuggestions(results);
    } catch (error) {
      console.error("Error searching products:", error);
      setProductSuggestions([]);
    } finally {
      setIsLoadingProducts(false);
    }
  }, []);

  useEffect(() => {
    const timeoutId = setTimeout(() => {
      handleSearchProducts(productInputValue);
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [productInputValue, handleSearchProducts]);

  useEffect(() => {
    if (!open) {
      setMode("recipes");
      setSelectedRecipe(null);
      setUserRecipes([]);
      setIsLoadingRecipes(false);
      setRecipesError(null);
      setProductInputValue("");
      setSelectedProductOption(null);
      setProducts([]);
      setCustomAllergen("");
      setCuisine("");
      setPreparationTime("");
      setServings("");
      setGeneratedRecipe(null);
      setGenerationError(null);
      setIsGeneratingRecipe(false);
      setSavedRecipeId(null);
      setIsSavingRecipe(false);
      setSaveRecipeError(null);
      setIsAddingToPlan(false);
      setAddToPlanError(null);
      setPreferences({
        isVegan: false,
        isVegetarian: false,
        hasGlutenIntolerance: false,
        hasLactoseIntolerance: false,
        allergies: [],
      });
      setProductSuggestions([]);
    } else {
      // Przy otwieraniu modala ładujemy preferencje i przepisy użytkownika
      const loadData = async () => {
        try {
          const prefs = await userMeasurementsService.getPreferences();
          setPreferences({
            isVegan: prefs.isVegan || false,
            isVegetarian: prefs.isVegetarian || false,
            hasGlutenIntolerance: prefs.hasGlutenIntolerance || false,
            hasLactoseIntolerance: prefs.hasLactoseIntolerance || false,
            allergies: prefs.allergies || [],
          });
        } catch (error) {
          console.error("Failed to load preferences:", error);
        }

        setIsLoadingRecipes(true);
        setRecipesError(null);
        try {
          const result = await getUserRecipes();
          setUserRecipes(result.recipes);
        } catch (error) {
          console.error("Failed to load user recipes:", error);
          setRecipesError(
            error instanceof Error
              ? error.message
              : "Nie udało się pobrać przepisów"
          );
        } finally {
          setIsLoadingRecipes(false);
        }
      };
      loadData();
    }
  }, [open]);

  const canAddProduct = productInputValue.trim().length > 0;

  const handleAddProduct = () => {
    if (!canAddProduct) return;

    const trimmedName = productInputValue.trim();
    const matchedOption = selectedProductOption;

    let productId: number | undefined;
    if (matchedOption) {
      if (typeof matchedOption.productId === "number") {
        productId = matchedOption.productId;
      } else if (
        typeof matchedOption.id === "number" &&
        Number.isFinite(matchedOption.id)
      ) {
        productId = matchedOption.id;
      } else if (
        typeof matchedOption.id === "string" &&
        matchedOption.id.trim().length > 0
      ) {
        const parsedId = Number.parseInt(matchedOption.id, 10);
        if (!Number.isNaN(parsedId)) {
          productId = parsedId;
        }
      }
    }

    const resolvedName =
      matchedOption?.name ?? matchedOption?.brand ?? trimmedName;

    setProducts((prev) => [
      ...prev,
      {
        id: crypto.randomUUID(),
        name: resolvedName,
        productId,
      },
    ]);
    setProductInputValue("");
    setSelectedProductOption(null);
  };

  const handleRemoveProduct = (id: string) => {
    setProducts((prev) => prev.filter((item) => item.id !== id));
  };

  const toggleAllergen = (name: string) => {
    setPreferences((prev) => ({
      ...prev,
      allergies: prev.allergies.includes(name)
        ? prev.allergies.filter((item) => item !== name)
        : [...prev.allergies, name],
    }));
  };

  const handleAddCustomAllergen = () => {
    const trimmed = customAllergen.trim();
    if (!trimmed) return;
    const normalized = trimmed
      .toLowerCase()
      .replace(/^\w/, (c) => c.toUpperCase());
    if (!preferences.allergies.includes(normalized)) {
      setPreferences((prev) => ({
        ...prev,
        allergies: [...prev.allergies, normalized],
      }));
    }
    setCustomAllergen("");
  };

  const customAllergens = useMemo(
    () =>
      preferences.allergies.filter(
        (name) => !allergenOptions.some((base) => base === name)
      ),
    [preferences.allergies]
  );

  const normalizedMealType = useMemo(() => {
    if (!meal?.type) return undefined;
    const trimmed = meal.type.trim();
    if (!trimmed) return undefined;
    if (trimmed in mealTypeMap) {
      return mealTypeMap[trimmed as keyof typeof mealTypeMap];
    }
    return trimmed.toLowerCase();
  }, [meal?.type]);

  const normalizedMealPlanName = useMemo(() => {
    if (!meal?.type) return undefined;
    const trimmed = meal.type.trim();
    if (!trimmed) return undefined;
    if (trimmed in mealPlanNameMap) {
      return mealPlanNameMap[trimmed as keyof typeof mealPlanNameMap];
    }
    return trimmed;
  }, [meal?.type]);

  const persistGeneratedRecipe = useCallback(
    async (recipe: GeneratedRecipe) => {
      setIsSavingRecipe(true);
      setSaveRecipeError(null);
      setSavedRecipeId(null);

      try {
        const payload = {
          title: recipe.title,
          description: recipe.description,
          instructions: recipe.instructions,
          preparationTimeMinutes: recipe.preparationTimeMinutes,
          totalWeightGrams: recipe.totalWeightGrams,
          calories: recipe.calories,
          proteins: recipe.proteins,
          carbohydrates: recipe.carbohydrates,
          fats: recipe.fats,
          ingredients: recipe.ingredients.map((ingredient) => ({
            productId: ingredient.productId,
            unitId: ingredient.unitId,
            quantity: ingredient.quantity,
            normalizedQuantityInGrams: ingredient.normalizedQuantityInGrams,
          })),
          additionalProducts: recipe.additionalProducts,
        };

        const { recipeId } = await saveGeneratedRecipe(payload);
        const numericRecipeId = Number(recipeId);
        if (Number.isNaN(numericRecipeId)) {
          throw new Error("Nieprawidłowy identyfikator przepisu");
        }
        setSavedRecipeId(numericRecipeId);
        return numericRecipeId;
      } catch (error) {
        setSavedRecipeId(null);
        setSaveRecipeError(
          error instanceof Error
            ? error.message
            : "Nie udało się zapisać przepisu."
        );
        return null;
      } finally {
        setIsSavingRecipe(false);
      }
    },
    []
  );

  const handleRetrySaveRecipe = useCallback(() => {
    if (generatedRecipe) {
      void persistGeneratedRecipe(generatedRecipe);
    }
  }, [generatedRecipe, persistGeneratedRecipe]);

  const planDateIso = useMemo(() => {
    if (!planDate) {
      return new Date().toISOString();
    }

    const normalized =
      planDate.includes("T") || planDate.includes("t")
        ? planDate
        : `${planDate}T00:00:00`;
    const parsed = new Date(normalized);
    if (Number.isNaN(parsed.getTime())) {
      return new Date().toISOString();
    }
    return parsed.toISOString();
  }, [planDate]);

  const handleGenerateRecipe = async () => {
    setGenerationError(null);
    setIsGeneratingRecipe(true);
    setGeneratedRecipe(null);
    setSavedRecipeId(null);
    setSaveRecipeError(null);
    setAddToPlanError(null);

    try {
      const productIds = products
        .map((product) => product.productId)
        .filter((id): id is number => typeof id === "number");

      const availableIngredients = products
        .map((product) => product.name.trim())
        .filter((name) => name.length > 0);

      const normalizedCuisine =
        cuisine.trim().length > 0 ? cuisine.trim() : undefined;
      const parsedPreparationTime = Number.parseInt(preparationTime, 10);
      const maxPreparationTime = Number.isNaN(parsedPreparationTime)
        ? undefined
        : parsedPreparationTime;

      const parsedServings = Number.parseInt(servings, 10);
      const additionalInstructionsParts: string[] = [];
      if (!Number.isNaN(parsedServings) && parsedServings > 0) {
        additionalInstructionsParts.push(
          `Przygotuj przepis dla ${parsedServings} porcji.`
        );
      }

      const requestPayload: GenerateRecipeRequest = {
        productIds,
        availableIngredients,
        cuisineType: normalizedCuisine,
        maxPreparationTimeMinutes: maxPreparationTime,
        additionalInstructions:
          additionalInstructionsParts.length > 0
            ? additionalInstructionsParts.join(" ")
            : undefined,
        mealType: normalizedMealType,
        preferences: {
          isVegetarian: preferences.isVegetarian,
          isVegan: preferences.isVegan,
          isGlutenFree: preferences.hasGlutenIntolerance,
          isLactoseFree: preferences.hasLactoseIntolerance,
          allergies: preferences.allergies,
          dislikedIngredients: [],
          mealType: normalizedMealType,
        },
      };

      const recipe = await generateRecipePreview(requestPayload);
      setGeneratedRecipe(recipe);
      await persistGeneratedRecipe(recipe);
    } catch (error) {
      console.error("Error generating recipe preview:", error);
      setGenerationError(
        error instanceof Error
          ? error.message
          : "Nie udało się wygenerować przepisu."
      );
    } finally {
      setIsGeneratingRecipe(false);
    }
  };

  const formatQuantity = (value: number) => {
    if (typeof value !== "number" || Number.isNaN(value)) {
      return "-";
    }
    return Number.isInteger(value) ? value.toString() : value.toFixed(1);
  };

  const handleAddToPlan = async () => {
    if (
      !generatedRecipe ||
      !savedRecipeId ||
      !meal ||
      !normalizedMealPlanName
    ) {
      return;
    }

    setAddToPlanError(null);
    setIsAddingToPlan(true);

    try {
      const { mealPlanId } = await createMealPlan({
        mealName: normalizedMealPlanName,
        date: planDateIso,
        recipeId: savedRecipeId,
      });

      onRecipeAdded?.({
        meal,
        recipe: generatedRecipe,
        recipeId: savedRecipeId,
        mealPlanId,
      });
      onClose();
    } catch (error) {
      console.error("Error adding recipe to plan:", error);
      setAddToPlanError(
        error instanceof Error
          ? error.message
          : "Nie udało się dodać przepisu do planu."
      );
    } finally {
      setIsAddingToPlan(false);
    }
  };

  const handleAddSelectedRecipeToPlan = async () => {
    if (!selectedRecipe || !meal || !normalizedMealPlanName) {
      return;
    }

    setAddToPlanError(null);
    setIsAddingToPlan(true);

    try {
      const { mealPlanId } = await createMealPlan({
        mealName: normalizedMealPlanName,
        date: planDateIso,
        recipeId: selectedRecipe,
      });

      const recipe = userRecipes.find((r) => r.id === selectedRecipe);
      if (recipe) {
        onRecipeAdded?.({
          meal,
          recipe: {
            title: recipe.title,
            description: recipe.description,
            instructions: recipe.instructions,
            preparationTimeMinutes: recipe.preparationTimeMinutes || 0,
            totalWeightGrams: recipe.totalWeightGrams || 0,
            calories: recipe.calories,
            proteins: recipe.proteins,
            carbohydrates: recipe.carbohydrates,
            fats: recipe.fats,
            ingredients: recipe.ingredients.map((ing) => ({
              productId: ing.productId,
              productName: ing.productName,
              unitId: ing.unitId || 0,
              unitName: ing.unitName || "",
              quantity: ing.quantity,
              normalizedQuantityInGrams: ing.normalizedQuantityInGrams || 0,
            })),
            additionalProducts: recipe.additionalProducts || [],
          },
          recipeId: selectedRecipe,
          mealPlanId,
        });
      }
      onClose();
    } catch (error) {
      console.error("Error adding recipe to plan:", error);
      setAddToPlanError(
        error instanceof Error
          ? error.message
          : "Nie udało się dodać przepisu do planu."
      );
    } finally {
      setIsAddingToPlan(false);
    }
  };

  const renderRecipePicker = () => (
    <Stack spacing={1.5}>
      {isLoadingRecipes && (
        <Stack alignItems="center" py={4}>
          <CircularProgress />
          <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
            Ładowanie przepisów...
          </Typography>
        </Stack>
      )}

      {recipesError && <Alert severity="error">{recipesError}</Alert>}

      {!isLoadingRecipes && !recipesError && userRecipes.length === 0 && (
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Typography variant="body2" color="text.secondary">
            Nie masz jeszcze zapisanych przepisów. Wygeneruj przepis z AI, a
            zostanie automatycznie zapisany.
          </Typography>
        </Paper>
      )}

      {!isLoadingRecipes &&
        !recipesError &&
        userRecipes.length > 0 &&
        userRecipes.map((recipe) => {
          const isActive = selectedRecipe === recipe.id;
          return (
            <Paper
              key={recipe.id}
              variant={isActive ? "outlined" : "elevation"}
              onClick={() => setSelectedRecipe(recipe.id)}
              sx={{
                p: 2,
                borderColor: (theme) =>
                  isActive ? theme.palette.secondary.main : undefined,
                cursor: "pointer",
                transition: "border-color 0.2s ease",
              }}
            >
              <Stack
                direction={{ xs: "column", sm: "row" }}
                justifyContent="space-between"
                spacing={1.5}
              >
                <Box>
                  <Typography variant="subtitle1" fontWeight={700}>
                    {recipe.title}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" mb={1}>
                    {recipe.description}
                  </Typography>
                  <Stack direction="row" spacing={1} flexWrap="wrap">
                    {recipe.preparationTimeMinutes && (
                      <Chip
                        label={`${recipe.preparationTimeMinutes} min`}
                        size="small"
                      />
                    )}
                    {recipe.totalWeightGrams && (
                      <Chip
                        label={`${recipe.totalWeightGrams} g`}
                        size="small"
                      />
                    )}
                  </Stack>
                </Box>
                <Box textAlign={{ xs: "left", sm: "right" }}>
                  <Typography variant="subtitle2" color="text.secondary">
                    {Math.round(recipe.calories)} kcal
                  </Typography>
                  <Typography variant="body2">
                    B: {Math.round(recipe.proteins)} g
                  </Typography>
                  <Typography variant="body2">
                    T: {Math.round(recipe.fats)} g
                  </Typography>
                  <Typography variant="body2">
                    W: {Math.round(recipe.carbohydrates)} g
                  </Typography>
                </Box>
              </Stack>
            </Paper>
          );
        })}
    </Stack>
  );

  const renderAiBuilder = () => {
    const macroItems = generatedRecipe
      ? [
          {
            label: "Kalorie",
            value: `${Math.round(generatedRecipe.calories)} kcal`,
          },
          {
            label: "Białko",
            value: `${Math.round(generatedRecipe.proteins)} g`,
          },
          {
            label: "Węglowodany",
            value: `${Math.round(generatedRecipe.carbohydrates)} g`,
          },
          {
            label: "Tłuszcze",
            value: `${Math.round(generatedRecipe.fats)} g`,
          },
        ]
      : [];

    return (
      <Stack spacing={2}>
        {/* Parametry przepisu */}
        <Box>
          <Typography variant="subtitle2" fontWeight={700} gutterBottom>
            Parametry przepisu
          </Typography>
          <Stack spacing={2}>
            <Autocomplete
              options={cuisineOptions}
              value={cuisine}
              onChange={(_, newValue) => setCuisine(newValue || "")}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Kuchnia"
                  placeholder="Wybierz kuchnię"
                />
              )}
              sx={{ flex: 1 }}
            />
            <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
              <TextField
                label="Czas wykonania (minuty)"
                type="number"
                value={preparationTime}
                onChange={(e) => setPreparationTime(e.target.value)}
                placeholder="np. 30"
                inputProps={{ min: 1 }}
                sx={{ flex: 1 }}
              />
              <TextField
                label="Liczba porcji"
                type="number"
                value={servings}
                onChange={(e) => setServings(e.target.value)}
                placeholder="np. 4"
                inputProps={{ min: 1 }}
                sx={{ flex: 1 }}
              />
            </Stack>
          </Stack>
        </Box>

        <Divider />

        {/* Składniki */}
        <Box>
          <Typography variant="subtitle2" fontWeight={700} gutterBottom>
            Składniki
          </Typography>
          <Stack
            direction={{ xs: "column", md: "row" }}
            spacing={1.5}
            alignItems={{ md: "flex-end" }}
          >
            <Autocomplete
              freeSolo
              options={productSuggestions}
              value={selectedProductOption}
              inputValue={productInputValue}
              getOptionLabel={(option) =>
                typeof option === "string"
                  ? option
                  : (option.name ?? option.brand ?? "")
              }
              onInputChange={(_, value, reason) => {
                setProductInputValue(value);
                if (reason === "input" || reason === "clear") {
                  setSelectedProductOption(null);
                }
              }}
              onChange={(_, value) => {
                if (typeof value === "string" || value === null) {
                  setSelectedProductOption(null);
                  if (typeof value === "string") {
                    setProductInputValue(value);
                  }
                } else {
                  setSelectedProductOption(value);
                  setProductInputValue(value.name ?? value.brand ?? "");
                }
              }}
              loading={isLoadingProducts}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Składnik"
                  placeholder="np. banan"
                />
              )}
              renderOption={(props, option) => {
                const optionName =
                  typeof option === "string"
                    ? option
                    : (option.name ?? option.brand ?? "");
                const optionBrand =
                  typeof option !== "string" ? option.brand : undefined;
                return (
                  <li {...props}>
                    <Box>
                      <Typography variant="body2">{optionName}</Typography>
                      {optionBrand && (
                        <Typography variant="caption" color="text.secondary">
                          {optionBrand}
                        </Typography>
                      )}
                    </Box>
                  </li>
                );
              }}
              sx={{ flex: 2 }}
            />
            <Button
              variant="contained"
              startIcon={<Add />}
              disabled={!canAddProduct}
              onClick={handleAddProduct}
              sx={{ whiteSpace: "nowrap", textTransform: "none" }}
            >
              Dodaj
            </Button>
          </Stack>
        </Box>

        <Paper
          variant="outlined"
          sx={{ maxHeight: 220, overflowY: "auto", p: 2 }}
        >
          {products.length === 0 ? (
            <Typography variant="body2" color="text.secondary">
              Dodaj składniki, które AI ma wykorzystać w przepisie.
            </Typography>
          ) : (
            products.map((product) => (
              <Box
                key={product.id}
                sx={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
                  py: 1,
                }}
              >
                <Box>
                  <Typography fontWeight={600}>{product.name}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {product.productId
                      ? "Produkt z bazy FoodMatch"
                      : "Własny składnik"}
                  </Typography>
                </Box>
                <IconButton onClick={() => handleRemoveProduct(product.id)}>
                  <Delete />
                </IconButton>
              </Box>
            ))
          )}
        </Paper>

        <Divider />

        {/* Preferencje żywieniowe */}
        <Box>
          <Typography variant="subtitle2" fontWeight={700} gutterBottom>
            Preferencje żywieniowe
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
            AI uwzględni wybrane preferencje przy generowaniu przepisu
          </Typography>
          <Stack spacing={1}>
            <FormControlLabel
              control={
                <Checkbox
                  checked={preferences.isVegan}
                  onChange={(e) =>
                    setPreferences((prev) => ({
                      ...prev,
                      isVegan: e.target.checked,
                    }))
                  }
                />
              }
              label="Dieta wegańska"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={preferences.isVegetarian}
                  onChange={(e) =>
                    setPreferences((prev) => ({
                      ...prev,
                      isVegetarian: e.target.checked,
                    }))
                  }
                />
              }
              label="Dieta wegetariańska"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={preferences.hasGlutenIntolerance}
                  onChange={(e) =>
                    setPreferences((prev) => ({
                      ...prev,
                      hasGlutenIntolerance: e.target.checked,
                    }))
                  }
                />
              }
              label="Nietolerancja glutenu"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={preferences.hasLactoseIntolerance}
                  onChange={(e) =>
                    setPreferences((prev) => ({
                      ...prev,
                      hasLactoseIntolerance: e.target.checked,
                    }))
                  }
                />
              }
              label="Nietolerancja laktozy"
            />
          </Stack>
        </Box>

        <Divider />

        <Box>
          <Typography variant="subtitle2" fontWeight={700} gutterBottom>
            Alergeny do pominięcia
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
            Wybierz alergeny, których AI ma unikać w przepisie
          </Typography>
          <Stack direction="row" flexWrap="wrap" gap={1}>
            {allergenOptions.map((name) => {
              const active = preferences.allergies.includes(name);
              return (
                <Chip
                  key={name}
                  label={name}
                  color={active ? "secondary" : "default"}
                  variant={active ? "filled" : "outlined"}
                  onClick={() => toggleAllergen(name)}
                />
              );
            })}
          </Stack>
        </Box>

        {/* Dodatkowy alergen */}
        {mode === "ai" && (
          <>
            <Box>
              <Typography variant="subtitle2" fontWeight={600} gutterBottom>
                Dodatkowy alergen
              </Typography>
              <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
                <TextField
                  label="Nazwa"
                  value={customAllergen}
                  onChange={(e) => setCustomAllergen(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") {
                      e.preventDefault();
                      handleAddCustomAllergen();
                    }
                  }}
                  size="small"
                  sx={{ flex: 1 }}
                />
                <Button
                  variant="contained"
                  onClick={handleAddCustomAllergen}
                  disabled={!customAllergen.trim()}
                  sx={{ textTransform: "none" }}
                >
                  Dodaj
                </Button>
              </Stack>
            </Box>

            {preferences.allergies.length > 0 && (
              <Box>
                <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                  Wybrane alergeny
                </Typography>
                <Stack direction="row" flexWrap="wrap" gap={1}>
                  {preferences.allergies.map((name) => (
                    <Chip
                      key={name}
                      label={name}
                      onDelete={() => toggleAllergen(name)}
                      color="secondary"
                      sx={{ textTransform: "capitalize" }}
                    />
                  ))}
                </Stack>
                {customAllergens.length > 0 && (
                  <Typography
                    variant="caption"
                    color="text.secondary"
                    sx={{ mt: 1, display: "block" }}
                  >
                    Własne alergeny: {customAllergens.join(", ")}
                  </Typography>
                )}
              </Box>
            )}
          </>
        )}

        <Divider />

        <Stack
          direction={{ xs: "column", md: "row" }}
          spacing={2}
          alignItems={{ md: "center" }}
          justifyContent="space-between"
        >
          <Typography variant="body2" color="text.secondary">
            AI wygeneruje propozycję przepisu na podstawie powyższych danych
          </Typography>
          <Button
            variant="contained"
            startIcon={
              isGeneratingRecipe ? (
                <CircularProgress size={16} color="inherit" />
              ) : (
                <AutoAwesome fontSize="small" />
              )
            }
            disabled={isGeneratingRecipe}
            onClick={handleGenerateRecipe}
            sx={{ textTransform: "none" }}
          >
            {isGeneratingRecipe ? "Generowanie..." : "Wygeneruj przepis"}
          </Button>
        </Stack>

        {generationError && <Alert severity="error">{generationError}</Alert>}
        {isSavingRecipe && (
          <Alert severity="info">Zapisywanie przepisu w bazie...</Alert>
        )}
        {saveRecipeError && (
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                size="small"
                onClick={handleRetrySaveRecipe}
              >
                Spróbuj ponownie
              </Button>
            }
          >
            {saveRecipeError}
          </Alert>
        )}
        {savedRecipeId && !isSavingRecipe && !saveRecipeError && (
          <Alert severity="success">
            Przepis został automatycznie zapisany w społeczności i dodany do
            Twoich przepisów (ID: {savedRecipeId})
          </Alert>
        )}
        {addToPlanError && <Alert severity="error">{addToPlanError}</Alert>}

        {generatedRecipe && (
          <Paper variant="outlined" sx={{ p: 2 }}>
            <Stack spacing={2}>
              <Box>
                <Typography variant="h6" fontWeight={700}>
                  {generatedRecipe.title}
                </Typography>
                {generatedRecipe.description && (
                  <Typography variant="body2" color="text.secondary">
                    {generatedRecipe.description}
                  </Typography>
                )}
              </Box>

              <Stack direction="row" spacing={1} flexWrap="wrap">
                {generatedRecipe.preparationTimeMinutes > 0 && (
                  <Chip
                    label={`${generatedRecipe.preparationTimeMinutes} min przygotowania`}
                    size="small"
                  />
                )}
                {generatedRecipe.totalWeightGrams > 0 && (
                  <Chip
                    label={`${generatedRecipe.totalWeightGrams} g`}
                    size="small"
                  />
                )}
              </Stack>

              <Stack
                direction={{ xs: "column", sm: "row" }}
                spacing={2}
                flexWrap="wrap"
              >
                {macroItems.map((item) => (
                  <Box key={item.label} sx={{ minWidth: 120 }}>
                    <Typography variant="caption" color="text.secondary">
                      {item.label}
                    </Typography>
                    <Typography variant="h6">{item.value}</Typography>
                  </Box>
                ))}
              </Stack>

              <Divider />

              <Box>
                <Typography variant="subtitle2" fontWeight={700}>
                  Składniki
                </Typography>
                {generatedRecipe.ingredients.length === 0 ? (
                  <Typography variant="body2" color="text.secondary">
                    AI nie zwróciło szczegółowych składników.
                  </Typography>
                ) : (
                  <Stack spacing={1} sx={{ mt: 1 }}>
                    {generatedRecipe.ingredients.map((ingredient) => (
                      <Box
                        key={`${ingredient.productId}-${ingredient.productName}-${ingredient.unitId}`}
                        sx={{
                          display: "flex",
                          justifyContent: "space-between",
                          gap: 2,
                          borderBottom: (theme) =>
                            `1px solid ${theme.palette.divider}`,
                          pb: 1,
                        }}
                      >
                        <Box>
                          <Typography fontWeight={600}>
                            {ingredient.productName}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            {formatQuantity(ingredient.quantity)}{" "}
                            {ingredient.unitName}
                          </Typography>
                        </Box>
                        <Typography variant="body2" color="text.secondary">
                          {Math.round(ingredient.normalizedQuantityInGrams)} g
                        </Typography>
                      </Box>
                    ))}
                  </Stack>
                )}
              </Box>

              <Divider />

              <Box>
                <Typography variant="subtitle2" fontWeight={700}>
                  Instrukcje
                </Typography>
                <Typography
                  variant="body2"
                  whiteSpace="pre-line"
                  sx={{ mt: 1 }}
                >
                  {generatedRecipe.instructions}
                </Typography>
              </Box>

              {generatedRecipe.additionalProducts?.length ? (
                <>
                  <Divider />
                  <Box>
                    <Typography variant="subtitle2" fontWeight={700}>
                      Dodatkowe produkty
                    </Typography>
                    <Stack
                      direction="row"
                      flexWrap="wrap"
                      gap={1}
                      sx={{ mt: 1 }}
                    >
                      {generatedRecipe.additionalProducts.map((item) => (
                        <Chip key={item} label={item} size="small" />
                      ))}
                    </Stack>
                  </Box>
                </>
              ) : null}
            </Stack>
          </Paper>
        )}

        {/* Dodatkowe miejsce na dole */}
        <Box sx={{ height: 40 }} />
      </Stack>
    );
  };

  const canAddToPlan =
    mode === "ai"
      ? Boolean(
          generatedRecipe &&
            savedRecipeId &&
            !isGeneratingRecipe &&
            !isSavingRecipe &&
            meal &&
            normalizedMealPlanName
        )
      : Boolean(selectedRecipe && meal && normalizedMealPlanName);

  const handleAddClick = () => {
    if (mode === "ai") {
      handleAddToPlan();
    } else {
      handleAddSelectedRecipeToPlan();
    }
  };

  const content = mode === "ai" ? renderAiBuilder() : renderRecipePicker();

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="lg">
      <DialogTitle
        sx={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
        }}
      >
        <Box>
          <Typography variant="h6" fontWeight={800}>
            Dodaj przepis
          </Typography>
          {meal?.type && (
            <Typography variant="body2" color="text.secondary">
              {meal.type}
            </Typography>
          )}
        </Box>
        <IconButton onClick={onClose}>
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent
        dividers
        sx={{
          p: 0,
          overflow: "hidden",
          display: "flex",
          flexDirection: "column",
        }}
      >
        <Stack
          direction={{ xs: "column", md: "row" }}
          spacing={3}
          sx={{
            height: { xs: "auto", md: MODAL_BODY_HEIGHT },
            minHeight: { xs: 400, md: MODAL_BODY_HEIGHT },
            maxHeight: { xs: "70vh", md: MODAL_BODY_HEIGHT },
          }}
        >
          {/* Sidebar z wyborem trybu - nie scrolluje się */}
          <Stack
            spacing={1.5}
            sx={{
              width: { xs: "100%", md: 260 },
              p: { xs: 2, md: 3 },
              flexShrink: 0,
            }}
          >
            {modeOptions.map((option) => {
              const isActive = mode === option.key;
              return (
                <Paper
                  key={option.key}
                  variant={isActive ? "outlined" : "elevation"}
                  onClick={() => setMode(option.key)}
                  sx={(theme) => ({
                    p: 1.5,
                    display: "flex",
                    flexDirection: "column",
                    gap: 0.5,
                    cursor: "pointer",
                    borderColor: isActive
                      ? theme.palette.secondary.main
                      : undefined,
                  })}
                >
                  <Stack direction="row" spacing={1} alignItems="center">
                    {option.icon}
                    <Typography fontWeight={700}>{option.label}</Typography>
                  </Stack>
                </Paper>
              );
            })}
          </Stack>

          {/* Główna zawartość - scrollowalna */}
          <Box
            sx={{
              flex: 1,
              minWidth: 0,
              overflow: "hidden",
              display: "flex",
              flexDirection: "column",
              pt: { xs: 0, md: 3 },
              pr: { xs: 2, md: 3 },
              pb: { xs: 2, md: 3 },
            }}
          >
            <Box
              sx={{
                flex: 1,
                overflowY: "auto",
                overflowX: "hidden",
                pr: 1,
                "&::-webkit-scrollbar": {
                  width: "8px",
                },
                "&::-webkit-scrollbar-track": {
                  background: "transparent",
                },
                "&::-webkit-scrollbar-thumb": {
                  background: "#888",
                  borderRadius: "4px",
                },
                "&::-webkit-scrollbar-thumb:hover": {
                  background: "#555",
                },
              }}
            >
              {content}
            </Box>
          </Box>
        </Stack>
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} sx={{ textTransform: "none" }}>
          Anuluj
        </Button>
        <Button
          variant="contained"
          sx={{ textTransform: "none" }}
          disabled={!canAddToPlan || isAddingToPlan}
          onClick={handleAddClick}
          startIcon={
            isAddingToPlan ? (
              <CircularProgress size={16} color="inherit" />
            ) : undefined
          }
        >
          {isAddingToPlan ? "Dodawanie..." : "Dodaj do planu"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
