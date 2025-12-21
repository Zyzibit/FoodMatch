import {
  Box,
  Button,
  Chip,
  Collapse,
  Stack,
  Typography,
  CircularProgress,
} from "@mui/material";
import { Delete, Add, Share, Public } from "@mui/icons-material";
import type { SavedRecipe, SavedRecipeIngredient } from "../../types/recipes";
import PlanMealMacroSummary from "../plan/PlanMealMacroSummary";
import { useState } from "react";
import ProductDetailsDialog from "../products/ProductDetailsDialog";
import { colors } from "../../theme";

type SelectedIngredientData = {
  productName: string;
  estimatedCalories?: number;
  estimatedProteins?: number;
  estimatedCarbohydrates?: number;
  estimatedFats?: number;
  normalizedQuantityInGrams?: number;
};

type SavedRecipeCardProps = {
  recipe: SavedRecipe;
  isExpanded?: boolean;
  onToggle?: (recipe: SavedRecipe) => void;
  onRemove?: (recipe: SavedRecipe) => void;
  onCopy?: (recipe: SavedRecipe) => void;
  onShare?: (recipe: SavedRecipe) => void;
  isCopying?: boolean;
  isSharing?: boolean;
};

export default function SavedRecipeCard({
  recipe,
  isExpanded = false,
  onToggle,
  onRemove,
  onCopy,
  onShare,
  isCopying = false,
  isSharing = false,
}: SavedRecipeCardProps) {
  const [selectedProductId, setSelectedProductId] = useState<string | null>(
    null
  );
  const [selectedIngredientData, setSelectedIngredientData] = useState<
    SelectedIngredientData | null
  >(null);

  const handleToggle = () => onToggle?.(recipe);
  const handleRemove = () => onRemove?.(recipe);
  const handleCopy = () => onCopy?.(recipe);
  const handleShare = () => onShare?.(recipe);

  const handleIngredientClick = (ingredient: SavedRecipeIngredient) => {
    if (typeof ingredient !== "string") {
      if (ingredient.productId) {
        setSelectedProductId(String(ingredient.productId));
      } else {
        setSelectedProductId("ai-product-" + Math.random());
      }
      
      if (
        ingredient.calories !== undefined ||
        ingredient.proteins !== undefined ||
        ingredient.carbohydrates !== undefined ||
        ingredient.fats !== undefined
      ) {
        setSelectedIngredientData({
          productName: ingredient.name,
          estimatedCalories: ingredient.calories,
          estimatedProteins: ingredient.proteins,
          estimatedCarbohydrates: ingredient.carbohydrates,
          estimatedFats: ingredient.fats,
          normalizedQuantityInGrams: ingredient.normalizedQuantityInGrams,
        });
      } else {
        // Don't pass embedded data if not available - let dialog fetch from API
        setSelectedIngredientData(null);
      }
    }
  };

  return (
    <Box
      sx={{
        borderBottom: (theme) => `1px solid ${theme.palette.grey[200]}`,
        pb: 2,
      }}
    >
      <Stack
        direction={{ xs: "column", sm: "row" }}
        justifyContent="space-between"
        spacing={2}
      >
        <Box sx={{ flex: 1 }}>
          <Typography variant="body2" color="text.secondary" fontWeight={600}>
            {recipe.calories} kcal • zapisana{" "}
            {recipe.createdAt
              ? new Date(recipe.createdAt).toLocaleDateString("pl-PL")
              : "niedawno"}
          </Typography>
          <Typography variant="h6" fontWeight={800}>
            {recipe.title}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
            {recipe.description}
          </Typography>

          <PlanMealMacroSummary macros={recipe.macros} />

          {recipe.tags?.length ? (
            <Stack direction="row" spacing={1} flexWrap="wrap" mt={1}>
              {recipe.tags.map((tag) => (
                <Chip key={tag} label={tag} size="small" />
              ))}
            </Stack>
          ) : null}
        </Box>

        <Stack spacing={1} alignItems={{ xs: "flex-start", sm: "flex-end" }}>
          {onShare && !recipe.isPublic && (
            <Button
              startIcon={isSharing ? <CircularProgress size={16} /> : <Share />}
              color="primary"
              variant="text"
              sx={{ textTransform: "none" }}
              onClick={handleShare}
              disabled={isSharing}
            >
              {isSharing ? "Udostępnianie..." : "Udostępnij"}
            </Button>
          )}
          {onShare && recipe.isPublic && (
            <Button
              startIcon={<Public />}
              color="success"
              variant="text"
              sx={{ textTransform: "none" }}
              disabled
            >
              Publiczny
            </Button>
          )}
          {onCopy && (
            <Button
              startIcon={isCopying ? <CircularProgress size={16} /> : <Add />}
              color="primary"
              variant="text"
              sx={{ textTransform: "none" }}
              onClick={handleCopy}
              disabled={isCopying}
            >
              {isCopying ? "Dodawanie..." : "Dodaj do moich"}
            </Button>
          )}
          {onRemove && (
            <Button
              startIcon={<Delete />}
              color="error"
              variant="text"
              sx={{ textTransform: "none" }}
              onClick={handleRemove}
            >
              Usuń
            </Button>
          )}
          <Button
            size="small"
            variant="text"
            sx={{ textTransform: "none" }}
            onClick={handleToggle}
          >
            {isExpanded ? "Zwiń" : "Rozwiń"}
          </Button>
        </Stack>
      </Stack>

      <Collapse in={isExpanded} timeout="auto" unmountOnExit>
        <Box
          sx={{
            mt: 1.5,
            pt: 1.5,
            borderTop: (theme) => `1px dashed ${theme.palette.grey[300]}`,
          }}
        >
          {recipe.description && (
            <Box sx={{ mb: 2 }}>
              <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                Opis
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ whiteSpace: "pre-wrap" }}>
                {recipe.description}
              </Typography>
            </Box>
          )}

          <Typography variant="subtitle2" fontWeight={700} gutterBottom>
            Produkty
          </Typography>
          <Stack spacing={1}>
            {recipe.ingredients.map((ingredient, index) => (
              <Box
                key={index}
                onClick={() => handleIngredientClick(ingredient)}
                sx={{
                  borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
                  pb: 1,
                  cursor:
                    typeof ingredient !== "string" &&
                    (ingredient.productId || ingredient.calories !== undefined)
                      ? "pointer"
                      : "default",
                }}
              >
                {typeof ingredient === "string" ? (
                  <Typography variant="body2" color="text.secondary">
                    {ingredient}
                  </Typography>
                ) : (
                  <Box>
                    <Typography
                      fontWeight={700}
                      sx={{
                        "&:hover":
                          ingredient.productId || ingredient.calories !== undefined
                            ? { textDecoration: "underline" }
                            : {},
                      }}
                    >
                      {ingredient.name}
                    </Typography>
                    {ingredient.quantity && (
                      <Typography variant="body2" color="text.secondary">
                        {ingredient.quantity}
                        {ingredient.unitName ? " " + ingredient.unitName : ""}
                      </Typography>
                    )}
                    {ingredient.isAdditional && (
                      <Typography variant="caption" color="text.secondary">
                        dodany ręcznie
                      </Typography>
                    )}
                    {ingredient.source === "OpenFoodFacts" && (
                      <Typography
                        variant="caption"
                        sx={{
                          color: (theme) =>
                            theme.palette.mode === "dark"
                              ? "rgba(255,255,255,0.5)"
                              : colors.elements.openFoodFactsBadge,
                          fontSize: "0.7rem",
                        }}
                      >
                        produkt z bazy openfoodfacts
                      </Typography>
                    )}
                    {ingredient.source === "AI" && (
                      <Typography
                        variant="caption"
                        sx={{
                          color: (theme) =>
                            theme.palette.mode === "dark"
                              ? "rgba(255,255,255,0.5)"
                              : "rgba(0,0,0,0.5)",
                          fontSize: "0.7rem",
                        }}
                      >
                        wygenerowany przez AI
                      </Typography>
                    )}
                  </Box>
                )}
              </Box>
            ))}
          </Stack>

          {recipe.instructions && (
            <Box>
              <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                Instrukcje
              </Typography>
              <Typography
                variant="body2"
                color="text.secondary"
                sx={{ whiteSpace: "pre-line" }}
              >
                {recipe.instructions}
              </Typography>
            </Box>
          )}
        </Box>
      </Collapse>

      <ProductDetailsDialog
        open={selectedProductId !== null}
        onClose={() => {
          setSelectedProductId(null);
          setSelectedIngredientData(null);
        }}
        productId={selectedProductId || ""}
        ingredientData={selectedIngredientData ?? undefined}
      />
    </Box>
  );
}
