import {
  Paper,
  Stack,
  Typography,
  CircularProgress,
  Alert,
  Button,
  Box,
} from "@mui/material";
import { Add } from "@mui/icons-material";
import { useEffect, useMemo, useState } from "react";
import { useDashboardContext } from "../layouts/DashboardLayout";
import SavedRecipeList from "../components/recipes/SavedRecipeList";
import { CreateRecipeModal } from "../components/recipes/CreateRecipeModal";
import type { SavedRecipe } from "../types/recipes";
import {
  getUserRecipes,
  getCommunityRecipes,
  copyRecipeToAccount,
  shareRecipe,
  deleteRecipe,
  type RecipeDetails,
} from "../services/recipeService";

const tabLabels: Record<string, string> = {
  moje: "Moje przepisy",
  spolecznosci: "Przepisy społeczności",
};

const convertRecipeDetailsToSavedRecipe = (
  recipe: RecipeDetails
): SavedRecipe => {
  const totalCalories = recipe.calories;
  const totalProtein = recipe.proteins;
  const totalCarbs = recipe.carbohydrates;
  const totalFat = recipe.fats;

  const baseIngredients = recipe.ingredients.map((ing) => ({
    name: ing.productName,
    productId: ing.productId,
    source: ing.source as any,
  }));

  const additionalIngredients =
    recipe.additionalProducts?.map((name) => ({
      name,
      source: "User" as const,
      isAdditional: true,
    })) ?? [];

  return {
    id: recipe.id.toString(),
    title: recipe.title,
    description: recipe.description || "",
    calories: Math.round(totalCalories),
    macros: {
      protein: Math.round(totalProtein),
      fat: Math.round(totalFat),
      carbs: Math.round(totalCarbs),
    },
    tags: [],
    ingredients: [...baseIngredients, ...additionalIngredients],
    additionalProducts: recipe.additionalProducts ?? [],
    createdAt: recipe.createdAt
      ? new Date(recipe.createdAt).toISOString().split("T")[0]
      : undefined,
    isPublic: recipe.isPublic,
  };
};

export default function RecipesPage() {
  const { activeTab } = useDashboardContext();
  const label = activeTab
    ? (tabLabels[activeTab] ?? activeTab)
    : "Moje przepisy";
  const [recipes, setRecipes] = useState<SavedRecipe[]>([]);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [copyingRecipeId, setCopyingRecipeId] = useState<number | null>(null);
  const [sharingRecipeId, setSharingRecipeId] = useState<number | null>(null);
  const [createModalOpen, setCreateModalOpen] = useState(false);
  const isOwnTab = !activeTab || activeTab === "moje";

  const notice = useMemo(() => {
    if (activeTab === "spolecznosci") {
      return "Przeglądaj przepisy tworzone przez społeczność.";
    }
    return "Twoje zapisane przepisy.";
  }, [activeTab]);

  useEffect(() => {
    const loadRecipes = async () => {
      setLoading(true);
      setError(null);
      try {
        let result;
        if (isOwnTab) {
          result = await getUserRecipes();
        } else {
          result = await getCommunityRecipes();
        }
        const convertedRecipes = result.recipes.map(
          convertRecipeDetailsToSavedRecipe
        );
        setRecipes(convertedRecipes);
      } catch (err) {
        console.error("Error loading recipes:", err);
        setError(
          err instanceof Error
            ? err.message
            : "Nie udało się załadować przepisów"
        );
      } finally {
        setLoading(false);
      }
    };

    loadRecipes();
  }, [isOwnTab, activeTab]);

  const handleToggle = (recipe: SavedRecipe) => {
    setExpandedId((prev) => (prev === recipe.id ? null : recipe.id));
  };

  const handleRemove = async (recipe: SavedRecipe) => {
    const recipeId = parseInt(recipe.id, 10);
    if (Number.isNaN(recipeId)) {
      return;
    }

    try {
      await deleteRecipe(recipeId);
      setRecipes((prev) => prev.filter((item) => item.id !== recipe.id));
      setExpandedId((prev) => (prev === recipe.id ? null : prev));
    } catch (err) {
      console.error("Błąd podczas usuwania przepisu:", err);
      alert("Nie udało się usunąć przepisu");
    }
  };

  const handleCopyRecipe = async (recipe: SavedRecipe) => {
    const recipeId = parseInt(recipe.id, 10);
    if (Number.isNaN(recipeId)) {
      return;
    }

    setCopyingRecipeId(recipeId);
    try {
      await copyRecipeToAccount(recipeId);
      alert("Przepis został dodany do Twoich przepisów!");
    } catch (err) {
      console.error("Error copying recipe:", err);
      alert(
        err instanceof Error ? err.message : "Nie udało się skopiować przepisu"
      );
    } finally {
      setCopyingRecipeId(null);
    }
  };

  const handleShareRecipe = async (recipe: SavedRecipe) => {
    const recipeId = parseInt(recipe.id, 10);
    if (Number.isNaN(recipeId)) {
      return;
    }

    setSharingRecipeId(recipeId);
    try {
      await shareRecipe(recipeId);
      alert("Przepis został udostępniony społeczności!");
      // Odśwież listę przepisów
      const result = await getUserRecipes();
      const convertedRecipes = result.recipes.map(
        convertRecipeDetailsToSavedRecipe
      );
      setRecipes(convertedRecipes);
    } catch (err) {
      console.error("Error sharing recipe:", err);
      alert(
        err instanceof Error ? err.message : "Nie udało się udostępnić przepisu"
      );
    } finally {
      setSharingRecipeId(null);
    }
  };

  const handleCreateRecipeSuccess = async () => {
    // Reload recipes after creating a new one
    setLoading(true);
    try {
      const result = await getUserRecipes();
      const convertedRecipes = result.recipes.map(
        convertRecipeDetailsToSavedRecipe
      );
      setRecipes(convertedRecipes);
    } catch (err) {
      console.error("Error reloading recipes:", err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Paper
      elevation={1}
      sx={{ p: 3, width: "100%", maxWidth: 1100, mx: "auto" }}
    >
      <Stack spacing={2}>
        <Box
          sx={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Typography variant="h5" fontWeight={800}>
            Przepisy
          </Typography>
          {isOwnTab && (
            <Button
              variant="contained"
              startIcon={<Add />}
              onClick={() => setCreateModalOpen(true)}
            >
              Stwórz przepis
            </Button>
          )}
        </Box>
        <Typography variant="subtitle1">Zakładka: {label}</Typography>
        <Typography variant="body2" color="text.secondary">
          {notice}
        </Typography>

        {loading && (
          <Stack alignItems="center" py={4}>
            <CircularProgress />
          </Stack>
        )}

        {error && <Alert severity="error">{error}</Alert>}

        {!loading && !error && (
          <SavedRecipeList
            recipes={recipes}
            expandedId={expandedId}
            onToggle={handleToggle}
            onRemove={isOwnTab ? handleRemove : undefined}
            onCopy={!isOwnTab ? handleCopyRecipe : undefined}
            onShare={isOwnTab ? handleShareRecipe : undefined}
            copyingRecipeId={copyingRecipeId}
            sharingRecipeId={sharingRecipeId}
          />
        )}
      </Stack>

      <CreateRecipeModal
        open={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        onSuccess={handleCreateRecipeSuccess}
      />
    </Paper>
  );
}
