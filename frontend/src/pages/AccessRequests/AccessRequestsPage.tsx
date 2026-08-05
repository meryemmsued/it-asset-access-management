import { useEffect, useState } from "react";
import {
  Alert,
  Box,
  CircularProgress,
  Chip,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";

import {
  getAllAccessRequests,
} from "../../services/accessRequestService";

import type {
  AccessRequestSummary,
} from "../../types/accessRequest";



function getStatusChip(status: number) {
  switch (status) {
    case 0:
      return (
        <Chip
          label="Pending"
          color="warning"
          size="small"
        />
      );

    case 1:
      return (
        <Chip
          label="Approved"
          color="success"
          size="small"
        />
      );

    case 2:
      return (
        <Chip
          label="Rejected"
          color="error"
          size="small"
        />
      );

    case 3:
      return (
        <Chip
          label="Cancelled"
          color="default"
          size="small"
        />
      );

    default:
      return (
        <Chip
          label="Unknown"
          size="small"
        />
      );
  }
}

export default function AccessRequestsPage() {
  const [requests, setRequests] =
    useState<AccessRequestSummary[]>([]);

  const [loading, setLoading] = useState(true);

  const [error, setError] = useState("");

  useEffect(() => {
    async function loadRequests() {
      try {
        const data =
          await getAllAccessRequests();

        setRequests(data);
      } catch {
        setError("Failed to load access requests.");
      } finally {
        setLoading(false);
      }
    }

    loadRequests();
  }, []);

  if (loading) {
    return <CircularProgress />;
  }

  if (error) {
    return <Alert severity="error">{error}</Alert>;
  }

  return (
    <Box>
      <Typography
        variant="h4"
        sx={{
          fontWeight: 700,
          mb: 3,
        }}
      >
        Access Requests
      </Typography>

      <Paper>

        <Table>

          <TableHead>

            <TableRow>

              <TableCell>
                Requested By
              </TableCell>

              <TableCell>
                Asset
              </TableCell>

              <TableCell>
                Status
              </TableCell>

              <TableCell>
                Created At
              </TableCell>

            </TableRow>

          </TableHead>

          <TableBody>

            {requests.map((request) => (

              <TableRow key={request.id}>

                <TableCell>
                  {request.requestedBy}
                </TableCell>

                <TableCell>
                  {request.assetName}
                </TableCell>

                <TableCell>
                  {getStatusChip(request.status)}
                </TableCell>

                <TableCell>
                  {new Date(
                    request.createdAt
                  ).toLocaleDateString()}
                </TableCell>

              </TableRow>

            ))}

          </TableBody>

        </Table>

      </Paper>

    </Box>
  );
}