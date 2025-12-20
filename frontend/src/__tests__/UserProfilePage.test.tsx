import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ThemeProvider, createTheme } from "@mui/material/styles";
import { vi } from "vitest";
import { within } from "@testing-library/react";
import UserProfilePage from "../pages/UserProfilePage";

const mockGetPreferences = vi.fn();
const mockUpdatePreferences = vi.fn();
const mockUseDashboardContext = vi.fn();
const mockRefreshUser = vi.fn();

vi.mock("@mui/icons-material", () => ({
  __esModule: true,
  Delete: () => <span data-testid="delete-icon" />,
  PhotoCamera: () => <span data-testid="photo-icon" />,
}));

vi.mock("../layouts/DashboardLayout", () => ({
  useDashboardContext: () => mockUseDashboardContext(),
}));

vi.mock("../contexts/AuthContext", () => ({
  useAuth: () => ({
    user: { username: "Tester", profilePictureUrl: null },
    refreshUser: mockRefreshUser,
  }),
}));

vi.mock("../services/userMeasurementsService", () => ({
  __esModule: true,
  default: {
    getPreferences: (...args: unknown[]) => mockGetPreferences(...args),
    updatePreferences: (...args: unknown[]) => mockUpdatePreferences(...args),
  },
  userMeasurementsService: {
    getPreferences: (...args: unknown[]) => mockGetPreferences(...args),
    updatePreferences: (...args: unknown[]) => mockUpdatePreferences(...args),
  },
}));

vi.mock("../services/userService", () => ({
  __esModule: true,
  default: {
    uploadProfilePicture: vi.fn(),
    deleteProfilePicture: vi.fn(),
  },
  userService: {
    uploadProfilePicture: vi.fn(),
    deleteProfilePicture: vi.fn(),
  },
}));

const renderWithTheme = () =>
  render(
    <ThemeProvider theme={createTheme()}>
      <UserProfilePage />
    </ThemeProvider>
  );

describe("UserProfilePage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseDashboardContext.mockReturnValue({ activeTab: "pomiary" });
    mockGetPreferences.mockResolvedValue({});
    vi.spyOn(window, "alert").mockImplementation(() => {});
    vi.spyOn(window, "confirm").mockImplementation(() => true);
  });

  it("blocks saving when measurements are invalid", async () => {
    const user = userEvent.setup();
    renderWithTheme();

    await waitFor(() => expect(mockGetPreferences).toHaveBeenCalled());
    await user.click(
      screen.getByRole("button", { name: /Zapisz pomiary/i })
    );

    expect(
      await screen.findByText(/Podaj prawidłowy wiek/i)
    ).toBeInTheDocument();
    expect(mockUpdatePreferences).not.toHaveBeenCalled();
  });

  it("shows calculated energy and macros when preferences are present", async () => {
    mockUseDashboardContext.mockReturnValue({ activeTab: "zapotrzebowanie" });
    mockGetPreferences.mockResolvedValue({
      age: 30,
      gender: "Male",
      weight: 80,
      height: 182,
      activityLevel: "ModeratelyActive",
      fitnessGoal: "Maintenance",
      calculatedBMR: 1700,
      calculatedDailyCalories: 2300,
      dailyCalorieGoal: 2400,
      dailyProteinGoal: 150,
      dailyCarbohydrateGoal: 250,
      dailyFatGoal: 70,
    });

    renderWithTheme();

    expect(await screen.findByText("1700 kcal")).toBeInTheDocument();
    const targetCaloriesBox = screen.getByText("Docelowe kalorie").closest("div");
    expect(targetCaloriesBox).not.toBeNull();
    expect(
      within(targetCaloriesBox as HTMLElement).getByText((content) =>
        content.replace(/\s+/g, " ").trim().startsWith("2400")
      )
    ).toBeInTheDocument();

    const dailyCaloriesBox = screen
      .getByText("Dzienne kalorie")
      .closest("div");
    expect(dailyCaloriesBox).not.toBeNull();
    expect(
      within(dailyCaloriesBox as HTMLElement).getByText((content) =>
        content.replace(/\s+/g, " ").trim().startsWith("2400")
      )
    ).toBeInTheDocument();

    expect(screen.getByText("150 g")).toBeInTheDocument();
    expect(screen.getByText("250 g")).toBeInTheDocument();
    expect(screen.getByText("70 g")).toBeInTheDocument();
  });
});
