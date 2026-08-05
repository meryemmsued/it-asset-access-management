import {
  Navigate,
  Route,
  Routes,
} from "react-router-dom";
import MainLayout from "./layouts/MainLayout";
import AccessRequestsPage from "./pages/AccessRequests/AccessRequestsPage";
import AssetsPage from "./pages/Assets/AssetsPage";
import AuditLogsPage from "./pages/AuditLogs/AuditLogsPage";
import DashboardPage from "./pages/Dashboard/DashboardPage";
import LoginPage from "./pages/Login/LoginPage";
import RolesPage from "./pages/Roles/RolesPage";
import UsersPage from "./pages/Users/UsersPage";

function App() {
  const token = localStorage.getItem("accessToken");

  if (!token) {
    return (
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="*"
          element={<Navigate to="/login" replace />}
        />
      </Routes>
    );
  }

  return (
    <Routes>
      <Route
        path="/login"
        element={<Navigate to="/dashboard" replace />}
      />

      <Route element={<MainLayout />}>
        <Route
          index
          element={<Navigate to="/dashboard" replace />}
        />

        <Route
          path="/dashboard"
          element={<DashboardPage />}
        />

        <Route
          path="/assets"
          element={<AssetsPage />}
        />

        <Route
          path="/access-requests"
          element={<AccessRequestsPage />}
        />

        <Route
          path="/users"
          element={<UsersPage />}
        />

        <Route
          path="/roles"
          element={<RolesPage />}
        />

        <Route
          path="/audit-logs"
          element={<AuditLogsPage />}
        />
      </Route>

      <Route
        path="*"
        element={<Navigate to="/dashboard" replace />}
      />
    </Routes>
  );
}

export default App;