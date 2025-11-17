import {
  Box,
  Button,
  Chip,
  Collapse,
  Stack,
  Typography,
} from "@mui/material";
import { Delete } from "@mui/icons-material";
import type { SavedRecipe } from "../../types/recipes";
import PlanMealMacroSummary from "../plan/PlanMealMacroSummary";

type SavedRecipeCardProps = {
  recipe: SavedRecipe;
  isExpanded?: boolean;
  onToggle?: (recipe: SavedRecipe) => void;
  onRemove?: (recipe: SavedRecipe) => void;
};

export default function SavedRecipeCard({
  recipe,
  isExpanded = false,
  onToggle,
  onRemove,
}: SavedRecipeCardProps) {
  const handleToggle = () => onToggle?.(recipe);
  const handleRemove = () => onRemove?.(recipe);

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
          <Button
            startIcon={<Delete />}
            color="error"
            variant="text"
            sx={{ textTransform: "none" }}
            onClick={handleRemove}
          >
            Usuń
          </Button>
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
            {recipe.ingredients.map((ingredient) => (
              <Typography
                key={ingredient}
                component="li"
                variant="body2"
                color="text.secondary"
              >
                {ingredient}
              </Typography>
            ))}
          </Stack>
        </Box>
      </Collapse>
    </Box>
  );
}
