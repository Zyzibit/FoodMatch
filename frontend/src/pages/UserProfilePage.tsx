import { useEffect, useState } from "react";
import {
  Alert,
  Box,
  Button,
  Collapse,
  Divider,
  FormControl,
  InputAdornment,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useDashboardContext } from "../layouts/DashboardLayout";

const ACTIVITY_LEVEL_VALUES = [
  "very_low",
  "low",
  "medium",
  "high",
  "very_high",
] as const;
type ActivityLevel = (typeof ACTIVITY_LEVEL_VALUES)[number];

const GOAL_VALUES = [
  "cut20",
  "cut15",
  "maintain",
  "bulk10",
  "bulk20",
] as const;
type Goal = (typeof GOAL_VALUES)[number];
type Gender = "male" | "female";
type MacroKey = "calories" | "protein" | "fat" | "carbs";
type MacroTargets = Record<MacroKey, number>;

const AGE_RANGE = { min: 7, max: 100 } as const;
const WEIGHT_RANGE = { min: 30, max: 250 } as const;
const HEIGHT_RANGE = { min: 130, max: 300 } as const;
const MACRO_KEYS: MacroKey[] = ["calories", "protein", "fat", "carbs"];

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

const CF_VALUES: Record<Goal, number> = {
  cut20: 0.8,
  cut15: 0.85,
  maintain: 1,
  bulk10: 1.1,
  bulk20: 1.2,
};

const GOAL_OPTIONS: Array<{
  value: Goal;
  label: string;
  description: string;
  cf: number;
}> = [
  {
    value: "cut20",
    label: "Redukcja –20%",
    description: "Agresywna redukcja masy ciała",
    cf: CF_VALUES.cut20,
  },
  {
    value: "cut15",
    label: "Umiarkowana redukcja –15%",
    description: "Delikatna redukcja kalorii",
    cf: CF_VALUES.cut15,
  },
  {
    value: "maintain",
    label: "Utrzymanie",
    description: "Bez zmian wagi",
    cf: CF_VALUES.maintain,
  },
  {
    value: "bulk10",
    label: "Przyrost +10%",
    description: "Budowa masy / mięśni",
    cf: CF_VALUES.bulk10,
  },
  {
    value: "bulk20",
    label: "Intensywny przyrost +20%",
    description: "Dynamiczne zwiększanie masy",
    cf: CF_VALUES.bulk20,
  },
];

const LEGACY_ACTIVITY_MAP: Record<string, ActivityLevel> = {
  niski: "low",
  sredni: "medium",
  wysoki: "high",
};

const LEGACY_GOAL_MAP: Record<string, Goal> = {
  odz: "cut20",
  przybranie: "bulk10",
  masa: "bulk20",
  utrzymanie: "maintain",
};

const MACRO_SPLITS: Record<Goal, { protein: number; fat: number; carbs: number }> = {
  cut20: { protein: 0.3, fat: 0.3, carbs: 0.4 },
  cut15: { protein: 0.28, fat: 0.27, carbs: 0.45 },
  maintain: { protein: 0.25, fat: 0.25, carbs: 0.5 },
  bulk10: { protein: 0.24, fat: 0.28, carbs: 0.48 },
  bulk20: { protein: 0.23, fat: 0.28, carbs: 0.49 },
};

const MACRO_FIELD_CONFIG: Array<{
  key: MacroKey;
  label: string;
  unit: string;
  description: string;
}> = [
  {
    key: "calories",
    label: "Kalorie",
    unit: "kcal",
    description: "Energia całkowita",
  },
  {
    key: "protein",
    label: "Białko",
    unit: "g",
    description: "Udział 4 kcal/g",
  },
  {
    key: "fat",
    label: "Tłuszcz",
    unit: "g",
    description: "Udział 9 kcal/g",
  },
  {
    key: "carbs",
    label: "Węglowodany",
    unit: "g",
    description: "Udział 4 kcal/g",
  },
];

const formatNumber = (value: number) => {
  if (Number.isInteger(value)) return value.toString();
  return value.toFixed(3).replace(/0+$/, "").replace(/\.$/, "");
};

const isActivityLevel = (value: unknown): value is ActivityLevel =>
  typeof value === "string" &&
  (ACTIVITY_LEVEL_VALUES as readonly string[]).includes(value);

