import { Add, AutoAwesome, Close, Delete, MenuBook } from "@mui/icons-material";
import {
  Autocomplete,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  IconButton,
  Paper,
  Stack,
  TextField,
  Typography,
  FormControlLabel,
  Checkbox,
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
import { searchProducts, type ProductDto } from "../../services/productService";

type AddMode = "recipes" | "ai";

type PlanAddRecipeModalProps = {
  open: boolean;
  onClose: () => void;
  meal?: Pick<PlanMeal, "id" | "type"> | null;
};

type RecipeSuggestion = {
  id: string;
  title: string;
  description: string;
  calories: number;
  macros: { protein: number; fat: number; carbs: number };
  tags: string[];
};

type AiProduct = {
  id: string;
  name: string;
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

const recipeSuggestions: RecipeSuggestion[] = [];

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

export default function PlanAddRecipeModal({
  open,
  onClose,
  meal,
}: PlanAddRecipeModalProps) {
  const [mode, setMode] = useState<AddMode>("recipes");
  const [selectedRecipe, setSelectedRecipe] = useState<string | null>(null);
  const [productName, setProductName] = useState("");
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
      handleSearchProducts(productName);
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [productName, handleSearchProducts]);

  useEffect(() => {
    if (!open) {
      setMode("recipes");
      setSelectedRecipe(null);
      setProductName("");
      setProducts([]);
      setCustomAllergen("");
      setCuisine("");
      setPreparationTime("");
      setServings("");
      setPreferences({
        isVegan: false,
        isVegetarian: false,
        hasGlutenIntolerance: false,
        hasLactoseIntolerance: false,
        allergies: [],
      });
      setProductSuggestions([]);
    } else {
      // Przy otwieraniu modala ładujemy preferencje z localStorage jako placeholder
      // TODO: Pobrać preferencje z backendu przez API
      const savedPrefs = localStorage.getItem("userFoodPreferences");
      if (savedPrefs) {
        try {
          const parsed = JSON.parse(savedPrefs);
          setPreferences({
            isVegan: parsed.isVegan || false,
            isVegetarian: parsed.isVegetarian || false,
            hasGlutenIntolerance: parsed.hasGlutenIntolerance || false,
            hasLactoseIntolerance: parsed.hasLactoseIntolerance || false,
            allergies: parsed.allergies || [],
          });
        } catch (error) {
          console.error("Failed to load preferences:", error);
        }
      }
    }
  }, [open]);

  const canAddProduct = productName.trim().length > 0;

  const handleAddProduct = () => {
    if (!canAddProduct) return;
    setProducts((prev) => [
      ...prev,
      {
        id: crypto.randomUUID(),
        name: productName.trim(),
      },
    ]);
    setProductName("");
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

  const renderRecipePicker = () => (
    <Stack spacing={1.5}>
      {recipeSuggestions.length === 0 && (
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Typography variant="body2" color="text.secondary">
            Nie masz jeszcze zapisanych przepisów. Wkrótce dodamy ich listę.
          </Typography>
        </Paper>
      )}
      {recipeSuggestions.map((recipe) => {
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
                  {recipe.tags.map((tag) => (
                    <Chip key={tag} label={tag} size="small" />
                  ))}
                </Stack>
              </Box>
              <Box textAlign={{ xs: "left", sm: "right" }}>
                <Typography variant="subtitle2" color="text.secondary">
                  {recipe.calories} kcal
                </Typography>
                <Typography variant="body2">
                  B: {recipe.macros.protein} g
                </Typography>
                <Typography variant="body2">
                  T: {recipe.macros.fat} g
                </Typography>
                <Typography variant="body2">
                  W: {recipe.macros.carbs} g
                </Typography>
              </Box>
            </Stack>
          </Paper>
        );
      })}
    </Stack>
  );

  const renderAiBuilder = () => (
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
            getOptionLabel={(option) =>
              typeof option === "string" ? option : option.name
            }
            value={productName}
            onInputChange={(_, value) => setProductName(value)}
            loading={isLoadingProducts}
            renderInput={(params) => (
              <TextField {...params} label="Składnik" placeholder="np. banan" />
            )}
            renderOption={(props, option) => (
              <li {...props}>
                <Box>
                  <Typography variant="body2">
                    {typeof option === "string" ? option : option.name}
                  </Typography>
                  {typeof option !== "string" && option.brand && (
                    <Typography variant="caption" color="text.secondary">
                      {option.brand}
                    </Typography>
                  )}
                </Box>
              </li>
            )}
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
                  Rekomendowany składnik
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
    </Stack>
  );

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
        <Button variant="contained" sx={{ textTransform: "none" }} disabled>
          Dodaj do planu (wkrótce)
        </Button>
      </DialogActions>
    </Dialog>
  );
}
