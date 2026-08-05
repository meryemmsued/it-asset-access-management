import { Box, Toolbar } from "@mui/material";
import { Outlet } from "react-router-dom";
import Navbar from "../components/layout/Navbar";
import Sidebar, {
  drawerWidth,
} from "../components/layout/Sidebar";

export default function MainLayout() {
  return (
    <Box sx={{ display: "flex" }}>
      <Sidebar />
      <Navbar />

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          width: `calc(100% - ${drawerWidth}px)`,
          minHeight: "100vh",
          backgroundColor: "#f5f7fb",
          p: 4,
        }}
      >
        <Toolbar sx={{ minHeight: 72 }} />

        <Outlet />
      </Box>
    </Box>
  );
}