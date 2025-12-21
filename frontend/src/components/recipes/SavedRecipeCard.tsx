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
  const [selectedIngredientData, setSelectedIngredientData] = useState<any>(null);

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
            Składniki
          </Typography>
          <Stack component="ul" spacing={0.5} sx={{ pl: 2, m: 0, mb: 2 }}>
            {recipe.ingredients.map((ingredient, index) => (
              <Box
                key={index}
                component="li"
                onClick={() => handleIngredientClick(ingredient)}
                sx={{
                  cursor:
                    typeof ingredient !== "string" &&
                    (ingredient.productId ||
                      ingredient.calories !== undefined)
                      ? "pointer"
                      : "default",
                }}
              >
                <Typography
                  variant="body2"
                  color="text.secondary"
                  sx={{
                    "&:hover":
                      typeof ingredient !== "string" &&
                      (ingredient.productId ||
                        ingredient.calories !== undefined)
                        ? { textDecoration: "underline" }
                        : {},
                  }}
                >
                  {typeof ingredient === "string"
                    ? ingredient
                    : `${ingredient.name}${
                        ingredient.quantity
                          ? ` - ${ingredient.quantity}${
                              ingredient.unitName ? " " + ingredient.unitName : ""
                            }`
                          : ""
                      }`}
                </Typography>
                {typeof ingredient !== "string" &&
                  ingredient.isAdditional && (
                    <Typography variant="caption" color="text.secondary">
                      dodany ręcznie
                    </Typography>
                  )}
                {typeof ingredient !== "string" &&
                  ingredient.source === "OpenFoodFacts" && (
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
                {typeof ingredient !== "string" &&
                  ingredient.source === "AI" && (
                    <Typography
                      variant="caption"
                      sx={{
                        color: (theme) =>
                          theme.palette.mode === "dark"
                            ? "rgba(255,255,255,0.5)"
                            : "#9c27b0",
                        fontSize: "0.7rem",
                      }}
                    >
                      produkt wygenerowany przez AI
                    </Typography>
                  )}
              </Box>
            ))}
          </Stack>

          {recipe.instructions && (
            <Box>
              <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                Instrukcje przygotowania
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ whiteSpace: "pre-wrap" }}>
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
        ingredientData={selectedIngredientData}
      />
    </Box>
  );
}