const isGoalValue = (value: unknown): value is Goal =>
  typeof value === "string" &&
  (GOAL_VALUES as readonly string[]).includes(value);

const isMacroTargets = (value: unknown): value is MacroTargets =>
  !!value &&
  typeof value === "object" &&
  MACRO_KEYS.every((key) =>
    typeof (value as Record<MacroKey, unknown>)[key] === "number"
  );

export default function UserProfilePage() {
  const { activeTab } = useDashboardContext();
  const [age, setAge] = useState<number | "">("");
  const [weight, setWeight] = useState<number | "">("");
  const [height, setHeight] = useState<number | "">("");
  const [activity, setActivity] = useState<ActivityLevel | "">("");
  const [goal, setGoal] = useState<Goal | "">("");
  const [gender, setGender] = useState<Gender | "">("");
  const [manualTargets, setManualTargets] = useState<MacroTargets | null>(null);
  const [showCalculationDetails, setShowCalculationDetails] = useState(false);

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
  const palValue = activity !== "" ? PAL_VALUES[activity] : null;
  const cfValue = goal !== "" ? CF_VALUES[goal] : null;
  const canCalculate =
    numericAge !== null &&
    numericWeight !== null &&
    numericHeight !== null &&
    gender !== "" &&
    activity !== "" &&
    goal !== "" &&
    !hasValidationError;

  useEffect(() => {
    try {
      const raw = localStorage.getItem("user_profile");
      if (!raw) return;
      const parsed = JSON.parse(raw);
      if (parsed.age) setAge(parsed.age);
      if (parsed.weight) setWeight(parsed.weight);
      if (parsed.height) setHeight(parsed.height);
      if (parsed.activity) {
        if (isActivityLevel(parsed.activity)) {
          setActivity(parsed.activity);
        } else if (
          typeof parsed.activity === "string" &&
          LEGACY_ACTIVITY_MAP[parsed.activity]
        ) {
          setActivity(LEGACY_ACTIVITY_MAP[parsed.activity]);
        }
      }
      if (parsed.goal) {
        if (isGoalValue(parsed.goal)) {
          setGoal(parsed.goal);
        } else if (
          typeof parsed.goal === "string" &&
          LEGACY_GOAL_MAP[parsed.goal]
        ) {
          setGoal(LEGACY_GOAL_MAP[parsed.goal]);
        }
      }
      if (parsed.gender) setGender(parsed.gender);
      if (parsed.manualTargets && isMacroTargets(parsed.manualTargets)) {
        setManualTargets(parsed.manualTargets);
      }
    } catch (e) {}
  }, []);

  const bmr =
    canCalculate &&
    numericAge !== null &&
    numericWeight !== null &&
    numericHeight !== null
      ? Math.round(
          10 * numericWeight +
            6.25 * numericHeight -
            5 * numericAge +
            (gender === "male" ? 5 : -161)
        )
      : null;

  const cpm =
    canCalculate &&
    bmr !== null &&
    palValue !== null &&
    cfValue !== null
      ? Math.round(bmr * palValue * cfValue)
      : null;

  const selectedActivityOption =
    activity !== ""
      ? ACTIVITY_OPTIONS.find((option) => option.value === activity)
      : undefined;
  const selectedGoalOption =
    goal !== ""
      ? GOAL_OPTIONS.find((option) => option.value === goal)
      : undefined;
  const activeGoal: Goal | null = goal === "" ? null : goal;

  const computedTargets:
    | MacroTargets
    | null =
    canCalculate && cpm !== null && activeGoal
      ? (() => {
          const split = MACRO_SPLITS[activeGoal];
          return {
            calories: cpm,
            protein: Math.round((cpm * split.protein) / 4),
            fat: Math.round((cpm * split.fat) / 9),
            carbs: Math.round((cpm * split.carbs) / 4),
          } as MacroTargets;
        })()
      : null;

  const displayedTargets = manualTargets ?? computedTargets;
  const isManualOverride = manualTargets !== null;
  const macroSplit = activeGoal ? MACRO_SPLITS[activeGoal] : null;
  const palDisplay = palValue !== null ? formatNumber(palValue) : null;
  const cfDisplay = cfValue !== null ? formatNumber(cfValue) : null;
  const weightValue = numericWeight ?? 0;
  const heightValue = numericHeight ?? 0;
  const ageValue = numericAge ?? 0;

  const persistProfile = (message: string) => {
    const payload = {
      age,
      weight,
      height,
      activity,
      goal,
      gender,
      manualTargets: manualTargets ?? undefined,
    };
    localStorage.setItem("user_profile", JSON.stringify(payload));
    alert(message);
  };

  const handleSaveMeasurements = () => {
    if (hasValidationError) {
      alert("Popraw wartości pól, aby mieściły się w wymaganych zakresach.");
      return;
    }
    persistProfile("Pomiary zapisane");
  };

  const handleSaveTargets = () => {
    if (!displayedTargets) {
      alert("Najpierw uzupełnij pomiary, aby móc zapisać zapotrzebowanie.");
      return;
    }
    persistProfile("Zapotrzebowanie zapisane");
  };

  const handleManualTargetChange = (key: MacroKey, value: string) => {
    if (!computedTargets && !manualTargets) return;
    const numeric = Number(value);
    if (Number.isNaN(numeric)) return;
    setManualTargets((prev) => ({
      ...(prev ?? computedTargets ?? {
        calories: 0,
        protein: 0,
        fat: 0,
        carbs: 0,
      }),
      [key]: numeric,
    }));
  };

  const handleResetManualTargets = () => {
    setManualTargets(null);
  };

  const renderMeasurements = () => (
    <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Pomiary
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            Zapisz swój wiek, masę, wzrost oraz podstawowe parametry stylu życia.
            Na ich podstawie wyliczymy później Twoje zapotrzebowanie.
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

            <FormControl>
              <InputLabel id="goal-label">Cel</InputLabel>
              <Select
                labelId="goal-label"
                label="Cel"
                value={goal}
                onChange={(e) => setGoal(e.target.value as Goal)}
              >
                {GOAL_OPTIONS.map(({ value, label, description, cf }) => (
                  <MenuItem key={value} value={value}>
                    <Box>
                      <Typography variant="body2" fontWeight={600}>
                        {label}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {description} · CF {cf}
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

  const renderDemandCard = (editable: boolean) => (
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
          {showCalculationDetails ? "Ukryj sposób obliczania" : "Sposób obliczania"}
        </Button>

        <Collapse in={showCalculationDetails}>
          <Stack spacing={1.5}>
            <Typography variant="body2" color="text.secondary">
              Całkowite zapotrzebowanie energetyczne (CPM, Total Daily Energy Expenditure)
              obliczamy ze wzoru CPM = (10×W + 6.25×H − 5×A + S) × PAL × CF. W to masa ciała
              w kilogramach, H wzrost w centymetrach, A wiek w latach, a S to stała zależna od
              płci (+5 dla mężczyzn, −161 dla kobiet). PAL (Physical Activity Level) opisuje
              poziom codziennej aktywności, natomiast CF (Correction Factor) odnosi się do
              celu kalorycznego (redukcja, utrzymanie, nadwyżka).
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Część w nawiasie to podstawowa przemiana materii (BMR, Basal Metabolic Rate) –
              energia potrzebna tylko do podtrzymania funkcji życiowych w spoczynku. CPM
              (Całkowita Przemiana Materii) powstaje po przemnożeniu BMR przez PAL i CF,
              dzięki czemu uwzględnia styl życia oraz obrany cel żywieniowy.
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Wzór ogólny: BMR = 10×W + 6.25×H − 5×A + S, a następnie CPM = BMR × PAL × CF.
            </Typography>
            {canCalculate && bmr && cpm && palDisplay && cfDisplay && (
              <>
                <Typography variant="body2" color="text.secondary">
                  Podstawienie: BMR = 10×{weightValue} + 6.25×{heightValue} − 5×{ageValue} +{" "}
                  {gender === "male" ? "+5" : "−161"} = {bmr} kcal.
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  CPM = BMR × PAL × CF = {bmr} × {palDisplay} × {cfDisplay} = {cpm} kcal/dzień.
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Przyjęto: PAL {palDisplay} (
                  {selectedActivityOption?.label ?? "poziom aktywności"}), CF {cfDisplay} (
                  {selectedGoalOption?.label ?? "cel kaloryczny"}).
                </Typography>
              </>
            )}
          </Stack>
        </Collapse>
      </Stack>
      {!computedTargets || !canCalculate ? (
        <Alert severity="info">
          Uzupełnij sekcję „Pomiary”, aby zobaczyć wyliczone zapotrzebowanie i
          makroskładniki.
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
                {bmr} kcal
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
                CPM – całkowite zapotrzebowanie
              </Typography>
              <Typography variant="h4" fontWeight={900} color="secondary.main">
                {cpm} kcal
              </Typography>
              <Typography variant="caption" color="text.secondary">
                PAL {palValue} ({selectedActivityOption?.label}) · CF {cfValue} (
                {selectedGoalOption?.label})
              </Typography>
            </Box>
          </Stack>

          <Divider />

          <Box>
            <Stack
              direction={{ xs: "column", md: "row" }}
              alignItems={{ md: "center" }}
              justifyContent="space-between"
              spacing={1}
              sx={{ mb: 2 }}
            >
              <Typography variant="subtitle1" fontWeight={700}>
                Cele makroskładników
              </Typography>
              {editable && isManualOverride && (
                <Button
                  size="small"
                  onClick={handleResetManualTargets}
                  sx={{ alignSelf: { xs: "flex-start", md: "auto" } }}
                >
                  Resetuj do obliczeń
                </Button>
              )}
            </Stack>

            {isManualOverride && (
              <Alert
                severity={editable ? "warning" : "info"}
                sx={{ mb: 2 }}
                action={
                  editable ? (
                    <Button
                      color="inherit"
                      size="small"
                      onClick={handleResetManualTargets}
                    >
                      Resetuj
                    </Button>
                  ) : undefined
                }
              >
                Wartości zostały zmienione ręcznie.
                {!editable && " Możesz je zmienić w zakładce Zapotrzebowanie."}
              </Alert>
            )}

            {editable ? (
              <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
                {MACRO_FIELD_CONFIG.map(({ key, label, unit, description }) => (
                  <TextField
                    key={key}
                    label={label}
                    type="number"
                    value={displayedTargets ? displayedTargets[key] : ""}
                    onChange={(e) => handleManualTargetChange(key, e.target.value)}
                    fullWidth
                    InputProps={{
                      endAdornment: (
                        <InputAdornment position="end">{unit}</InputAdornment>
                      ),
                    }}
                    helperText={description}
                    disabled={!computedTargets}
                  />
                ))}
              </Stack>
            ) : (
              <Stack direction={{ xs: "column", md: "row" }} spacing={2}>
                {MACRO_FIELD_CONFIG.map(({ key, label, unit, description }) => (
                  <Box
                    key={key}
                    sx={(theme) => ({
                      flex: 1,
                      p: 2,
                      borderRadius: 2,
                      border: `1px solid ${theme.palette.divider}`,
                    })}
                  >
                    <Typography variant="body2" color="text.secondary">
                      {label}
                    </Typography>
                    <Typography variant="h5" fontWeight={800}>
                      {displayedTargets ? displayedTargets[key] : "–"} {unit}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {description}
                    </Typography>
                  </Box>
                ))}
              </Stack>
            )}
            {macroSplit && (
              <Typography
                variant="caption"
                color="text.secondary"
                sx={{ display: "block", mt: 1 }}
              >
                Aktualny rozkład opiera się na proporcjach {" "}
                {Math.round(macroSplit.protein * 100)}% białka, {" "}
                {Math.round(macroSplit.fat * 100)}% tłuszczu i {" "}
                {Math.round(macroSplit.carbs * 100)}% węglowodanów.
              </Typography>
            )}

            {editable && (
              <Box sx={{ display: "flex", justifyContent: "flex-end", mt: 3 }}>
                <Button
                  variant="contained"
                  onClick={handleSaveTargets}
                  disabled={!displayedTargets}
                >
                  Zapisz
                </Button>
              </Box>
            )}
          </Box>
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
        Sekcja w przygotowaniu – wkrótce dodasz tu alergeny i preferencje żywieniowe.
      </Typography>
    </Paper>
  );

  return (
    <Box sx={{ p: 2, width: "100%" }}>
      <Stack spacing={3} sx={{ width: "100%" }}>
        {(activeTab === "pomiary" || !activeTab) && renderMeasurements()}
        {activeTab === "zapotrzebowanie" && renderDemandCard(true)}
        {activeTab === "alergeny" && renderAllergens()}
      </Stack>
    </Box>
  );
}
