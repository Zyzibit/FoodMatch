import {
  useCallback,
  useEffect,
  useRef,
  useState,
  type ChangeEvent,
} from "react";
import {
  Alert,
  Avatar,
  Box,
  Button,
  IconButton,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import { Delete as DeleteIcon, PhotoCamera } from "@mui/icons-material";
import { useDashboardContext } from "../layouts/DashboardLayout";
import userMeasurementsService, {
  type FoodPreferencesResponse,
} from "../services/userMeasurementsService";
import userService from "../services/userService";
import { useAuth } from "../contexts/AuthContext";
import { API_BASE_URL } from "../config";
import {
  UserMeasurementsForm,
  validateMeasurements,
  type UserMeasurementsFormData,
} from "../components/user/UserMeasurementsForm";
import { ACTIVITY_OPTIONS, type ActivityLevel } from "../components/inputs";
import {
  FITNESS_GOAL_OPTIONS,
  type FitnessGoal,
} from "../constants/fitnessGoals";

const toNumberOrNull = (value?: number | null): number | null =>
  typeof value === "number" && Number.isFinite(value) ? value : null;

const isActivityLevelValue = (value?: string): value is ActivityLevel =>
  !!value && ACTIVITY_OPTIONS.some((option) => option.value === value);

const isFitnessGoalValue = (value?: string): value is FitnessGoal =>
  !!value &&
  FITNESS_GOAL_OPTIONS.some((goalOption) => goalOption.value === value);

export default function UserProfilePage() {
  const { activeTab } = useDashboardContext();
  const { user, refreshUser } = useAuth();
  const latestPreferencesRequest = useRef(0);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const [formData, setFormData] = useState<UserMeasurementsFormData>({
    age: "",
    gender: "Male",
    weight: "",
    height: "",
    activityLevel: "ModeratelyActive",
    fitnessGoal: "Maintenance",
  });
  const [formKey, setFormKey] = useState(0);
  const [persistedPreferences, setPersistedPreferences] =
    useState<FoodPreferencesResponse | null>(null);
  const [uploadingImage, setUploadingImage] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [isSavingMeasurements, setIsSavingMeasurements] = useState(false);

  const applyPreferencesFromApi = useCallback(
    (prefs: FoodPreferencesResponse) => {
      setPersistedPreferences(prefs);
      setFormData({
        age: toNumberOrNull(prefs.age) ?? "",
        gender: prefs.gender === "Female" ? "Female" : "Male",
        weight: toNumberOrNull(prefs.weight) ?? "",
        height: toNumberOrNull(prefs.height) ?? "",
        activityLevel: isActivityLevelValue(prefs.activityLevel)
          ? prefs.activityLevel
          : "ModeratelyActive",
        fitnessGoal: isFitnessGoalValue(prefs.fitnessGoal)
          ? prefs.fitnessGoal
          : "Maintenance",
      });
      setFormKey((prev) => prev + 1);
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

  const handleSaveMeasurements = async () => {
    const validationError = validateMeasurements(formData, true);
    if (validationError) {
      setFormError(validationError);
      return;
    }

    setFormError(null);
    setIsSavingMeasurements(true);
    try {
      await userMeasurementsService.updatePreferences({
        age: formData.age as number,
        weight: formData.weight as number,
        height: formData.height as number,
        gender: formData.gender,
        activityLevel: formData.activityLevel,
        fitnessGoal: formData.fitnessGoal ?? "Maintenance",
      });

      await loadPreferences();
      alert("Pomiary zapisane");
    } catch (error) {
      console.error("Failed to save to API:", error);
      alert("Błąd podczas zapisywania pomiarów. Spróbuj ponownie.");
    } finally {
      setIsSavingMeasurements(false);
    }
  };

  const handleProfilePictureClick = () => {
    fileInputRef.current?.click();
  };

  const handleFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    const validTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];
    if (!validTypes.includes(file.type)) {
      alert("Nieprawidłowy format pliku. Dozwolone: JPG, PNG, GIF, WebP");
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      alert("Plik jest za duży. Maksymalny rozmiar to 5MB");
      return;
    }

    setSelectedFile(file);

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

  const hasPersistedMeasurements =
    typeof persistedPreferences?.age === "number" &&
    typeof persistedPreferences?.weight === "number" &&
    typeof persistedPreferences?.height === "number" &&
    !!persistedPreferences?.gender &&
    !!persistedPreferences?.activityLevel;

  const calorieTarget =
    persistedPreferences?.dailyCalorieGoal ??
    persistedPreferences?.calculatedDailyCalories ??
    null;

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
        ich podstawie backend wylicza zapotrzebowanie.
      </Typography>

      <UserMeasurementsForm
        key={formKey}
        initialData={formData}
        onChange={setFormData}
        disabled={isSavingMeasurements}
        showGoal
        error={formError}
        onErrorClose={() => setFormError(null)}
      />

      <Box sx={{ display: "flex", justifyContent: "flex-end", mt: 3 }}>
        <Button
          variant="contained"
          onClick={handleSaveMeasurements}
          disabled={isSavingMeasurements}
        >
          {isSavingMeasurements ? "Zapisywanie..." : "Zapisz pomiary"}
        </Button>
      </Box>
    </Box>
  );

  const renderDemandCard = () => {
    const hasCalculatedValues =
      (persistedPreferences?.calculatedBMR ?? null) !== null ||
      calorieTarget !== null;

    return (
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Zapotrzebowanie energetyczne
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Wszystkie obliczenia wykonuje backend — wyświetlamy zapisane wartości.
        </Typography>

        {!hasPersistedMeasurements ? (
          <Alert severity="info">
            Uzupełnij sekcję „Pomiary”, aby zobaczyć zapisane dane.
          </Alert>
        ) : !hasCalculatedValues ? (
          <Alert severity="warning">
            Zapisz pomiary, aby backend przeliczył zapotrzebowanie energetyczne.
          </Alert>
        ) : (
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
                BMR
              </Typography>
              <Typography variant="h4" fontWeight={900}>
                {persistedPreferences?.calculatedBMR ?? "—"} kcal
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Bazowy metabolizm spoczynkowy
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
                Docelowe kalorie
              </Typography>
              <Typography variant="h4" fontWeight={900} color="secondary.main">
                {calorieTarget ?? "—"} kcal
              </Typography>
              <Typography variant="caption" color="text.secondary">
                Uwzględnia poziom aktywności i wybrany cel
              </Typography>
            </Box>
          </Stack>
        )}
      </Paper>
    );
  };

  const renderMacroGoals = () => {
    const macroItems = [
      {
        key: "calories",
        label: "Dzienne kalorie",
        value: calorieTarget,
        unit: "kcal",
      },
      {
        key: "protein",
        label: "Białko",
        value: persistedPreferences?.dailyProteinGoal ?? null,
        unit: "g",
      },
      {
        key: "carbs",
        label: "Węglowodany",
        value: persistedPreferences?.dailyCarbohydrateGoal ?? null,
        unit: "g",
      },
      {
        key: "fat",
        label: "Tłuszcze",
        value: persistedPreferences?.dailyFatGoal ?? null,
        unit: "g",
      },
    ];
    const hasMacroData = macroItems.some(
      (item) => item.value !== null && item.value !== undefined
    );

    return (
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Dzienne makroskładniki
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Wartości pobieramy z ostatnich wyliczeń backendu.
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
                  {item.value ?? "—"} {item.unit}
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
      </Stack>
    </Box>
  );
}
