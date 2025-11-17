import {
  Add,
  AutoAwesome,
  Close,
  Delete,
  MenuBook,
} from "@mui/icons-material";
import {
  Autocomplete,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  IconButton,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useEffect, useState, type ReactNode } from "react";
import { allergenOptions } from "../../constants/allergens";
import type { PlanMeal } from "../../types/plan";

type AddMode = "recipes" | "ai";

type PlanAddRecipeModalProps = {
  open: boolean;
  onClose: () => void;
  meal?: Pick<PlanMeal, "id" | "type"> | null;
};

type RecipeSuggestion = {
  id: string;
  title: string;
  description: string;
  calories: number;
  macros: { protein: number; fat: number; carbs: number };
  tags: string[];
};

type AiProduct = {
  id: string;
  name: string;
};

const recipeSuggestions: RecipeSuggestion[] = [];

const suggestedProducts = [
  "Płatki owsiane",
  "Jogurt grecki",
  "Jabłko",
  "Masło orzechowe",
  "Szpinak",
  "Komosa ryżowa",
];

const modeOptions: {
  key: AddMode;
  label: string;
  icon: ReactNode;
}[] = [
  {
    key: "recipes",
    label: "Wybierz z przepisów",
    icon: <MenuBook fontSize="small" />,
  },
  {
    key: "ai",
    label: "Wygeneruj z AI",
    icon: <AutoAwesome fontSize="small" />,
  },
];

const MODAL_BODY_HEIGHT = 460;

