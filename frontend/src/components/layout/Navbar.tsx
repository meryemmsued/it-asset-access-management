import {
  AssignmentOutlined,
  DashboardOutlined,
  DevicesOutlined,
  HistoryOutlined,
  PeopleOutlined,
  SecurityOutlined,
} from "@mui/icons-material";
import {
  Box,
  Divider,
  Drawer,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography,
} from "@mui/material";
import { useLocation, useNavigate } from "react-router-dom";

export const drawerWidth = 250;

const menuItems = [
  {
    label: "Dashboard",
    path: "/dashboard",
    icon: <DashboardOutlined />,
  },
  {
    label: "Assets",
    path: "/assets",
    icon: <DevicesOutlined />,
  },
  {
    label: "Access Requests",
    path: "/access-requests",
    icon: <AssignmentOutlined />,
  },
  {
    label: "Users",
    path: "/users",
    icon: <PeopleOutlined />,
  },
  {
    label: "Roles",
    path: "/roles",
    icon: <SecurityOutlined />,
  },
  {
    label: "Audit Logs",
    path: "/audit-logs",
    icon: <HistoryOutlined />,
  },
];

export default function Sidebar() {
  const navigate = useNavigate();
  const location = useLocation();

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width: drawerWidth,
          boxSizing: "border-box",
        },
      }}
    >
      <Toolbar
        sx={{
          minHeight: 72,
          px: 3,
        }}
      >
        <Box>
          <Typography
            variant="h6"
            sx={{
              fontWeight: 800,
              lineHeight: 1.2,
            }}
          >
            IT Asset
          </Typography>

          <Typography
            variant="body2"
            color="text.secondary"
          >
            Management System
          </Typography>
        </Box>
      </Toolbar>

      <Divider />

      <List sx={{ px: 1.5, pt: 2 }}>
        {menuItems.map((item) => {
          const selected = location.pathname === item.path;

          return (
            <ListItemButton
              key={item.path}
              selected={selected}
              onClick={() => navigate(item.path)}
              sx={{
                mb: 0.5,
                borderRadius: 2,
              }}
            >
              <ListItemIcon
                sx={{
                  minWidth: 42,
                }}
              >
                {item.icon}
              </ListItemIcon>

              <ListItemText primary={item.label} />
            </ListItemButton>
          );
        })}
      </List>
    </Drawer>
  );
}