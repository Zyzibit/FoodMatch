import { useEffect, useState } from "react";
import {
  Alert,
  Box,
  Button,
  Collapse,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useDashboardContext } from "../layouts/DashboardLayout";
import userMeasurementsService from "../services/userMeasurementsService";

const ACTIVITY_LEVEL_VALUES = [
  "very_low",
  "low",
  "medium",
  "high",
  "very_high",
] as const;
type ActivityLevel = (typeof ACTIVITY_LEVEL_VALUES)[number];
type Gender = "male" | "female";

const AGE_RANGE = { min: 7, max: 100 } as const;
const WEIGHT_RANGE = { min: 30, max: 250 } as const;
const HEIGHT_RANGE = { min: 130, max: 300 } as const;

const PAL_VALUES: Record<ActivityLevel, number> = {
  very_low: 1.2,
  low: 1.375,
  medium: 1.55,
  high: 1.725,
  very_high: 1.9,
};

const ACTIVITY_OPTIONS: Array<{
  value: ActivityLevel;
  label: string;
  description: string;
  pal: number;
}> = [
  {
    value: "very_low",
    label: "Bardzo niski",
    description: "Brak ćwiczeń, praca siedząca",
    pal: PAL_VALUES.very_low,
  },
  {
    value: "low",
    label: "Niski",
    description: "Lekka aktywność 1–2× w tygodniu",
    pal: PAL_VALUES.low,
  },
  {
    value: "medium",
    label: "Średni",
    description: "Umiarkowana aktywność 3–4× w tygodniu",
    pal: PAL_VALUES.medium,
  },
  {
    value: "high",
    label: "Wysoki",
    description: "Intensywne ćwiczenia 5–6× w tygodniu",
    pal: PAL_VALUES.high,
  },
  {
    value: "very_high",
    label: "Bardzo wysoki",
    description: "Codzienne treningi lub praca fizyczna",
    pal: PAL_VALUES.very_high,
  },
];

const formatNumber = (value: number) => {
  if (Number.isInteger(value)) return value.toString();
  return value.toFixed(3).replace(/0+$/, "").replace(/\.$/, "");
};

