import { Paper } from "@mui/material";
import { useDashboardContext } from "../layouts/DashboardLayout";
import UserProfilePage from "./UserProfilePage";
import UserAllergensManager from "../components/user/UserAllergensManager";
import UserAccountSettings from "../components/user/UserAccountSettings";

export default function UserDashboardPage() {
  const { activeTab } = useDashboardContext();

  const showProfile =
    !activeTab || activeTab === "pomiary" || activeTab === "zapotrzebowanie";

  return (
    <Paper elevation={1} sx={{ p: 3, width: "100%", maxWidth: 960 }}>
      {activeTab === "profil" && <UserAccountSettings />}

      {showProfile && <UserProfilePage />}

      {activeTab === "alergeny" && <UserAllergensManager />}
    </Paper>
  );
}