export default function PlanAddRecipeModal({
  open,
  onClose,
  meal,
}: PlanAddRecipeModalProps) {
  const [mode, setMode] = useState<AddMode>("recipes");
  const [selectedRecipe, setSelectedRecipe] = useState<string | null>(null);
  const [productName, setProductName] = useState("");
  const [products, setProducts] = useState<AiProduct[]>([]);
  const [selectedAllergens, setSelectedAllergens] = useState<string[]>([]);

  useEffect(() => {
    if (!open) {
      setMode("recipes");
      setSelectedRecipe(null);
      setProductName("");
      setProducts([]);
      setSelectedAllergens([]);
    }
  }, [open]);

  const canAddProduct = productName.trim().length > 0;

  const handleAddProduct = () => {
    if (!canAddProduct) return;
      setProducts((prev) => [
        ...prev,
        {
          id: crypto.randomUUID(),
          name: productName.trim(),
        },
      ]);
      setProductName("");
    };

  const handleRemoveProduct = (id: string) => {
    setProducts((prev) => prev.filter((item) => item.id !== id));
  };

  const toggleAllergen = (name: string) => {
    setSelectedAllergens((prev) =>
      prev.includes(name)
        ? prev.filter((item) => item !== name)
        : [...prev, name]
    );
  };

  const renderRecipePicker = () => (
    <Stack spacing={1.5}>
      {recipeSuggestions.length === 0 && (
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Typography variant="body2" color="text.secondary">
            Nie masz jeszcze zapisanych przepisów. Wkrótce dodamy ich listę.
          </Typography>
        </Paper>
      )}
      {recipeSuggestions.map((recipe) => {
        const isActive = selectedRecipe === recipe.id;
        return (
          <Paper
            key={recipe.id}
            variant={isActive ? "outlined" : "elevation"}
            onClick={() => setSelectedRecipe(recipe.id)}
            sx={{
              p: 2,
              borderColor: (theme) =>
                isActive ? theme.palette.secondary.main : undefined,
              cursor: "pointer",
              transition: "border-color 0.2s ease",
            }}
          >
            <Stack
              direction={{ xs: "column", sm: "row" }}
              justifyContent="space-between"
              spacing={1.5}
            >
              <Box>
                <Typography variant="subtitle1" fontWeight={700}>
                  {recipe.title}
                </Typography>
                <Typography variant="body2" color="text.secondary" mb={1}>
                  {recipe.description}
                </Typography>
                <Stack direction="row" spacing={1} flexWrap="wrap">
                  {recipe.tags.map((tag) => (
                    <Chip key={tag} label={tag} size="small" />
                  ))}
                </Stack>
              </Box>
              <Box textAlign={{ xs: "left", sm: "right" }}>
                <Typography variant="subtitle2" color="text.secondary">
                  {recipe.calories} kcal
                </Typography>
                <Typography variant="body2">
                  B: {recipe.macros.protein} g
                </Typography>
                <Typography variant="body2">
                  T: {recipe.macros.fat} g
                </Typography>
                <Typography variant="body2">
                  W: {recipe.macros.carbs} g
                </Typography>
              </Box>
            </Stack>
          </Paper>
        );
      })}
    </Stack>
  );

  const renderAiBuilder = () => (
    <Stack spacing={2}>
      <Stack
        direction={{ xs: "column", md: "row" }}
        spacing={1.5}
        alignItems={{ md: "flex-end" }}
      >
        <Autocomplete
          freeSolo
          options={suggestedProducts}
          value={productName}
          onInputChange={(_, value) => setProductName(value)}
          renderInput={(params) => (
            <TextField {...params} label="Składnik" placeholder="np. banan" />
          )}
          sx={{ flex: 2 }}
        />
        <Button
          variant="contained"
          startIcon={<Add />}
          disabled={!canAddProduct}
          onClick={handleAddProduct}
          sx={{ whiteSpace: "nowrap", textTransform: "none" }}
        >
          Dodaj
        </Button>
      </Stack>

      <Paper variant="outlined" sx={{ maxHeight: 220, overflowY: "auto", p: 2 }}>
        {products.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            Dodaj składniki, które AI ma wykorzystać w przepisie.
          </Typography>
        ) : (
          products.map((product) => (
            <Box
              key={product.id}
              sx={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
                py: 1,
              }}
            >
              <Box>
                <Typography fontWeight={600}>{product.name}</Typography>
                <Typography variant="body2" color="text.secondary">
                  Rekomendowany składnik
                </Typography>
              </Box>
              <IconButton onClick={() => handleRemoveProduct(product.id)}>
                <Delete />
              </IconButton>
            </Box>
          ))
        )}
      </Paper>

      <Divider />

      <Box>
        <Typography variant="subtitle2" fontWeight={700} gutterBottom>
          Alergeny do pominięcia
        </Typography>
        <Stack direction="row" flexWrap="wrap" gap={1}>
          {allergenOptions.map((name) => {
            const active = selectedAllergens.includes(name);
            return (
              <Chip
                key={name}
                label={name}
                color={active ? "secondary" : "default"}
                variant={active ? "filled" : "outlined"}
                onClick={() => toggleAllergen(name)}
              />
            );
          })}
        </Stack>
      </Box>
    </Stack>
  );

  const content = mode === "ai" ? renderAiBuilder() : renderRecipePicker();

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="lg">
      <DialogTitle
        sx={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
        }}
      >
        <Box>
          <Typography variant="h6" fontWeight={800}>
            Dodaj przepis
          </Typography>
          {meal?.type && (
            <Typography variant="body2" color="text.secondary">
              {meal.type}
            </Typography>
          )}
        </Box>
        <IconButton onClick={onClose}>
          <Close />
        </IconButton>
      </DialogTitle>
      <DialogContent
        dividers
        sx={{
          minHeight: { md: MODAL_BODY_HEIGHT },
          maxHeight: { md: MODAL_BODY_HEIGHT },
          overflow: { md: "hidden" },
        }}
      >
        <Stack direction={{ xs: "column", md: "row" }} spacing={3} sx={{ height: "100%" }}>
          <Stack spacing={1.5} sx={{ width: { xs: "100%", md: 260 } }}>
            {modeOptions.map((option) => {
              const isActive = mode === option.key;
              return (
                <Paper
                  key={option.key}
                  variant={isActive ? "outlined" : "elevation"}
                  onClick={() => setMode(option.key)}
                  sx={(theme) => ({
                    p: 1.5,
                    display: "flex",
                    flexDirection: "column",
                    gap: 0.5,
                    cursor: "pointer",
                    borderColor: isActive
                      ? theme.palette.secondary.main
                      : undefined,
                  })}
                >
                  <Stack direction="row" spacing={1} alignItems="center">
                    {option.icon}
                    <Typography fontWeight={700}>{option.label}</Typography>
                  </Stack>
                </Paper>
              );
            })}
          </Stack>
          <Box
            sx={{
              flex: 1,
              minWidth: 0,
              height: { xs: "auto", md: "100%" },
              overflow: "hidden",
            }}
          >
            <Box
              sx={{
                height: "100%",
                overflowY: "auto",
                pr: { md: 1 },
              }}
            >
              {content}
            </Box>
          </Box>
        </Stack>
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} sx={{ textTransform: "none" }}>
          Anuluj
        </Button>
        <Button
          variant="contained"
          sx={{ textTransform: "none" }}
          disabled
        >
          Dodaj do planu (wkrótce)
        </Button>
      </DialogActions>
    </Dialog>
  );
}