export default function UserProfilePage() {
  const { activeTab } = useDashboardContext();
  const [age, setAge] = useState<number | "">("");
  const [weight, setWeight] = useState<number | "">("");
  const [height, setHeight] = useState<number | "">("");
  const [activity, setActivity] = useState<ActivityLevel | "">("");
  const [gender, setGender] = useState<Gender | "">("");
  const [showCalculationDetails, setShowCalculationDetails] = useState(false);

  // Dane z backendu
  const [calculatedBMR, setCalculatedBMR] = useState<number | null>(null);
  const [calculatedDailyCalories, setCalculatedDailyCalories] = useState<
    number | null
  >(null);

  const isAgeValid =
    age === "" || (age >= AGE_RANGE.min && age <= AGE_RANGE.max);
  const isWeightValid =
    weight === "" || (weight >= WEIGHT_RANGE.min && weight <= WEIGHT_RANGE.max);
  const isHeightValid =
    height === "" || (height >= HEIGHT_RANGE.min && height <= HEIGHT_RANGE.max);

  const showAgeError = age !== "" && !isAgeValid;
  const showWeightError = weight !== "" && !isWeightValid;
  const showHeightError = height !== "" && !isHeightValid;
  const hasValidationError = showAgeError || showWeightError || showHeightError;

  const numericAge = typeof age === "number" ? age : null;
  const numericWeight = typeof weight === "number" ? weight : null;
  const numericHeight = typeof height === "number" ? height : null;
  const canCalculate =
    numericAge !== null &&
    numericWeight !== null &&
    numericHeight !== null &&
    gender !== "" &&
    activity !== "" &&
    !hasValidationError;

  // Mapowanie ActivityLevel z backendu do lokalnego typu
  const mapBackendActivityToLocal = (
    backendActivity?: string
  ): ActivityLevel => {
    const activityMap: Record<string, ActivityLevel> = {
      Sedentary: "very_low",
      LightlyActive: "low",
      ModeratelyActive: "medium",
      VeryActive: "high",
      ExtraActive: "very_high",
    };
    return activityMap[backendActivity || ""] || "medium";
  };

  // Mapowanie Gender z backendu do lokalnego typu
  const mapBackendGenderToLocal = (backendGender?: string): Gender => {
    if (backendGender === "Male") return "male";
    if (backendGender === "Female") return "female";
    return "male";
  };

  useEffect(() => {
    // Załaduj dane z API
    const loadFromApi = async () => {
      try {
        const prefs = await userMeasurementsService.getPreferences();
        if (prefs.age) setAge(prefs.age);
        if (prefs.weight) setWeight(prefs.weight);
        if (prefs.height) setHeight(prefs.height);
        if (prefs.gender) setGender(mapBackendGenderToLocal(prefs.gender));
        if (prefs.activityLevel)
          setActivity(mapBackendActivityToLocal(prefs.activityLevel));

        // Ustaw obliczone wartości z backendu
        if (prefs.calculatedBMR) setCalculatedBMR(prefs.calculatedBMR);
        if (prefs.calculatedDailyCalories)
          setCalculatedDailyCalories(prefs.calculatedDailyCalories);
      } catch (error) {
        console.error("Failed to load preferences from API:", error);
      }
    };

    loadFromApi();
  }, []);

  const selectedActivityOption =
    activity !== ""
      ? ACTIVITY_OPTIONS.find((option) => option.value === activity)
      : undefined;

  const palValue = activity !== "" ? PAL_VALUES[activity] : null;
  const palDisplay = palValue !== null ? formatNumber(palValue) : null;

  // Mapowanie lokalnego ActivityLevel do formatu backendu
  const mapLocalActivityToBackend = (
    localActivity: ActivityLevel | ""
  ): string => {
    const activityMap: Record<ActivityLevel, string> = {
      very_low: "Sedentary",
      low: "LightlyActive",
      medium: "ModeratelyActive",
      high: "VeryActive",
      very_high: "ExtraActive",
    };
    return localActivity ? activityMap[localActivity] : "ModeratelyActive";
  };

  // Mapowanie lokalnego Gender do formatu backendu
  const mapLocalGenderToBackend = (localGender: Gender | ""): string => {
    if (localGender === "male") return "Male";
    if (localGender === "female") return "Female";
    return "Male";
  };

  const persistProfile = async (message: string) => {
    try {
      await userMeasurementsService.updatePreferences({
        age: typeof age === "number" ? age : undefined,
        weight: typeof weight === "number" ? weight : undefined,
        height: typeof height === "number" ? height : undefined,
        gender: gender
          ? (mapLocalGenderToBackend(gender) as "Male" | "Female")
          : undefined,
        activityLevel: activity
          ? (mapLocalActivityToBackend(activity) as any)
          : undefined,
      });

      // Po zapisaniu pobierz zaktualizowane obliczenia
      const prefs = await userMeasurementsService.getPreferences();
      if (prefs.calculatedBMR) setCalculatedBMR(prefs.calculatedBMR);
      if (prefs.calculatedDailyCalories)
        setCalculatedDailyCalories(prefs.calculatedDailyCalories);

      alert(message);
    } catch (error) {
      console.error("Failed to save to API:", error);
      alert("Błąd podczas zapisywania pomiarów. Spróbuj ponownie.");
    }
  };

  const handleSaveMeasurements = () => {
    if (hasValidationError) {
      alert("Popraw wartości pól, aby mieściły się w wymaganych zakresach.");
      return;
    }
    persistProfile("Pomiary zapisane");
  };

  const renderMeasurements = () => (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Pomiary
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Zapisz swój wiek, masę, wzrost oraz podstawowe parametry stylu życia. Na
        ich podstawie wyliczymy Twoje zapotrzebowanie.
      </Typography>
      <Box
        sx={{
          display: "grid",
          gap: 2,
          gridTemplateColumns: { md: "repeat(2, minmax(0, 1fr))", xs: "1fr" },
        }}
      >
        <TextField
          label="Wiek (lata)"
          type="number"
          value={age}
          onChange={(e) =>
            setAge(e.target.value === "" ? "" : Number(e.target.value))
          }
          inputProps={{ min: AGE_RANGE.min, max: AGE_RANGE.max }}
          error={showAgeError}
          helperText={
            showAgeError
              ? `Wiek musi mieścić się w zakresie ${AGE_RANGE.min}-${AGE_RANGE.max} lat.`
              : undefined
          }
        />

        <TextField
          label="Waga (kg)"
          type="number"
          value={weight}
          onChange={(e) =>
            setWeight(e.target.value === "" ? "" : Number(e.target.value))
          }
          inputProps={{ min: WEIGHT_RANGE.min, max: WEIGHT_RANGE.max }}
          error={showWeightError}
          helperText={
            showWeightError
              ? `Waga musi mieścić się w zakresie ${WEIGHT_RANGE.min}-${WEIGHT_RANGE.max} kg.`
              : undefined
          }
        />

        <TextField
          label="Wzrost (cm)"
          type="number"
          value={height}
          onChange={(e) =>
            setHeight(e.target.value === "" ? "" : Number(e.target.value))
          }
          inputProps={{ min: HEIGHT_RANGE.min, max: HEIGHT_RANGE.max }}
          error={showHeightError}
          helperText={
            showHeightError
              ? `Wzrost musi mieścić się w zakresie ${HEIGHT_RANGE.min}-${HEIGHT_RANGE.max} cm.`
              : undefined
          }
        />

        <FormControl>
          <InputLabel id="gender-label">Płeć</InputLabel>
          <Select
            labelId="gender-label"
            label="Płeć"
            value={gender}
            onChange={(e) => setGender(e.target.value as Gender)}
          >
            <MenuItem value={"female"}>Kobieta</MenuItem>
            <MenuItem value={"male"}>Mężczyzna</MenuItem>
          </Select>
        </FormControl>

        <FormControl>
          <InputLabel id="activity-label">Poziom aktywności</InputLabel>
          <Select
            labelId="activity-label"
            label="Poziom aktywności"
            value={activity}
            onChange={(e) => setActivity(e.target.value as ActivityLevel)}
          >
            {ACTIVITY_OPTIONS.map(({ value, label, description, pal }) => (
              <MenuItem key={value} value={value}>
                <Box>
                  <Typography variant="body2" fontWeight={600}>
                    {label}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {description} · PAL {pal}
                  </Typography>
                </Box>
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      <Box sx={{ display: "flex", justifyContent: "flex-end", mt: 3 }}>
        <Button
          variant="contained"
          onClick={handleSaveMeasurements}
          disabled={hasValidationError}
        >
          Zapisz pomiary
        </Button>
      </Box>
    </Paper>
  );

  const renderDemandCard = () => (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Zapotrzebowanie energetyczne
      </Typography>
      <Stack spacing={1.5} sx={{ mb: 3 }}>
        <Button
          variant="outlined"
          size="small"
          onClick={() => setShowCalculationDetails((prev) => !prev)}
          sx={{ alignSelf: "flex-start" }}
        >
          {showCalculationDetails
            ? "Ukryj sposób obliczania"
            : "Sposób obliczania"}
        </Button>

        <Collapse in={showCalculationDetails}>
          <Stack spacing={1.5}>
            <Typography variant="body2" color="text.secondary">
              Podstawowa przemiana materii (BMR, Basal Metabolic Rate) obliczamy
              ze wzoru Mifflin-St Jeor: BMR = (10×W + 6.25×H − 5×A + S), gdzie W
              to masa ciała w kilogramach, H wzrost w centymetrach, A wiek w
              latach, a S to stała zależna od płci (+5 dla mężczyzn, −161 dla
              kobiet).
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Całkowite dzienne zapotrzebowanie energetyczne (TDEE, Total Daily
              Energy Expenditure) uwzględnia poziom aktywności fizycznej: TDEE =
              BMR × PAL, gdzie PAL (Physical Activity Level) to współczynnik
              aktywności fizycznej w zakresie 1.2–1.9.
            </Typography>
            {canCalculate &&
              calculatedBMR !== null &&
              calculatedDailyCalories !== null && (
                <>
                  <Typography variant="body2" color="text.secondary">
                    Dla Twoich danych: BMR = (10×{numericWeight} + 6.25×
                    {numericHeight} − 5×{numericAge} +{" "}
                    {gender === "male" ? "+5" : "−161"}) = {calculatedBMR} kcal.
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    TDEE = BMR × PAL = {calculatedBMR} × {palDisplay} ={" "}
                    {calculatedDailyCalories} kcal/dzień.
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    Przyjęto: PAL {palDisplay} (
                    {selectedActivityOption?.label ?? "poziom aktywności"}).
                  </Typography>
                </>
              )}
          </Stack>
        </Collapse>
      </Stack>
      {!canCalculate ? (
        <Alert severity="info">
          Uzupełnij sekcję „Pomiary", aby zobaczyć wyliczone zapotrzebowanie.
        </Alert>
      ) : calculatedBMR === null || calculatedDailyCalories === null ? (
        <Alert severity="warning">
          Zapisz pomiary, aby obliczyć zapotrzebowanie energetyczne.
        </Alert>
      ) : (
        <Stack spacing={3}>
          <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
            <Box
              sx={(theme) => ({
                flex: 1,
                p: 2,
                borderRadius: 2,
                border: `1px solid ${theme.palette.divider}`,
              })}
            >
              <Typography variant="subtitle2" color="text.secondary">
                BMR – Mifflin–St Jeor
              </Typography>
              <Typography variant="h4" fontWeight={900}>
                {calculatedBMR} kcal
              </Typography>
              <Typography variant="caption" color="text.secondary">
                10×W + 6.25×H − 5×A + {gender === "male" ? "+5" : "−161"}
              </Typography>
            </Box>
            <Box
              sx={(theme) => ({
                flex: 1,
                p: 2,
                borderRadius: 2,
                border: `1px solid ${theme.palette.divider}`,
              })}
            >
              <Typography variant="subtitle2" color="text.secondary">
                TDEE – całkowite zapotrzebowanie
              </Typography>
              <Typography variant="h4" fontWeight={900} color="secondary.main">
                {calculatedDailyCalories} kcal
              </Typography>
              <Typography variant="caption" color="text.secondary">
                PAL {palDisplay} ({selectedActivityOption?.label})
              </Typography>
            </Box>
          </Stack>
        </Stack>
      )}
    </Paper>
  );

  const renderAllergens = () => (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Alergeny
      </Typography>
      <Typography variant="body2" color="text.secondary">
        Sekcja w przygotowaniu – wkrótce dodasz tu alergeny i preferencje
        żywieniowe.
      </Typography>
    </Paper>
  );

  return (
    <Box sx={{ p: 2, width: "100%" }}>
      <Stack spacing={3} sx={{ width: "100%" }}>
        {(activeTab === "pomiary" || !activeTab) && renderMeasurements()}
        {activeTab === "zapotrzebowanie" && renderDemandCard()}
        {activeTab === "alergeny" && renderAllergens()}
      </Stack>
    </Box>
  );
}
