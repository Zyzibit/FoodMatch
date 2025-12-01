import { Box, Button, Collapse, Stack, Typography } from "@mui/material";
import { Add, Edit } from "@mui/icons-material";
import type { PlanMeal } from "../../types/plan";
import PlanMealMacroSummary from "./PlanMealMacroSummary";

type PlanMealCardProps = {
  meal: PlanMeal;
  onEdit?: (meal: PlanMeal) => void;
  onAddRecipe?: (meal: PlanMeal) => void;
  onExpand?: (meal: PlanMeal) => void;
  isExpanded?: boolean;
};

export default function PlanMealCard({
  meal,
  onEdit,
  onAddRecipe,
  onExpand,
  isExpanded = false,
}: PlanMealCardProps) {
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
            <Button
              size="small"
              variant="text"
              sx={{ textTransform: "none" }}
              onClick={() => onExpand?.(meal)}
            >
              {isExpanded ? "Zwiń" : "Rozwiń"}
            </Button>
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
              <Typography variant="body2" color="text.secondary" mt={meal.description ? 1.5 : 0}>
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
                      <Typography fontWeight={700}>{product.name}</Typography>
                      {product.quantityLabel && (
                        <Typography variant="body2" color="text.secondary">
                          {product.quantityLabel}
                        </Typography>
                      )}
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
    </Box>
  );
}
