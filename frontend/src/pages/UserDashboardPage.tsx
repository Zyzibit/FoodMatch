import { Paper, Typography } from "@mui/material";
import { useDashboardContext } from "../layouts/DashboardLayout";
import UserProfilePage from "./UserProfilePage";

export default function UserDashboardPage() {
  const { activeTab } = useDashboardContext();

  return (
    <Paper elevation={1} sx={{ p: 3, width: "100%", maxWidth: 960 }}>
      {activeTab === "pomiary" && <UserProfilePage />}

      {activeTab === "zapotrzebowanie" && (
        <Typography variant="h6">
          Zapotrzebowanie kaloryczne – w przygotowaniu
        </Typography>
      )}

      {activeTab === "alergeny" && (
        <Typography variant="h6">Alergeny – w przygotowaniu</Typography>
      )}

      {!activeTab && (
        <Typography variant="body2" color="text.secondary">
          Wybierz zakładkę, aby zobaczyć szczegóły profilu.
        </Typography>
      )}
    </Paper>
  );
}
