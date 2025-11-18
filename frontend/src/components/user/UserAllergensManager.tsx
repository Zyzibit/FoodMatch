import { useMemo, useState, useEffect } from "react";
import {
  Box,
  Button,
  Checkbox,
  Chip,
  Divider,
  FormControlLabel,
  FormGroup,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { allergenOptions } from "../../constants/allergens";

interface FoodPreferences {
  isVegan: boolean;
  isVegetarian: boolean;
  hasGlutenIntolerance: boolean;
  hasLactoseIntolerance: boolean;
  allergies: string[];
}

export default function UserAllergensManager() {
  const [preferences, setPreferences] = useState<FoodPreferences>({
    isVegan: false,
    isVegetarian: false,
    hasGlutenIntolerance: false,
    hasLactoseIntolerance: false,
    allergies: [],
  });
  const [customValue, setCustomValue] = useState("");
  const [saving, setSaving] = useState(false);
  const [lastSavedAt, setLastSavedAt] = useState<Date | null>(null);

  // Ładowanie preferencji przy montowaniu komponentu
  useEffect(() => {
    const savedPrefs = localStorage.getItem("userFoodPreferences");
    if (savedPrefs) {
      try {
        const parsed = JSON.parse(savedPrefs);
        setPreferences({
          isVegan: parsed.isVegan || false,
          isVegetarian: parsed.isVegetarian || false,
          hasGlutenIntolerance: parsed.hasGlutenIntolerance || false,
          hasLactoseIntolerance: parsed.hasLactoseIntolerance || false,
          allergies: parsed.allergies || [],
        });
      } catch (error) {
        console.error("Failed to load preferences:", error);
      }
    }
  }, []);

  const toggleAllergen = (name: string) => {
    setPreferences((prev) => ({
      ...prev,
      allergies: prev.allergies.includes(name)
        ? prev.allergies.filter((item) => item !== name)
        : [...prev.allergies, name],
    }));
  };

  const handleAddCustom = () => {
    const trimmed = customValue.trim();
    if (!trimmed) return;
    const normalized = trimmed
      .toLowerCase()
      .replace(/^\w/, (c) => c.toUpperCase());
    if (!preferences.allergies.includes(normalized)) {
      setPreferences((prev) => ({
        ...prev,
        allergies: [...prev.allergies, normalized],
      }));
    }
    setCustomValue("");
  };

  const customAllergens = useMemo(
    () =>
      preferences.allergies.filter(
        (name) => !allergenOptions.some((base) => base === name)
      ),
    [preferences.allergies]
  );

  const handleSave = () => {
    if (saving) return;
    setSaving(true);

    // Zapisujemy do localStorage jako placeholder
    localStorage.setItem("userFoodPreferences", JSON.stringify(preferences));

    // Symulujemy zapis do API – docelowo tu trafi wywołanie backendu.
    setTimeout(() => {
      setLastSavedAt(new Date());
      setSaving(false);
    }, 500);
  };

  return (
    <Stack spacing={3}>
      <Box>
        <Typography variant="h5" fontWeight={800}>
          Preferencje żywieniowe
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Określ swoje preferencje żywieniowe i nietolerancje pokarmowe.
        </Typography>
      </Box>

      {/* Dieta */}
      <Box>
        <Typography variant="h6" fontWeight={600} gutterBottom>
          Dieta
        </Typography>
        <FormGroup>
          <FormControlLabel
            control={
              <Checkbox
                checked={preferences.isVegan}
                onChange={(e) =>
                  setPreferences((prev) => ({
                    ...prev,
                    isVegan: e.target.checked,
                  }))
                }
              />
            }
            label="Dieta wegańska"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={preferences.isVegetarian}
                onChange={(e) =>
                  setPreferences((prev) => ({
                    ...prev,
                    isVegetarian: e.target.checked,
                  }))
                }
              />
            }
            label="Dieta wegetariańska"
          />
        </FormGroup>
      </Box>

      <Divider />

      {/* Nietolerancje */}
      <Box>
        <Typography variant="h6" fontWeight={600} gutterBottom>
          Nietolerancje pokarmowe
        </Typography>
        <FormGroup>
          <FormControlLabel
            control={
              <Checkbox
                checked={preferences.hasGlutenIntolerance}
                onChange={(e) =>
                  setPreferences((prev) => ({
                    ...prev,
                    hasGlutenIntolerance: e.target.checked,
                  }))
                }
              />
            }
            label="Nietolerancja glutenu"
          />
          <FormControlLabel
            control={
              <Checkbox
                checked={preferences.hasLactoseIntolerance}
                onChange={(e) =>
                  setPreferences((prev) => ({
                    ...prev,
                    hasLactoseIntolerance: e.target.checked,
                  }))
                }
              />
            }
            label="Nietolerancja laktozy"
          />
        </FormGroup>
      </Box>

      <Divider />

      {/* Alergeny */}
      <Box>
        <Typography variant="h6" fontWeight={600} gutterBottom>
          Alergeny
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Wybierz alergeny, których chcesz unikać
        </Typography>
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
                checked={preferences.allergies.includes(name)}
                onChange={() => toggleAllergen(name)}
              />
              <Typography>{name}</Typography>
            </Box>
          ))}
        </Stack>
      </Box>

      <Divider />

      {/* Dodatkowe alergeny */}
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

      {preferences.allergies.length > 0 && (
        <Stack spacing={1}>
          <Typography variant="subtitle2" fontWeight={700}>
            Wybrane alergeny
          </Typography>
          <Stack direction="row" flexWrap="wrap" gap={1}>
            {preferences.allergies.map((name) => (
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
          {saving ? "Zapisywanie…" : "Zapisz preferencje"}
        </Button>
      </Stack>
    </Stack>
  );
}
