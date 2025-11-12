import {
  Autocomplete,
  Box,
  Button,
  Checkbox,
  IconButton,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { Add, Clear, Delete } from "@mui/icons-material";
import { useMemo, useState } from "react";

type ShoppingItem = {
  id: string;
  productName: string;
  quantity?: string;
  unit?: string;
  purchased: boolean;
};

const unitOptions = [
  "sztuk",
  "dg",
  "gram",
  "kilogram",
  "litrów",
  "kawałków",
];

const suggestedProducts = [
  "Banany",
  "Płatki owsiane",
  "Jogurt naturalny",
  "Pierś z kurczaka",
  "Szpinak",
  "Pomidor",
];

export default function ShoppingListPage() {
  const [search, setSearch] = useState("");
  const [quantity, setQuantity] = useState("");
  const [unit, setUnit] = useState<string | null>(null);
  const [items, setItems] = useState<ShoppingItem[]>([]);

  const canAddItem = search.trim().length > 0;

  const handleAddItem = () => {
    if (!canAddItem) return;
    setItems((prev) => [
      ...prev,
      {
        id: crypto.randomUUID(),
        productName: search.trim(),
        quantity: quantity.trim() || undefined,
        unit: unit || undefined,
        purchased: false,
      },
    ]);
    setSearch("");
    setQuantity("");
    setUnit(null);
  };

  const handleToggle = (id: string) => {
    setItems((prev) =>
      prev.map((item) =>
        item.id === id ? { ...item, purchased: !item.purchased } : item
      )
    );
  };

  const handleRemove = (id: string) => {
    setItems((prev) => prev.filter((item) => item.id !== id));
  };

  const handleClear = () => setItems([]);

  const renderedItems = useMemo(
    () =>
      items.map((item) => (
        <Box
          key={item.id}
          sx={{
            display: "flex",
            alignItems: "center",
            justifyContent: "space-between",
            borderBottom: (theme) => `1px solid ${theme.palette.grey[200]}`,
            py: 1.5,
            px: 1,
          }}
        >
          <Box sx={{ display: "flex", alignItems: "center", flex: 1, gap: 1 }}>
            <Checkbox
              checked={item.purchased}
              onChange={() => handleToggle(item.id)}
            />
            <Box>
              <Typography
                variant="body1"
                sx={{
                  textDecoration: item.purchased ? "line-through" : "none",
                }}
              >
                {item.productName}
              </Typography>
              <Typography
                variant="body2"
                color="text.secondary"
                sx={{
                  textDecoration: item.purchased ? "line-through" : "none",
                }}
              >
                {item.quantity ?? "—"} {item.unit ?? ""}
              </Typography>
            </Box>
          </Box>
          <IconButton onClick={() => handleRemove(item.id)}>
            <Delete />
          </IconButton>
        </Box>
      )),
    [items]
  );

  return (
    <Box
      sx={{
        width: "100%",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
      }}
    >
      <Typography
        variant="h4"
        sx={{ fontWeight: 800, mb: 3, width: "100%", maxWidth: 900 }}
      >
        Do kupienia
      </Typography>

      <Paper sx={{ width: "100%", maxWidth: 900, p: 3 }}>
        <Stack direction={{ xs: "column", md: "row" }} spacing={2} mb={3}>
        <Autocomplete
          freeSolo
          options={suggestedProducts}
          value={search}
          onInputChange={(_, value) => setSearch(value)}
          renderInput={(params) => (
            <TextField {...params} label="Produkt" placeholder="np. banany" />
          )}
          sx={{ flex: 2 }}
        />

        <TextField
          value={quantity}
          onChange={(e) => setQuantity(e.target.value)}
          placeholder="500"
          type="number"
          inputProps={{ min: 0 }}
          sx={{ width: 120 }}
        />

        <Autocomplete
          options={unitOptions}
          value={unit}
          onChange={(_, value) => setUnit(value)}
          renderInput={(params) => (
            <TextField {...params} placeholder="Jednostka" />
          )}
          sx={{ width: 160 }}
        />

        <Button
          variant="contained"
          startIcon={<Add />}
          disabled={!canAddItem}
          onClick={handleAddItem}
          sx={{ whiteSpace: "nowrap", textTransform: "none" }}
        >
          Dodaj
        </Button>
      </Stack>

        <Paper variant="outlined" sx={{ maxHeight: 420, overflowY: "auto" }}>
        {items.length === 0 ? (
          <Box sx={{ p: 3, textAlign: "center" }}>
            <Typography variant="body2" color="text.secondary">
              Brak produktów na liście. Dodaj coś powyżej.
            </Typography>
          </Box>
        ) : (
          renderedItems
        )}
      </Paper>

        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={2}
          justifyContent="flex-end"
          mt={3}
        >
        <Button
          variant="outlined"
          startIcon={<Clear />}
          color="error"
          onClick={handleClear}
          sx={{ textTransform: "none" }}
        >
          Wyczyść listę zakupów
        </Button>
        </Stack>
      </Paper>
    </Box>
  );
}
