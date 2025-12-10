import { useCallback, useEffect, useState, useRef } from "react";
import {
  Alert,
  Avatar,
  Box,
  Button,
  Collapse,
  FormControl,
  FormHelperText,
  IconButton,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { PhotoCamera, Delete as DeleteIcon } from "@mui/icons-material";
import { useDashboardContext } from "../layouts/DashboardLayout";
import userMeasurementsService, {
  type FoodPreferencesResponse,
} from "../services/userMeasurementsService";
import {
  FITNESS_GOAL_OPTIONS,
  type FitnessGoal,
} from "../constants/fitnessGoals";
import userService from "../services/userService";
import { useAuth } from "../contexts/AuthContext";
import { API_BASE_URL } from "../config";

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

const backendActivityToLocalMap: Record<string, ActivityLevel> = {
  Sedentary: "very_low",
  LightlyActive: "low",
  ModeratelyActive: "medium",
  VeryActive: "high",
  ExtraActive: "very_high",
};

const mapBackendActivityToLocal = (
  backendActivity?: string
): ActivityLevel | "" =>
  backendActivity ? (backendActivityToLocalMap[backendActivity] ?? "") : "";

const mapBackendGenderToLocal = (backendGender?: string): Gender | "" => {
  if (backendGender === "Male") return "male";
  if (backendGender === "Female") return "female";
  return "";
};

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

const mapLocalGenderToBackend = (localGender: Gender | ""): string => {
  if (localGender === "male") return "Male";
  if (localGender === "female") return "Female";
  return "Male";
};

const isFitnessGoalValue = (value?: string): value is FitnessGoal =>
  !!value &&
  FITNESS_GOAL_OPTIONS.some((goalOption) => goalOption.value === value);

const toNumberOrNull = (value?: number | null): number | null =>
  typeof value === "number" && Number.isFinite(value) ? value : null;

export default function UserProfilePage() {
  const { activeTab } = useDashboardContext();
  const { user, refreshUser } = useAuth();
  const latestPreferencesRequest = useRef(0);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [age, setAge] = useState<number | "">("");
  const [weight, setWeight] = useState<number | "">("");
  const [height, setHeight] = useState<number | "">("");
  const [activity, setActivity] = useState<ActivityLevel | "">("");
  const [gender, setGender] = useState<Gender | "">("");
  const [fitnessGoal, setFitnessGoal] = useState<FitnessGoal>("Maintenance");
  const [showCalculationDetails, setShowCalculationDetails] = useState(false);
  const [uploadingImage, setUploadingImage] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [calculatedBMR, setCalculatedBMR] = useState<number | null>(null);
  const [calculatedDailyCalories, setCalculatedDailyCalories] = useState<
    number | null
  >(null);
  const [macroGoals, setMacroGoals] = useState<{
    calories: number | null;
    protein: number | null;
    fat: number | null;
    carbs: number | null;
  }>({
    calories: null,
    protein: null,
    fat: null,
    carbs: null,
  });
  const [persistedPreferences, setPersistedPreferences] =
    useState<FoodPreferencesResponse | null>(null);

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

  const applyPreferencesFromApi = useCallback(
    (prefs: FoodPreferencesResponse) => {
      const calorieGoal =
        toNumberOrNull(prefs.calculatedDailyCalories) ??
        toNumberOrNull(prefs.dailyCalorieGoal);

      setPersistedPreferences(prefs);
      setAge(toNumberOrNull(prefs.age) ?? "");
      setWeight(toNumberOrNull(prefs.weight) ?? "");
      setHeight(toNumberOrNull(prefs.height) ?? "");
      setGender(mapBackendGenderToLocal(prefs.gender));
      setActivity(mapBackendActivityToLocal(prefs.activityLevel));
      setFitnessGoal(
        isFitnessGoalValue(prefs.fitnessGoal)
          ? prefs.fitnessGoal
          : "Maintenance"
      );
      setCalculatedBMR(toNumberOrNull(prefs.calculatedBMR));
      setCalculatedDailyCalories(toNumberOrNull(prefs.calculatedDailyCalories));

      setMacroGoals({
        calories: calorieGoal,
        protein: toNumberOrNull(prefs.dailyProteinGoal),
        fat: toNumberOrNull(prefs.dailyFatGoal),
        carbs: toNumberOrNull(prefs.dailyCarbohydrateGoal),
      });
    },
    []
  );

  const loadPreferences = useCallback(async () => {
    const requestId = Date.now();
    latestPreferencesRequest.current = requestId;
    const prefs = await userMeasurementsService.getPreferences();
    if (latestPreferencesRequest.current === requestId) {
      applyPreferencesFromApi(prefs);
    }
    return prefs;
  }, [applyPreferencesFromApi]);

  useEffect(() => {
    void loadPreferences().catch((error) =>
      console.error("Failed to load preferences from API:", error)
    );
  }, [loadPreferences]);

  useEffect(() => {
    if (activeTab === "pomiary" || activeTab === "zapotrzebowanie") {
      void loadPreferences().catch((error) =>
        console.error("Failed to refresh preferences on tab change:", error)
      );
    }
  }, [activeTab, loadPreferences]);

  const persistedActivity = mapBackendActivityToLocal(
    persistedPreferences?.activityLevel
  );
  const persistedActivityOption = persistedActivity
    ? ACTIVITY_OPTIONS.find((option) => option.value === persistedActivity)
    : undefined;
  const palValue = persistedActivity ? PAL_VALUES[persistedActivity] : null;
  const palDisplay = palValue !== null ? formatNumber(palValue) : null;
  const calorieTarget = macroGoals.calories ?? calculatedDailyCalories;
  const rawTdee =
    palValue !== null && calculatedBMR !== null
      ? Math.round(calculatedBMR * palValue)
      : null;
  const isCalorieTargetAdjusted =
    rawTdee !== null && calorieTarget !== null && calorieTarget !== rawTdee;
  const isTargetBelowBmr =
    calorieTarget !== null &&
    calculatedBMR !== null &&
    calorieTarget < calculatedBMR;
  const hasPersistedMeasurements =
    typeof persistedPreferences?.age === "number" &&
    typeof persistedPreferences?.weight === "number" &&
    typeof persistedPreferences?.height === "number" &&
    !!persistedPreferences?.gender &&
    !!persistedPreferences?.activityLevel;

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
        fitnessGoal,
      });

      // Po zapisaniu pobierz zaktualizowane obliczenia
      await loadPreferences();

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

  const handleProfilePictureClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file type
    const validTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];
    if (!validTypes.includes(file.type)) {
      alert("Nieprawidłowy format pliku. Dozwolone: JPG, PNG, GIF, WebP");
      return;
    }

    // Validate file size (5MB)
    if (file.size > 5 * 1024 * 1024) {
      alert("Plik jest za duży. Maksymalny rozmiar to 5MB");
      return;
    }

    setSelectedFile(file);

    // Create preview URL
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreviewUrl(reader.result as string);
    };
    reader.readAsDataURL(file);
  };

  const handleSaveProfilePicture = async () => {
    if (!selectedFile) return;

    setUploadingImage(true);
    try {
      await userService.uploadProfilePicture(selectedFile);
      await refreshUser();
      setSelectedFile(null);
      setPreviewUrl(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
      alert("Zdjęcie profilowe zostało zaktualizowane");
    } catch (error) {
      console.error("Failed to upload profile picture:", error);
      alert("Nie udało się wgrać zdjęcia profilowego");
    } finally {
      setUploadingImage(false);
    }
  };

  const handleDeleteProfilePicture = async () => {
    if (!confirm("Czy na pewno chcesz usunąć zdjęcie profilowe?")) return;

    setUploadingImage(true);
    try {
      await userService.deleteProfilePicture();
      await refreshUser();
      alert("Zdjęcie profilowe zostało usunięte");
    } catch (error) {
      console.error("Failed to delete profile picture:", error);
      alert("Nie udało się usunąć zdjęcia profilowego");
    } finally {
      setUploadingImage(false);
    }
  };

  const renderProfilePicture = () => {
    const displayUrl =
      previewUrl ||
      (user?.profilePictureUrl
        ? `${API_BASE_URL.replace("/api/v1", "")}${user.profilePictureUrl}`
        : undefined);

    return (
      <Paper sx={{ p: 3, mb: 3 }}>
        <Typography variant="h6" gutterBottom>
          Zdjęcie profilowe
        </Typography>
        <Stack direction="row" spacing={3} alignItems="center">
          <Box sx={{ position: "relative" }}>
            <Avatar src={displayUrl} sx={{ width: 120, height: 120 }}>
              {user?.username?.charAt(0).toUpperCase()}
            </Avatar>
            <IconButton
              sx={{
                position: "absolute",
                bottom: 0,
                right: 0,
                bgcolor: "primary.main",
                color: "white",
                "&:hover": { bgcolor: "primary.dark" },
              }}
              size="small"
              onClick={handleProfilePictureClick}
              disabled={uploadingImage}
            >
              <PhotoCamera fontSize="small" />
            </IconButton>
          </Box>
          <Stack spacing={1}>
            <Typography variant="body2" color="text.secondary">
              Dozwolone formaty: JPG, PNG, GIF, WebP
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Maksymalny rozmiar: 5MB
            </Typography>
            <Stack direction="row" spacing={1}>
              {selectedFile && (
                <Button
                  variant="contained"
                  color="primary"
                  size="small"
                  onClick={handleSaveProfilePicture}
                  disabled={uploadingImage}
                >
                  Zapisz zdjęcie
                </Button>
              )}
              {user?.profilePictureUrl && !selectedFile && (
                <Button
                  variant="outlined"
                  color="error"
                  size="small"
                  startIcon={<DeleteIcon />}
                  onClick={handleDeleteProfilePicture}
                  disabled={uploadingImage}
                >
                  Usuń zdjęcie
                </Button>
              )}
            </Stack>
          </Stack>
          <input
            ref={fileInputRef}
            type="file"
            accept="image/jpeg,image/png,image/gif,image/webp"
            style={{ display: "none" }}
            onChange={handleFileChange}
          />
        </Stack>
      </Paper>
    );
  };

  const renderMeasurements = () => (
    <Box>
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

        <FormControl>
          <InputLabel id="fitness-goal-label">Cel fitness</InputLabel>
          <Select
            labelId="fitness-goal-label"
            label="Cel fitness"
            value={fitnessGoal}
            onChange={(e) => setFitnessGoal(e.target.value as FitnessGoal)}
          >
            {FITNESS_GOAL_OPTIONS.map(
              ({ value, label, description, adjustmentNote }) => (
                <MenuItem key={value} value={value}>
                  <Box>
                    <Typography variant="body2" fontWeight={600}>
                      {label}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {description} · {adjustmentNote}
                    </Typography>
                  </Box>
                </MenuItem>
              )
            )}
          </Select>
          <FormHelperText>
            Wybierz cel, aby przeliczyć kalorie i makroskładniki.
          </FormHelperText>
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
    </Box>
  );

  const renderMacroGoals = () => {
    const macroItems = [
      {
        key: "calories",
        label: "Dzienne kalorie",
        value: macroGoals.calories,
        unit: "kcal",
      },
      {
        key: "protein",
        label: "Białko",
        value: macroGoals.protein,
        unit: "g",
      },
      {
        key: "carbs",
        label: "Węglowodany",
        value: macroGoals.carbs,
        unit: "g",
      },
      { key: "fat", label: "Tłuszcze", value: macroGoals.fat, unit: "g" },
    ];
    const hasMacroData = macroItems.some((item) => item.value !== null);

    return (
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Dzienne makroskładniki
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Wartości pochodzą z Twojego celu fitness i posłużą do planowania
          posiłków.
        </Typography>
        {hasMacroData ? (
          <Box
            sx={{
              display: "grid",
              gap: 2,
              gridTemplateColumns: {
                sm: "repeat(2, minmax(0, 1fr))",
                xs: "1fr",
              },
            }}
          >
            {macroItems.map((item) => (
              <Box
                key={item.key}
                sx={(theme) => ({
                  border: `1px solid ${theme.palette.divider}`,
                  borderRadius: 2,
                  p: 2,
                })}
              >
                <Typography variant="body2" color="text.secondary">
                  {item.label}
                </Typography>
                <Typography
                  variant="h5"
                  fontWeight={700}
                  color={item.value !== null ? "text.primary" : "text.disabled"}
                >
                  {item.value !== null ? `${item.value} ${item.unit}` : "—"}
                </Typography>
              </Box>
            ))}
          </Box>
        ) : (
          <Alert severity="info">
            Zapisz pomiary i wybierz cel fitness, aby zobaczyć wyliczone
            wartości makro.
          </Alert>
        )}
      </Paper>
    );
  };

  const renderDemandCard = () => {
    const hasCalculatedValues =
      calculatedBMR !== null && calculatedDailyCalories !== null;
    const persistedGender = mapBackendGenderToLocal(
      persistedPreferences?.gender
    );
    const genderOffsetLabel =
      persistedGender === "female"
        ? "−161"
        : persistedGender === "male"
          ? "+5"
          : "+5/−161";
    const activityLabel = persistedActivityOption?.label;

    return (
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
                Podstawowa przemiana materii (BMR, Basal Metabolic Rate)
                obliczana jest ze wzoru Mifflin-St Jeor: BMR = (10×W + 6.25×H −
                5×A + S), gdzie W to masa ciała w kilogramach, H wzrost w
                centymetrach, A wiek w latach, a S to stała zależna od płci (+5
                dla mężczyzn, −161 dla kobiet).
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Backend mnoży BMR przez PAL, a następnie dostosowuje kalorie do
                wybranego celu (redukcja/utrzymanie/masa). Docelowe kalorie mogą
                więc być niższe od BMR przy redukcji.
              </Typography>
              {hasPersistedMeasurements && hasCalculatedValues && (
                <>
                  <Typography variant="body2" color="text.secondary">
                    BMR z backendu: {calculatedBMR} kcal.
                  </Typography>
                  {rawTdee !== null && (
                    <Typography variant="body2" color="text.secondary">
                      TDEE (BMR × PAL) przed uwzględnieniem celu: {rawTdee}{" "}
                      kcal/dzień.
                    </Typography>
                  )}
                  {calorieTarget !== null && (
                    <Typography variant="body2" color="text.secondary">
                      Kalorie zapisane po uwzględnieniu celu: {calorieTarget}{" "}
                      kcal/dzień.
                    </Typography>
                  )}
                  {palDisplay && (
                    <Typography variant="body2" color="text.secondary">
                      Przyjęty PAL: {palDisplay}
                      {activityLabel ? ` (${activityLabel})` : ""}
                    </Typography>
                  )}
                  {isTargetBelowBmr && (
                    <Typography variant="body2" color="warning.main">
                      Cel kaloryczny jest niższy niż BMR (deficyt z wybranego
                      celu).
                    </Typography>
                  )}
                </>
              )}
            </Stack>
          </Collapse>
        </Stack>
        {!hasPersistedMeasurements ? (
          <Alert severity="info">
            Uzupełnij sekcję „Pomiary”, aby zobaczyć dane zapisane w bazie.
          </Alert>
        ) : !hasCalculatedValues ? (
          <Alert severity="warning">
            Zapisz pomiary, aby backend przeliczył zapotrzebowanie energetyczne.
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
                  {calculatedBMR ?? "—"} kcal
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  10×W + 6.25×H − 5×A + {genderOffsetLabel}
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
                  Kalorie po uwzględnieniu celu
                </Typography>
                <Typography
                  variant="h4"
                  fontWeight={900}
                  color="secondary.main"
                >
                  {calorieTarget ?? "—"} kcal
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {isCalorieTargetAdjusted && rawTdee !== null
                    ? `TDEE przed celem: ${rawTdee} kcal`
                    : palDisplay
                      ? `PAL ${palDisplay}${
                          activityLabel ? ` (${activityLabel})` : ""
                        }`
                      : (activityLabel ?? "Poziom aktywności z profilu")}
                </Typography>
              </Box>
            </Stack>
          </Stack>
        )}
      </Paper>
    );
  };

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
        {activeTab === "profil" && renderProfilePicture()}
        {(activeTab === "pomiary" || !activeTab) && renderMeasurements()}
        {activeTab === "zapotrzebowanie" && (
          <>
            {renderDemandCard()}
            {renderMacroGoals()}
          </>
        )}
        {(activeTab === "alergeny" || activeTab === "preferencje") &&
          renderAllergens()}
      </Stack>
    </Box>
  );
}
