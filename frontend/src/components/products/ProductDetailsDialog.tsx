import {
  Dialog,
  DialogTitle,
  DialogContent,
  IconButton,
  Typography,
  Box,
  Stack,
  Chip,
  Divider,
  CircularProgress,
} from "@mui/material";
import { Close } from "@mui/icons-material";
import { useEffect, useState } from "react";
import { API_BASE_URL, API_ENDPOINTS } from "../../config";
import { colors } from "../../theme";

type ProductDetails = {
  id: string;
  name: string;
  brand: string;
  barcode: string;
  imageUrl?: string;
  categories: string[];
  ingredients: string[];
  allergens: string[];
  countries: string[];
  nutrition?: {
    calories?: number;
    fat?: number;
    carbohydrates?: number;
    proteins?: number;
    estimatedCalories?: number;
    estimatedProteins?: number;
    estimatedCarbohydrates?: number;
    estimatedFats?: number;
  };
  nutritionGrade?: string;
  ecoScoreGrade?: string;
  source: string;
};

type ProductDetailsDialogProps = {
  open: boolean;
  onClose: () => void;
  productId: number | string;
};

export default function ProductDetailsDialog({
  open,
  onClose,
  productId,
}: ProductDetailsDialogProps) {
  const [product, setProduct] = useState<ProductDetails | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!open || !productId) {
      return;
    }

    const fetchProduct = async () => {
      setLoading(true);
      setError(null);
      try {
        const response = await fetch(
          `${API_BASE_URL}${API_ENDPOINTS.PRODUCTS.BASE}/${productId}`,
          {
            credentials: "include",
          }
        );

        if (!response.ok) {
          throw new Error("Nie udało się pobrać danych produktu");
        }

        const data = await response.json();
        setProduct(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Wystąpił nieznany błąd");
      } finally {
        setLoading(false);
      }
    };

    fetchProduct();
  }, [open, productId]);

  const getNutritionValue = (
    estimated?: number,
    actual?: number
  ): number | undefined => {
    return estimated ?? actual;
  };

  const getSourceLabel = (source: string) => {
    if (source === "OpenFoodFacts") return "produkt z bazy openfoodfacts";
    if (source === "AI") return "wygenerowany przez AI";
    if (source === "User") return "własny produkt";
    return "produkt z bazy FoodMatch";
  };

  const getSourceColor = (source: string) => {
    if (source === "OpenFoodFacts") return colors.elements.openFoodFactsBadge;
    return "rgba(0,0,0,0.5)";
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        <Box display="flex" justifyContent="space-between" alignItems="center">
          <Typography variant="h6" fontWeight={700}>
            Szczegóły produktu
          </Typography>
          <IconButton onClick={onClose} size="small">
            <Close />
          </IconButton>
        </Box>
      </DialogTitle>
      <DialogContent>
        {loading && (
          <Box display="flex" justifyContent="center" py={4}>
            <CircularProgress />
          </Box>
        )}

        {error && (
          <Typography color="error" variant="body2">
            {error}
          </Typography>
        )}

        {product && !loading && (
          <Stack spacing={2}>
            {product.imageUrl && (
              <Box
                component="img"
                src={product.imageUrl}
                alt={product.name}
                sx={{
                  width: "100%",
                  maxHeight: 200,
                  objectFit: "contain",
                  borderRadius: 1,
                }}
              />
            )}

            <Box>
              <Typography variant="h6" fontWeight={700}>
                {product.name}
              </Typography>
              {product.brand && (
                <Typography variant="body2" color="text.secondary">
                  {product.brand}
                </Typography>
              )}
              <Typography
                variant="caption"
                sx={{
                  color: (theme) =>
                    theme.palette.mode === "dark"
                      ? "rgba(255,255,255,0.5)"
                      : getSourceColor(product.source),
                  fontSize: "0.7rem",
                }}
              >
                {getSourceLabel(product.source)}
              </Typography>
            </Box>

            {(product.nutritionGrade || product.ecoScoreGrade) && (
              <Stack direction="row" spacing={1}>
                {product.nutritionGrade && (
                  <Chip
                    label={`Nutri-Score: ${product.nutritionGrade.toUpperCase()}`}
                    size="small"
                    color="primary"
                  />
                )}
                {product.ecoScoreGrade && (
                  <Chip
                    label={`Eco-Score: ${product.ecoScoreGrade.toUpperCase()}`}
                    size="small"
                    color="success"
                  />
                )}
              </Stack>
            )}

            <Divider />

            {product.nutrition && (
              <Box>
                <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                  Wartości odżywcze (na 100g)
                </Typography>
                <Stack spacing={0.5}>
                  {getNutritionValue(
                    product.nutrition.estimatedCalories,
                    product.nutrition.calories
                  ) !== undefined && (
                    <Typography variant="body2">
                      Kalorie:{" "}
                      {getNutritionValue(
                        product.nutrition.estimatedCalories,
                        product.nutrition.calories
                      )?.toFixed(1)}{" "}
                      kcal
                    </Typography>
                  )}
                  {getNutritionValue(
                    product.nutrition.estimatedProteins,
                    product.nutrition.proteins
                  ) !== undefined && (
                    <Typography variant="body2">
                      Białko:{" "}
                      {getNutritionValue(
                        product.nutrition.estimatedProteins,
                        product.nutrition.proteins
                      )?.toFixed(1)}{" "}
                      g
                    </Typography>
                  )}
                  {getNutritionValue(
                    product.nutrition.estimatedCarbohydrates,
                    product.nutrition.carbohydrates
                  ) !== undefined && (
                    <Typography variant="body2">
                      Węglowodany:{" "}
                      {getNutritionValue(
                        product.nutrition.estimatedCarbohydrates,
                        product.nutrition.carbohydrates
                      )?.toFixed(1)}{" "}
                      g
                    </Typography>
                  )}
                  {getNutritionValue(
                    product.nutrition.estimatedFats,
                    product.nutrition.fat
                  ) !== undefined && (
                    <Typography variant="body2">
                      Tłuszcze:{" "}
                      {getNutritionValue(
                        product.nutrition.estimatedFats,
                        product.nutrition.fat
                      )?.toFixed(1)}{" "}
                      g
                    </Typography>
                  )}
                </Stack>
              </Box>
            )}

            {product.categories && product.categories.length > 0 && (
              <Box>
                <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                  Kategorie
                </Typography>
                <Stack direction="row" spacing={0.5} flexWrap="wrap" gap={0.5}>
                  {product.categories.map((cat) => (
                    <Chip key={cat} label={cat} size="small" />
                  ))}
                </Stack>
              </Box>
            )}

            {product.allergens && product.allergens.length > 0 && (
              <Box>
                <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                  Alergeny
                </Typography>
                <Stack direction="row" spacing={0.5} flexWrap="wrap" gap={0.5}>
                  {product.allergens.map((allergen) => (
                    <Chip
                      key={allergen}
                      label={allergen}
                      size="small"
                      color="error"
                    />
                  ))}
                </Stack>
              </Box>
            )}

            {product.ingredients && product.ingredients.length > 0 && (
              <Box>
                <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                  Składniki
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {product.ingredients.join(", ")}
                </Typography>
              </Box>
            )}
          </Stack>
        )}
      </DialogContent>
    </Dialog>
  );
}
