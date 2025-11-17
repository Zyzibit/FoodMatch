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

            {meal.products?.length ? (
              <Box mt={meal.description ? 1.5 : 0}>
                <Typography variant="subtitle2" fontWeight={700} gutterBottom>
                  Produkty
                </Typography>
                <Stack component="ul" spacing={0.5} sx={{ pl: 2, m: 0 }}>
                  {meal.products.map((product) => (
                    <Typography
                      key={product}
                      component="li"
                      variant="body2"
                      color="text.secondary"
                    >
                      {product}
                    </Typography>
                  ))}
                </Stack>
              </Box>
            ) : null}

            {!meal.description && !meal.products?.length && (
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
