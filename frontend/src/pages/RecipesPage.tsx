import { Paper, Stack, Typography } from "@mui/material";
import { useMemo, useState } from "react";
import { useDashboardContext } from "../layouts/DashboardLayout";
import SavedRecipeList from "../components/recipes/SavedRecipeList";
import type { SavedRecipe } from "../types/recipes";

const tabLabels: Record<string, string> = {
  moje: "Moje przepisy",
  spolecznosci: "Przepisy społeczności",
};

const mockRecipes: SavedRecipe[] = [
  {
    id: "recipe-1",
    title: "Miska burrito z indykiem",
    description:
      "Sycąca miska z komosą, mielonym indykiem w przyprawach i świeżymi warzywami.",
    calories: 520,
    macros: { protein: 36, fat: 18, carbs: 52 },
    tags: ["Obiad", "High-protein"],
    ingredients: [
      "150 g mielonego indyka",
      "120 g komosy ryżowej",
      "Papryka, kukurydza, czerwona fasola",
      "Salsa pomidorowa",
    ],
    createdAt: "2024-03-04",
  },
  {
    id: "recipe-2",
    title: "Owsianka nocna z malinami",
    description:
      "Kremowa owsianka na napoju migdałowym z nasionami chia i malinami.",
    calories: 380,
    macros: { protein: 16, fat: 12, carbs: 50 },
    tags: ["Śniadanie", "Szybkie"],
    ingredients: [
      "Płatki owsiane",
      "Napój migdałowy",
      "Nasiona chia",
      "Maliny, miód",
    ],
    createdAt: "2024-02-12",
  },
  {
    id: "recipe-3",
    title: "Sałatka z halloumi i cytrusami",
    description:
      "Lekka sałatka na bazie roszponki z grillowanym serem halloumi i sosem cytrusowym.",
    calories: 410,
    macros: { protein: 21, fat: 22, carbs: 28 },
    tags: ["Kolacja", "Vege"],
    ingredients: [
      "Ser halloumi",
      "Roszponka, rukola",
      "Pomarańcza, grejpfrut",
      "Miód, oliwa, pistacje",
    ],
  },
];

export default function RecipesPage() {
  const { activeTab } = useDashboardContext();
  const label = activeTab ? tabLabels[activeTab] ?? activeTab : "Moje przepisy";
  const [recipes, setRecipes] = useState<SavedRecipe[]>(mockRecipes);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const isOwnTab = !activeTab || activeTab === "moje";

  const notice = useMemo(() => {
    if (activeTab === "spolecznosci") {
      return "Przeglądaj przepisy tworzone przez społeczność (w przygotowaniu).";
    }
    return "Twoje zapisane przepisy – już wkrótce zsynchronizujemy je z backendem.";
  }, [activeTab]);

  const handleToggle = (recipe: SavedRecipe) => {
    setExpandedId((prev) => (prev === recipe.id ? null : recipe.id));
  };

  const handleRemove = (recipe: SavedRecipe) => {
    setRecipes((prev) => prev.filter((item) => item.id !== recipe.id));
    setExpandedId((prev) => (prev === recipe.id ? null : prev));
  };

  return (
    <Paper
      elevation={1}
      sx={{ p: 3, width: "100%", maxWidth: 1100, mx: "auto" }}
    >
      <Stack spacing={2}>
        <Typography variant="h5" fontWeight={800}>
          Przepisy
        </Typography>
        <Typography variant="subtitle1">
          Zakładka: {label}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {notice}
        </Typography>

        {isOwnTab ? (
          <SavedRecipeList
            recipes={recipes}
            expandedId={expandedId}
            onToggle={handleToggle}
            onRemove={handleRemove}
          />
        ) : (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
            Lista przepisów społeczności pojawi się tutaj.
          </Typography>
        )}
      </Stack>
    </Paper>
  );
}
