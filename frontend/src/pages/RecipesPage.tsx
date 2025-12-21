import {
  Paper,
  Stack,
  Typography,
  CircularProgress,
  Alert,
  Button,
  Box,
} from "@mui/material";
import { Add, ArrowBack, ArrowForward } from "@mui/icons-material";
import { useEffect, useMemo, useState, useRef } from "react";
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
  searchRecipes,
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
    quantity: ing.quantity,
    unitName: ing.unitName,
    normalizedQuantityInGrams: ing.normalizedQuantityInGrams,
    calories: ing.calories,
    proteins: ing.proteins,
    carbohydrates: ing.carbohydrates,
    fats: ing.fats,
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
    instructions: recipe.instructions,
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
  const [currentPage, setCurrentPage] = useState(1);
  const [totalRecipes, setTotalRecipes] = useState(0);
  const [searchQuery, setSearchQuery] = useState<string>("");
  const [recipesPerPage, setRecipesPerPage] = useState(15);
  const debounceTimerRef = useRef<NodeJS.Timeout | null>(null);
  const searchFieldRef = useRef<HTMLInputElement>(null);
  const RECIPES_PER_PAGE = recipesPerPage;
  const isOwnTab = !activeTab || activeTab === "moje";

  const totalPages = Math.ceil(totalRecipes / RECIPES_PER_PAGE);

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
        const offset = (currentPage - 1) * RECIPES_PER_PAGE;
        let result;
        if (searchQuery.trim().length > 0) {
          // Jeśli jest query, szukaj zawsze ze searchRecipes (niezależnie od tabu)
          result = await searchRecipes(searchQuery, RECIPES_PER_PAGE, offset);
        } else if (isOwnTab) {
          result = await getUserRecipes(RECIPES_PER_PAGE, offset);
        } else {
          result = await getCommunityRecipes(RECIPES_PER_PAGE, offset);
        }
        const convertedRecipes = result.recipes.map(
          convertRecipeDetailsToSavedRecipe
        );
        setRecipes(convertedRecipes);
        setTotalRecipes(result.totalCount);
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
  }, [isOwnTab, activeTab, searchQuery, currentPage]);

  useEffect(() => {
    setCurrentPage(1);
  }, [activeTab, recipesPerPage]);

  // Utrzymaj focus na polu wyszukiwania podczas wpisywania
  useEffect(() => {
    if (searchQuery.length > 0) {
      searchFieldRef.current?.focus();
    }
  }, [searchQuery]);

  const handleSearchChange = (value: string) => {
    setSearchQuery(value);
    setCurrentPage(1); // Reset do pierwszej strony

    // Wyczyść poprzedni timer
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
    }

    // Ustaw nowy timer (debounce 300ms)
    debounceTimerRef.current = setTimeout(() => {
      // loadRecipes będzie wywoływana automatycznie przez useEffect
    }, 300);
  };

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
      // Przeładuj przepisy z API
      const offset = (currentPage - 1) * RECIPES_PER_PAGE;
      const result = await getUserRecipes(RECIPES_PER_PAGE, offset);
      const convertedRecipes = result.recipes.map(
        convertRecipeDetailsToSavedRecipe
      );
      setRecipes(convertedRecipes);
      setTotalRecipes(result.totalCount);
      setExpandedId(null);
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
      // Przeładuj przepisy z API
      setCurrentPage(1); // Wróć do pierwszej strony
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
      // Przeładuj przepisy z API
      const offset = (currentPage - 1) * RECIPES_PER_PAGE;
      const result = await getUserRecipes(RECIPES_PER_PAGE, offset);
      const convertedRecipes = result.recipes.map(
        convertRecipeDetailsToSavedRecipe
      );
      setRecipes(convertedRecipes);
      setTotalRecipes(result.totalCount);
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
        {}

        {loading && (
          <Stack alignItems="center" py={4}>
            <CircularProgress />
          </Stack>
        )}

        {error && <Alert severity="error">{error}</Alert>}

        {!loading && !error && (
          <Stack spacing={2}>
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

            {/* SEKCJA PAGINACJI - Zawsze widoczna na dole */}
            <Box 
              sx={{ 
                display: "flex", 
                flexDirection: "column", 
                alignItems: "center", 
                mt: 4, 
                pt: 2, 
                gap: 2,
                borderTop: "1px solid",
                borderColor: "divider"
              }}
            >


              <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", gap: 3 }}>
                <Button
                  variant="outlined"
                  startIcon={<ArrowBack />}
                  onClick={() => setCurrentPage(Math.max(1, currentPage - 1))}
                  disabled={currentPage === 1}
                  size="small"
                >
                  Poprzednia
                </Button>
                
                <Typography variant="body2" fontWeight="medium">
                  Strona {currentPage} z {totalPages || 1}
                </Typography>

                <Button
                  variant="outlined"
                  endIcon={<ArrowForward />}
                  onClick={() => setCurrentPage(Math.min(totalPages, currentPage + 1))}
                  disabled={currentPage === totalPages || totalPages === 0}
                  size="small"
                >
                  Następna
                </Button>
                
              </Box>
            </Box>
          </Stack>
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