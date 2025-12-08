import { Stack, Typography } from "@mui/material";
import type { SavedRecipe } from "../../types/recipes";
import SavedRecipeCard from "./SavedRecipeCard";

type SavedRecipeListProps = {
  recipes: SavedRecipe[];
  expandedId?: string | null;
  onToggle?: (recipe: SavedRecipe) => void;
  onRemove?: (recipe: SavedRecipe) => void;
  onCopy?: (recipe: SavedRecipe) => void;
  onShare?: (recipe: SavedRecipe) => void;
  copyingRecipeId?: number | null;
  sharingRecipeId?: number | null;
};

export default function SavedRecipeList({
  recipes,
  expandedId,
  onToggle,
  onRemove,
  onCopy,
  onShare,
  copyingRecipeId,
  sharingRecipeId,
}: SavedRecipeListProps) {
  if (!recipes.length) {
    return (
      <Stack
        spacing={1}
        sx={{
          p: 3,
          alignItems: "center",
          textAlign: "center",
          borderRadius: 2,
          border: (theme) => `1px dashed ${theme.palette.divider}`,
        }}
      >
        <Typography variant="subtitle1" fontWeight={600}>
          Brak zapisanych przepisów
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Zacznij dodawać swoje ulubione dania – pojawią się tutaj.
        </Typography>
      </Stack>
    );
  }

  return (
    <Stack spacing={2}>
      {recipes.map((recipe) => (
        <SavedRecipeCard
          key={recipe.id}
          recipe={recipe}
          isExpanded={expandedId === recipe.id}
          onToggle={onToggle}
          onRemove={onRemove}
          onCopy={onCopy}
          onShare={onShare}
          isCopying={copyingRecipeId === parseInt(recipe.id, 10)}
          isSharing={sharingRecipeId === parseInt(recipe.id, 10)}
        />
      ))}
    </Stack>
  );
}
