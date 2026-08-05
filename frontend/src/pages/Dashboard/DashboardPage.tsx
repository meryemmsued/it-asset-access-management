import { useEffect, useState } from "react";
import {
  Alert,
  Box,
  Card,
  CardContent,
  CircularProgress,
  Grid,
  Paper,
  Typography,
} from "@mui/material";
import { getDashboardSummary } from "../../services/dashboardService";
import type { DashboardSummary } from "../../types/dashboard";

export default function DashboardPage() {

  const [dashboard, setDashboard] =
    useState<DashboardSummary | null>(null);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadDashboard() {
      try {
        const data = await getDashboardSummary();
        setDashboard(data);
      } catch {
        setError("Dashboard data could not be loaded.");
      } finally {
        setLoading(false);
      }
    }

    loadDashboard();
  }, []);


  if (loading) {
    return (
      <Box
        sx={{
          minHeight: "100vh",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
        }}
      >
        <CircularProgress />
      </Box>
    );
  }

  if (error || !dashboard) {
    return (
      <Box sx={{ p: 4 }}>
        <Alert severity="error">
          {error || "Dashboard data is unavailable."}
        </Alert>
      </Box>
    );
  }

  const cards = [
    {
      title: "Total Users",
      value: dashboard.totalUsers,
    },
    {
      title: "Active Users",
      value: dashboard.activeUsers,
    },
    {
      title: "Total Assets",
      value: dashboard.totalAssets,
    },
    {
      title: "Available Assets",
      value: dashboard.availableAssets,
    },
    {
      title: "Assigned Assets",
      value: dashboard.assignedAssets,
    },
    {
      title: "Pending Requests",
      value: dashboard.pendingAccessRequests,
    },
    {
      title: "Approved Requests",
      value: dashboard.approvedAccessRequests,
    },
    {
      title: "Rejected Requests",
      value: dashboard.rejectedAccessRequests,
    },
  ];

  return (
    <Box
      sx={{
        minHeight: "100vh",
        backgroundColor: "#f5f7fb",
        p: 4,
      }}
    >
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 4,
        }}
      >
        <Box>
          <Typography
            variant="h3"
            sx={{ fontWeight: 700 }}
          >
            Dashboard
          </Typography>

          <Typography color="text.secondary">
            Overview of users, assets and access requests
          </Typography>
        </Box>
      </Box>

      <Grid container spacing={3}>
        {cards.map((card) => (
          <Grid
            key={card.title}
            size={{
              xs: 12,
              sm: 6,
              md: 3,
            }}
          >
            <Card
              sx={{
                borderRadius: 3,
                height: "100%",
              }}
            >
              <CardContent>
                <Typography
                  color="text.secondary"
                  sx={{ mb: 1 }}
                >
                  {card.title}
                </Typography>

                <Typography
                  variant="h4"
                  sx={{ fontWeight: 700 }}
                >
                  {card.value}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Paper
        sx={{
          mt: 4,
          p: 3,
          borderRadius: 3,
        }}
      >
        <Typography
          variant="h5"
          sx={{
            fontWeight: 700,
            mb: 2,
          }}
        >
          Recent Audit Logs
        </Typography>

        {dashboard.recentAuditLogs.length === 0 ? (
          <Typography color="text.secondary">
            No audit logs found.
          </Typography>
        ) : (
          dashboard.recentAuditLogs.map((log) => (
            <Box
              key={log.id}
              sx={{
                py: 2,
                borderBottom: "1px solid",
                borderColor: "divider",
              }}
            >
              <Typography sx={{ fontWeight: 600 }}>
                {log.action}
              </Typography>

              <Typography
                variant="body2"
                color="text.secondary"
              >
                {log.entityType} ·{" "}
                {new Date(log.createdAt).toLocaleString()}
              </Typography>
            </Box>
          ))
        )}
      </Paper>
    </Box>
  );
}