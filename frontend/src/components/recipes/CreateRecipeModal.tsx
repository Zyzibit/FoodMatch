import { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Stack,
  IconButton,
  Box,
  Typography,
  Autocomplete,
  CircularProgress,
} from "@mui/material";
import { Add, Delete } from "@mui/icons-material";
import { searchProducts, type ProductDto } from "../../services/productService";
import { getAllUnits, type UnitDto } from "../../services/unitService";
import { createRecipe, type CreateRecipeRequest } from "../../services/recipeService";
import { API_BASE_URL } from "../../config";

interface CreateRecipeModalProps {
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

interface RecipeIngredient {
  product: ProductDto | null;
  productSearch: string;
  quantity: string;
  unitId: number;
}

export function CreateRecipeModal({
  open,
  onClose,
  onSuccess,
}: CreateRecipeModalProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [calories, setCalories] = useState("");
  const [proteins, setProteins] = useState("");
  const [carbohydrates, setCarbohydrates] = useState("");
  const [fats, setFats] = useState("");
  const [instructions, setInstructions] = useState("");
  const [preparationTime, setPreparationTime] = useState("");
  const [ingredients, setIngredients] = useState<RecipeIngredient[]>([
    { product: null, productSearch: "", quantity: "", unitId: 1 },
  ]);
  const [additionalProducts, setAdditionalProducts] = useState<string[]>([""]);
  const [units, setUnits] = useState<UnitDto[]>([]);
  const [productSuggestions, setProductSuggestions] = useState<ProductDto[][]>([]);
  const [loadingProducts, setLoadingProducts] = useState<boolean[]>([]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const resolveProductId = (product: ProductDto | null): number => {
    if (!product) return 0;
    const candidates = [
      product.id,
      (product as any)?.Id,
      product.productId,
      (product as any)?.ProductId,
    ];

    for (const candidate of candidates) {
      const idValue =
        typeof candidate === "number"
          ? candidate
          : typeof candidate === "string"
            ? Number(candidate)
            : undefined;
      if (Number.isFinite(idValue) && idValue > 0) {
        return idValue;
      }
    }

    return 0;
  };

  // Load units on mount
  useState(() => {
    const loadUnits = async () => {
      try {
        const unitsData = await getAllUnits();
        setUnits(unitsData);
      } catch (err) {
        console.error("Error loading units:", err);
      }
    };
    loadUnits();
  });

  const handleSearchProducts = async (query: string, index: number) => {
    if (!query || query.trim().length < 2) {
      setProductSuggestions((prev) => {
        const newSuggestions = [...prev];
        newSuggestions[index] = [];
        return newSuggestions;
      });
      return;
    }

    setLoadingProducts((prev) => {
      const newLoading = [...prev];
      newLoading[index] = true;
      return newLoading;
    });

    try {
      const results = await searchProducts(query, 10);
      setProductSuggestions((prev) => {
        const newSuggestions = [...prev];
        newSuggestions[index] = results;
        return newSuggestions;
      });
    } catch (error) {
      console.error("Error searching products:", error);
    } finally {
      setLoadingProducts((prev) => {
        const newLoading = [...prev];
        newLoading[index] = false;
        return newLoading;
      });
    }
  };

  const addIngredient = () => {
    setIngredients([
      ...ingredients,
      { product: null, productSearch: "", quantity: "", unitId: 1 },
    ]);
  };

  const removeIngredient = (index: number) => {
    setIngredients(ingredients.filter((_, i) => i !== index));
  };

  const updateIngredient = (index: number, field: keyof RecipeIngredient, value: any) => {
    const newIngredients = [...ingredients];
    newIngredients[index] = { ...newIngredients[index], [field]: value };
    setIngredients(newIngredients);
  };

  const addAdditionalProduct = () => {
    setAdditionalProducts([...additionalProducts, ""]);
  };

  const removeAdditionalProduct = (index: number) => {
    setAdditionalProducts(additionalProducts.filter((_, i) => i !== index));
  };

  const updateAdditionalProduct = (index: number, value: string) => {
    const newProducts = [...additionalProducts];
    newProducts[index] = value;
    setAdditionalProducts(newProducts);
  };

  const calculateNutrients = async () => {
    // Calculate nutrients based on ingredients and their nutritional data
    let totalCalories = 0;
    let totalProteins = 0;
    let totalCarbohydrates = 0;
    let totalFats = 0;
    let totalWeight = 0;

    try {
      for (const ing of ingredients) {
        if (ing.product && ing.quantity) {
          const qty = parseFloat(ing.quantity);
          if (!isNaN(qty)) {
            totalWeight += qty;

            // Try to get product details to calculate nutrients
            const productId = resolveProductId(ing.product);
            if (productId > 0) {
              try {
                const response = await fetch(
                  `${API_BASE_URL}/products/${productId}`,
                  {
                    method: "GET",
                    headers: {
                      "Content-Type": "application/json",
                    },
                    credentials: "include",
                  }
                );

                if (response.ok) {
                  const productData = await response.json();
                  const nutrition = productData.nutrition || productData.Nutrition;

                  if (nutrition) {
                    const calories = nutrition.calories ?? nutrition.Calories ?? nutrition.estimatedCalories ?? nutrition.EstimatedCalories ?? 0;
                    const proteins = nutrition.proteins ?? nutrition.Proteins ?? nutrition.estimatedProteins ?? nutrition.EstimatedProteins ?? 0;
                    const carbs = nutrition.carbohydrates ?? nutrition.Carbohydrates ?? nutrition.estimatedCarbohydrates ?? nutrition.EstimatedCarbohydrates ?? 0;
                    const fats = nutrition.fat ?? nutrition.Fat ?? nutrition.estimatedFats ?? nutrition.EstimatedFats ?? 0;

                    // Assume calories/proteins/carbs/fats are per 100g
                    const caloriesPerGram = calories / 100;
                    const proteinsPerGram = proteins / 100;
                    const carbsPerGram = carbs / 100;
                    const fatsPerGram = fats / 100;

                    totalCalories += caloriesPerGram * qty;
                    totalProteins += proteinsPerGram * qty;
                    totalCarbohydrates += carbsPerGram * qty;
                    totalFats += fatsPerGram * qty;
                  }
                }
              } catch (err) {
                console.warn(`Could not fetch nutrition data for product ${productId}:`, err);
              }
            }
          }
        }
      }
    } catch (err) {
      console.error("Error calculating nutrients:", err);
    }

    return {
      calories: Math.round(totalCalories),
      proteins: Math.round(totalProteins * 10) / 10,
      carbohydrates: Math.round(totalCarbohydrates * 10) / 10,
      fats: Math.round(totalFats * 10) / 10,
      totalWeight,
    };
  };

  const handleSave = async () => {
    setError(null);

    // Validation
    if (!title.trim()) {
      setError("Tytuł jest wymagany");
      return;
    }

    if (ingredients.length === 0 || !ingredients[0].product) {
      setError("Dodaj co najmniej jeden składnik");
      return;
    }

    setSaving(true);

    try {
      const nutrients = await calculateNutrients();
      // Mapuj składniki i upewnij się, że mamy prawidłowe productId
      const mappedIngredients = ingredients
        .filter((ing) => ing.product && ing.quantity)
        .map((ing) => {
          const productId = resolveProductId(ing.product);
          return {
            productId,
            unitId: ing.unitId,
            quantity: parseFloat(ing.quantity),
            normalizedQuantityInGrams: parseFloat(ing.quantity),
          };
        })
        .filter((ing) => ing.productId > 0);

      if (mappedIngredients.length === 0) {
        setError(
          "Nie udało się dodać składników. Wybierz produkt z listy wyszukiwania i podaj ilość."
        );
        setSaving(false);
        return;
      }

      // Jeśli użytkownik wpisał makroskładniki, użyj ich zamiast domyślnych
      // Ale jeśli wartość to 0 lub pusta, zawsze użyj obliczonej
      const caloriesValue = calories.trim() && parseFloat(calories) > 0 ? parseFloat(calories) : nutrients.calories;
      const proteinsValue = proteins.trim() && parseFloat(proteins) > 0 ? parseFloat(proteins) : nutrients.proteins;
      const carbohydratesValue = carbohydrates.trim() && parseFloat(carbohydrates) > 0 ? parseFloat(carbohydrates) : nutrients.carbohydrates;
      const fatsValue = fats.trim() && parseFloat(fats) > 0 ? parseFloat(fats) : nutrients.fats;

      const request: CreateRecipeRequest = {
        title: title.trim(),
        description: description.trim(),
        instructions: instructions.trim(),
        preparationTimeMinutes: parseInt(preparationTime) || 0,
        totalWeightGrams: Math.round(nutrients.totalWeight),
        calories: caloriesValue,
        proteins: proteinsValue,
        carbohydrates: carbohydratesValue,
        fats: fatsValue,
        ingredients: mappedIngredients,
        additionalProducts: additionalProducts.filter((p) => p.trim().length > 0),
      };

      await createRecipe(request);
      onSuccess();
      handleClose();
    } catch (err) {
      console.error("Error creating recipe:", err);
      setError(err instanceof Error ? err.message : "Nie udało się utworzyć przepisu");
    } finally {
      setSaving(false);
    }
  };

  const handleClose = () => {
    setTitle("");
    setDescription("");
    setInstructions("");
    setPreparationTime("");
    setIngredients([{ product: null, productSearch: "", quantity: "", unitId: 1 }]);
    setAdditionalProducts([""]);
    setError(null);
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
      <DialogTitle>Stwórz nowy przepis</DialogTitle>
      <DialogContent>
        <Stack spacing={3} sx={{ mt: 1 }}>
          {error && (
            <Typography color="error" variant="body2">
              {error}
            </Typography>
          )}
          <TextField
            label="Tytuł przepisu"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            fullWidth
            required         />
          <TextField
            label="Opis"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            multiline
            rows={2}
            fullWidth     />
          <TextField
            label="Instrukcje przygotowania"
            value={instructions}
            onChange={(e) => setInstructions(e.target.value)}
            multiline
            rows={4}
            fullWidth         />
          <TextField
            label="Czas przygotowania (minuty)"
            type="number"
            value={preparationTime}
            onChange={(e) => setPreparationTime(e.target.value)}
            fullWidth         />
          <Box>
            <Typography variant="h6" gutterBottom>
              Makroskładniki (opcjonalnie)
            </Typography>
            <Stack direction="row" spacing={2} sx={{ mb: 2 }}>
              <TextField
                label="Kalorie (kcal)"
                type="number"
                value={calories}
                onChange={(e) => setCalories(e.target.value)}
                sx={{ width: 120 }}       />
              <TextField
                label="Białko (g)"
                type="number"
                value={proteins}
                onChange={(e) => setProteins(e.target.value)}
                sx={{ width: 120 }} />
              <TextField
                label="Węglowodany (g)"
                type="number"
                value={carbohydrates}
                onChange={(e) => setCarbohydrates(e.target.value)}
                sx={{ width: 120 }} />
              <TextField
                label="Tłuszcze (g)"
                type="number"
                value={fats}
                onChange={(e) => setFats(e.target.value)}
                sx={{ width: 120 }}
              />
            </Stack>
          </Box>
          <Box>
            <Typography variant="h6" gutterBottom>
              Składniki
            </Typography>
            {ingredients.map((ing, index) => (
              <Stack key={index} direction="row" spacing={1} sx={{ mb: 2 }}>
                <Autocomplete
                  sx={{ flex: 2 }}
                  options={productSuggestions[index] || []}
                  getOptionLabel={(option) =>
                    typeof option === "string" ? option : option.name ?? ""
                  }
                  value={ing.product}
                  inputValue={ing.productSearch}
                  onInputChange={(_, value) => {
                    updateIngredient(index, "productSearch", value);
                    handleSearchProducts(value, index);
                  }}
                  onChange={(_, value) => {
                    updateIngredient(index, "product", value);
                  }}
                  loading={loadingProducts[index]}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Produkt"
                      InputProps={{
                        ...params.InputProps,
                        endAdornment: (
                          <>
                            {loadingProducts[index] ? (
                              <CircularProgress size={20} />
                            ) : null}
                            {params.InputProps.endAdornment}
                          </>
                        ),
                      }}
                    />
                  )}
                />
                <TextField
                  label="Ilość"
                  type="number"
                  value={ing.quantity}
                  onChange={(e) => updateIngredient(index, "quantity", e.target.value)}
                  sx={{ width: 100 }}
                />
                <Autocomplete
                  sx={{ width: 120 }}
                  options={units}
                  getOptionLabel={(option) => option.name}
                  value={units.find((u) => u.unitId === ing.unitId) || null}
                  onChange={(_, value) => {
                    if (value) {
                      updateIngredient(index, "unitId", value.unitId);
                    }
                  }}
                  renderInput={(params) => <TextField {...params} label="Jednostka" />}
                />
                <IconButton onClick={() => removeIngredient(index)} disabled={ingredients.length === 1}>
                  <Delete />
                </IconButton>
              </Stack>
            ))}
            <Button startIcon={<Add />} onClick={addIngredient} variant="outlined" size="small">
              Dodaj składnik
            </Button>
          </Box>

          <Box>
            <Typography variant="h6" gutterBottom>
              Dodatkowe produkty (opcjonalne)
            </Typography>
            {additionalProducts.map((product, index) => (
              <Stack key={index} direction="row" spacing={1} sx={{ mb: 1 }}>
                <TextField
                  value={product}
                  onChange={(e) => updateAdditionalProduct(index, e.target.value)}
                  placeholder="np. sól, pieprz"
                  fullWidth
                />
                <IconButton onClick={() => removeAdditionalProduct(index)}>
                  <Delete />
                </IconButton>
              </Stack>
            ))}
            <Button
              startIcon={<Add />}
              onClick={addAdditionalProduct}
              variant="outlined"
              size="small"
            >
              Dodaj produkt
            </Button>
          </Box>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} disabled={saving}>
          Anuluj
        </Button>
        <Button onClick={handleSave} variant="contained" disabled={saving}>
          {saving ? "Zapisywanie..." : "Zapisz przepis"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
