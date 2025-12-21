import { Box, Button, Chip, Collapse, Stack, Typography, Link } from "@mui/material";
import { Add, Edit, Delete } from "@mui/icons-material";
import type { PlanMeal } from "../../types/plan";
import PlanMealMacroSummary from "./PlanMealMacroSummary";
import { useState } from "react";
import ProductDetailsDialog from "../products/ProductDetailsDialog";
import { colors } from "../../theme";

type PlanMealCardProps = {
  meal: PlanMeal;
  onEdit?: (meal: PlanMeal) => void;
  onDelete?: (meal: PlanMeal) => void;
  onAddRecipe?: (meal: PlanMeal) => void;
  onExpand?: (meal: PlanMeal) => void;
  isExpanded?: boolean;
};

export default function PlanMealCard({
  meal,
  onEdit,
  onDelete,
  onAddRecipe,
  onExpand,
  isExpanded = false,
}: PlanMealCardProps) {
  const [selectedProductId, setSelectedProductId] = useState<
    number | string | null
  >(null);
  const [selectedProductData, setSelectedProductData] = useState<any>(null);
  const isPlaceholder = Boolean(meal.isPlaceholder);
  const primaryLabel = isPlaceholder ? "Dodaj przepis" : "Edytuj";
  const primaryIcon = isPlaceholder ? <Add /> : <Edit />;
  const metaLabel = isPlaceholder
    ? meal.time
    : `${meal.time} · ${meal.calories} kcal`;
  const hasProducts = Boolean(meal.products?.length);
  const hasInstructions = Boolean(meal.instructions);
  const showFallback =
    !meal.description &&
    !hasProducts &&
    !hasInstructions &&
    !meal.isDetailsLoading &&
    !meal.detailsError;

  const handlePrimaryAction = () => {
    if (isPlaceholder) {
      onAddRecipe?.(meal);
    } else {
      onEdit?.(meal);
    }
  };

  const handleProductClick = (product: {
    productId?: number;
    id: string;
    source?: string;
    name?: string;
    calories?: number;
    proteins?: number;
    carbohydrates?: number;
    fats?: number;
    normalizedQuantityInGrams?: number;
  }) => {
    if (product.source === "OpenFoodFacts" || product.source === "AI") {
      setSelectedProductId(product.productId || product.id);
      // Zawsze przesłaj dane jeśli mamy normalizedQuantityInGrams
      setSelectedProductData({
        productName: product.name,
        estimatedCalories: product.calories,
        estimatedProteins: product.proteins,
        estimatedCarbohydrates: product.carbohydrates,
        estimatedFats: product.fats,
        normalizedQuantityInGrams: product.normalizedQuantityInGrams,
      });
    }
  };

  return (
    <Box
      sx={{
        borderBottom: (theme) => `1px solid ${theme.palette.grey[200]}`,
        pb: 1.5,
      }}
    >
      <Stack
        direction="row"
        justifyContent="space-between"
        flexWrap="wrap"
        spacing={1}
      >
        <Box>
          <Typography variant="body2" color="text.secondary" fontWeight={600}>
            {metaLabel}
          </Typography>
          <Typography variant="h6" fontWeight={800}>
            {meal.type}
          </Typography>
          {!isPlaceholder ? (
            <>
              <Typography variant="body2">{meal.title}</Typography>
              <PlanMealMacroSummary macros={meal.macros} />
            </>
          ) : (
            <Typography variant="body2" color="text.secondary" mt={0.5}>
              Nie masz jeszcze przepisu dla tego posiłku. Dodaj go ręcznie lub
              wygeneruj z AI.
            </Typography>
          )}
        </Box>
        <Stack spacing={1} alignItems="flex-end">
          <Button
            startIcon={primaryIcon}
            size="small"
            variant="text"
            sx={{ textTransform: "none" }}
            onClick={handlePrimaryAction}
          >
            {primaryLabel}
          </Button>
          {!isPlaceholder && (
            <>
              <Button
                startIcon={<Delete />}
                size="small"
                variant="text"
                color="error"
                sx={{ textTransform: "none" }}
                onClick={() => onDelete?.(meal)}
              >
                Usuń
              </Button>
              <Button
                size="small"
                variant="text"
                sx={{ textTransform: "none" }}
                onClick={() => onExpand?.(meal)}
              >
                {isExpanded ? "Zwiń" : "Rozwiń"}
              </Button>
            </>
          )}
        </Stack>
      </Stack>
      {!isPlaceholder && (
        <Collapse in={isExpanded} timeout="auto" unmountOnExit>
          <Box
            sx={{
              mt: 1.5,
              pt: 1.5,
              borderTop: (theme) => `1px dashed ${theme.palette.grey[300]}`,
            }}
          >
            {meal.description && (
              <Box>
                <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                  Opis
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {meal.description}
                </Typography>
              </Box>
            )}

            {meal.isDetailsLoading && (
              <Typography
                variant="body2"
                color="text.secondary"
                mt={meal.description ? 1.5 : 0}
              >
                Ładowanie szczegółów przepisu...
              </Typography>
            )}

            {meal.detailsError && !meal.isDetailsLoading && (
              <Typography
                variant="body2"
                color="error"
                mt={meal.description ? 1.5 : 0}
              >
                {meal.detailsError}
              </Typography>
            )}

            {hasProducts && (
              <Box mt={meal.description ? 1.5 : 0}>
                <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                  Produkty
                </Typography>
                <Stack spacing={1}>
                  {meal.products?.map((product) => (
                    <Box
                      key={product.id}
                      sx={{
                        borderBottom: (theme) =>
                          `1px solid ${theme.palette.divider}`,
                        pb: 1,
                      }}
                    >
                      <Box>
                        <Typography
                          fontWeight={700}
                          onClick={() => handleProductClick(product)}
                          sx={{
                            cursor:
                              (product.source === "OpenFoodFacts" ||
                                product.source === "AI")
                                ? "pointer"
                                : "default",
                            "&:hover":
                              (product.source === "OpenFoodFacts" ||
                                product.source === "AI")
                                ? { textDecoration: "underline" }
                                : {},
                          }}
                        >
                          {product.name}
                        </Typography>
                        {product.quantityLabel && (
                          <Typography variant="body2" color="text.secondary">
                            {product.quantityLabel}
                          </Typography>
                        )}
                        {product.source === "OpenFoodFacts" && (
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
                            {product.code ? (
                              <Link
                                href={`https://world.openfoodfacts.org/product/${product.code}`}
                                target="_blank"
                                rel="noopener noreferrer"
                                sx={{
                                  color: "inherit",
                                  textDecoration: "none",
                                  cursor: "pointer",
                                  "&:hover": {
                                    textDecoration: "underline",
                                  },
                                }}
                              >
                                Open Food Facts ({product.code})
                              </Link>
                            ) : (
                              "produkt z bazy openfoodfacts"
                            )}
                          </Typography>
                        )}
                        {product.source === "AI" && (
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
                    </Box>
                  ))}
                </Stack>
              </Box>
            )}

            {hasInstructions && (
              <Box mt={meal.description || hasProducts ? 1.5 : 0}>
                <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                  Instrukcje
                </Typography>
                <Typography
                  variant="body2"
                  color="text.secondary"
                  whiteSpace="pre-line"
                >
                  {meal.instructions}
                </Typography>
              </Box>
            )}

            {showFallback && (
              <Typography variant="body2" color="text.secondary">
                Brak dodatkowych informacji dla tego posiłku.
              </Typography>
            )}
          </Box>
        </Collapse>
      )}

      <ProductDetailsDialog
        open={selectedProductId !== null}
        onClose={() => {
          setSelectedProductId(null);
          setSelectedProductData(null);
        }}
        productId={selectedProductId || ""}
        ingredientData={selectedProductData}
      />
    </Box>
  );
}
