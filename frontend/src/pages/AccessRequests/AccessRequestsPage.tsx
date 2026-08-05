import { useEffect, useState } from "react";
import {
  Alert,
  Box,
  Chip,
  CircularProgress,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TablePagination,
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

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    async function loadRequests() {
      setLoading(true);
      setError("");

      try {
        const data = await getAllAccessRequests(
          page,
          pageSize
        );

        setRequests(data.items);
        setTotalCount(data.totalCount);
      } catch {
        setError("Failed to load access requests.");
      } finally {
        setLoading(false);
      }
    }

    loadRequests();
  }, [page, pageSize]);

  if (loading) {
    return (
      <Box
        sx={{
          display: "flex",
          justifyContent: "center",
          py: 8,
        }}
      >
        <CircularProgress />
      </Box>
    );
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
              <TableCell>Requested By</TableCell>
              <TableCell>Asset</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Created At</TableCell>
            </TableRow>
          </TableHead>

          <TableBody>
            {requests.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={4}
                  align="center"
                  sx={{ py: 4 }}
                >
                  No access requests found.
                </TableCell>
              </TableRow>
            ) : (
              requests.map((request) => (
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
              ))
            )}
          </TableBody>
        </Table>

        <TablePagination
          component="div"
          count={totalCount}
          page={page - 1}
          rowsPerPage={pageSize}
          rowsPerPageOptions={[5, 10, 25, 50]}
          onPageChange={(_, newPage) => {
            setPage(newPage + 1);
          }}
          onRowsPerPageChange={(event) => {
            setPageSize(Number(event.target.value));
            setPage(1);
          }}
        />
      </Paper>
    </Box>
  );
}