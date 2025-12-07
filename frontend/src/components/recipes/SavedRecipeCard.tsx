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
import type { SavedRecipe } from "../../types/recipes";
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
  const handleToggle = () => onToggle?.(recipe);
  const handleRemove = () => onRemove?.(recipe);
  const handleCopy = () => onCopy?.(recipe);
  const handleShare = () => onShare?.(recipe);

  const handleIngredientClick = (
    ingredient:
      | string
      | { name: string; productId?: number | string; source?: string }
  ) => {
    if (
      typeof ingredient !== "string" &&
      ingredient.source === "OpenFoodFacts" &&
      ingredient.productId
    ) {
      setSelectedProductId(String(ingredient.productId));
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
              color="success"
              variant="outlined"
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
              variant="contained"
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
              variant="outlined"
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
          <Typography variant="subtitle2" fontWeight={700} gutterBottom>
            Składniki
          </Typography>
          <Stack component="ul" spacing={0.5} sx={{ pl: 2, m: 0 }}>
            {recipe.ingredients.map((ingredient, index) => (
              <Box
                key={index}
                component="li"
                onClick={() => handleIngredientClick(ingredient)}
                sx={{
                  cursor:
                    typeof ingredient !== "string" &&
                    ingredient.source === "OpenFoodFacts"
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
                      ingredient.source === "OpenFoodFacts"
                        ? { textDecoration: "underline" }
                        : {},
                  }}
                >
                  {typeof ingredient === "string"
                    ? ingredient
                    : ingredient.name}
                </Typography>
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
              </Box>
            ))}
          </Stack>
        </Box>
      </Collapse>

      <ProductDetailsDialog
        open={selectedProductId !== null}
        onClose={() => setSelectedProductId(null)}
        productId={selectedProductId || ""}
      />
    </Box>
  );
}
