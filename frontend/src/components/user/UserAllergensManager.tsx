import { useMemo, useState } from "react";
import {
  Box,
  Button,
  Checkbox,
  Chip,
  Divider,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { allergenOptions } from "../../constants/allergens";

export default function UserAllergensManager() {
  const [selected, setSelected] = useState<string[]>(["Laktoza"]);
  const [customValue, setCustomValue] = useState("");
  const [saving, setSaving] = useState(false);
  const [lastSavedAt, setLastSavedAt] = useState<Date | null>(null);

  const toggleAllergen = (name: string) => {
    setSelected((prev) =>
      prev.includes(name)
      ? prev.filter((item) => item !== name)
      : [...prev, name]
    );
  };

  const handleAddCustom = () => {
    const trimmed = customValue.trim();
    if (!trimmed) return;
    const normalized = trimmed
      .toLowerCase()
      .replace(/^\w/, (c) => c.toUpperCase());
    if (!selected.includes(normalized)) {
      setSelected((prev) => [...prev, normalized]);
    }
    setCustomValue("");
  };

  const customAllergens = useMemo(
    () =>
      selected.filter(
        (name) => !allergenOptions.some((base) => base === name)
      ),
    [selected]
  );

  const handleSave = () => {
    if (saving) return;
    setSaving(true);
    // Symulujemy zapis lokalny – docelowo tu trafi wywołanie API.
    setTimeout(() => {
      setLastSavedAt(new Date());
      setSaving(false);
    }, 500);
  };

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" fontWeight={800}>
          Alergeny
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Zaznacz produkty, które chcesz oznaczyć jako potencjalnie
          problematyczne. Na razie lista jest lokalna – po podpięciu backendu
          będziemy ją zapisywać w profilu.
        </Typography>
      </Box>

      <Stack spacing={1}>
        {allergenOptions.map((name) => (
          <Box
            key={name}
            sx={{
              display: "flex",
              alignItems: "center",
              borderBottom: (theme) => `1px solid ${theme.palette.divider}`,
              py: 0.5,
            }}
          >
            <Checkbox
              checked={selected.includes(name)}
              onChange={() => toggleAllergen(name)}
            />
            <Typography>{name}</Typography>
          </Box>
        ))}
      </Stack>

      <Divider />

      <Box>
        <Typography variant="subtitle1" fontWeight={600} gutterBottom>
          Dodatkowy alergen
        </Typography>
        <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
          <TextField
            label="Nazwa"
            value={customValue}
            onChange={(e) => setCustomValue(e.target.value)}
            sx={{ flex: 1 }}
          />
          <Button
            variant="contained"
            onClick={handleAddCustom}
            disabled={!customValue.trim()}
            sx={{ textTransform: "none" }}
          >
            Dodaj
          </Button>
        </Stack>
      </Box>

      {selected.length > 0 && (
        <Stack spacing={1}>
          <Typography variant="subtitle2" fontWeight={700}>
            Aktualnie zapisane
          </Typography>
          <Stack direction="row" flexWrap="wrap" gap={1}>
            {selected.map((name) => (
              <Chip
                key={name}
                label={name}
                onDelete={() => toggleAllergen(name)}
                sx={{ textTransform: "capitalize" }}
              />
            ))}
          </Stack>
          {customAllergens.length > 0 && (
            <Typography variant="caption" color="text.secondary">
              Własne alergeny: {customAllergens.join(", ")}
            </Typography>
          )}
        </Stack>
      )}

      <Stack
        direction={{ xs: "column", sm: "row" }}
        spacing={2}
        justifyContent="flex-end"
        alignItems={{ xs: "stretch", sm: "center" }}
      >
        {lastSavedAt && (
          <Typography variant="body2" color="text.secondary">
            Zapisano: {lastSavedAt.toLocaleTimeString("pl-PL")}
          </Typography>
        )}
        <Button
          variant="contained"
          color="secondary"
          onClick={handleSave}
          disabled={saving}
          sx={{ textTransform: "none" }}
        >
          {saving ? "Zapisywanie…" : "Zapisz alergeny"}
        </Button>
      </Stack>
    </Stack>
  );
}
