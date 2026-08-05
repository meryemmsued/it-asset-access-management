import { useEffect, useState } from "react";
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
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
  getAuditLogById,
  getAuditLogs,
} from "../../services/auditLogService";

import type { AuditLog } from "../../types/auditLog";

export default function AuditLogsPage() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [selectedLog, setSelectedLog] =
    useState<AuditLog | null>(null);

  const [loading, setLoading] = useState(true);
  const [detailLoading, setDetailLoading] =
    useState(false);

  const [error, setError] = useState("");
  const [detailError, setDetailError] = useState("");

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  const [detailDialogOpen, setDetailDialogOpen] =
    useState(false);

  useEffect(() => {
    async function loadAuditLogs() {
      setLoading(true);
      setError("");

      try {
        const data = await getAuditLogs(
          page,
          pageSize
        );

        setLogs(data.items);
        setTotalCount(data.totalCount);
      } catch {
        setError("Failed to load audit logs.");
      } finally {
        setLoading(false);
      }
    }

    loadAuditLogs();
  }, [page, pageSize]);

  async function handleViewDetails(id: string) {
    setDetailDialogOpen(true);
    setDetailLoading(true);
    setDetailError("");
    setSelectedLog(null);

    try {
      const log = await getAuditLogById(id);
      setSelectedLog(log);
    } catch {
      setDetailError(
        "Audit log details could not be loaded."
      );
    } finally {
      setDetailLoading(false);
    }
  }

  function handleCloseDetails() {
    setDetailDialogOpen(false);
    setSelectedLog(null);
    setDetailError("");
  }

  function formatDate(value: string) {
    return new Date(value).toLocaleString();
  }

  function formatJson(value: string | null) {
    if (!value) {
      return "No data";
    }

    try {
      return JSON.stringify(
        JSON.parse(value),
        null,
        2
      );
    } catch {
      return value;
    }
  }

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
      <Box sx={{ mb: 3 }}>
        <Typography
          variant="h4"
          sx={{ fontWeight: 700 }}
        >
          Audit Logs
        </Typography>

        <Typography color="text.secondary">
          Review recorded system actions and changes
        </Typography>
      </Box>

      <Paper>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Action</TableCell>
              <TableCell>Entity Type</TableCell>
              <TableCell>User ID</TableCell>
              <TableCell>Created At</TableCell>
              <TableCell align="right">
                Actions
              </TableCell>
            </TableRow>
          </TableHead>

          <TableBody>
            {logs.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={5}
                  align="center"
                  sx={{ py: 4 }}
                >
                  No audit logs found.
                </TableCell>
              </TableRow>
            ) : (
              logs.map((log) => (
                <TableRow key={log.id} hover>
                  <TableCell>
                    <Chip
                      label={log.action}
                      size="small"
                      variant="outlined"
                    />
                  </TableCell>

                  <TableCell>
                    {log.entityType}
                  </TableCell>

                  <TableCell>
                    {log.userId ?? "System"}
                  </TableCell>

                  <TableCell>
                    {formatDate(log.createdAt)}
                  </TableCell>

                  <TableCell align="right">
                    <Button
                      size="small"
                      onClick={() =>
                        handleViewDetails(log.id)
                      }
                    >
                      View Details
                    </Button>
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

      <Dialog
        open={detailDialogOpen}
        onClose={handleCloseDetails}
        fullWidth
        maxWidth="md"
      >
        <DialogTitle>Audit Log Details</DialogTitle>

        <DialogContent dividers>
          {detailLoading && (
            <Box
              sx={{
                display: "flex",
                justifyContent: "center",
                py: 5,
              }}
            >
              <CircularProgress />
            </Box>
          )}

          {detailError && (
            <Alert severity="error">
              {detailError}
            </Alert>
          )}

          {selectedLog && (
            <Box
              sx={{
                display: "flex",
                flexDirection: "column",
                gap: 2,
              }}
            >
              <Box>
                <Typography
                  variant="body2"
                  color="text.secondary"
                >
                  Action
                </Typography>

                <Typography>
                  {selectedLog.action}
                </Typography>
              </Box>

              <Box>
                <Typography
                  variant="body2"
                  color="text.secondary"
                >
                  Entity Type
                </Typography>

                <Typography>
                  {selectedLog.entityType}
                </Typography>
              </Box>

              <Box>
                <Typography
                  variant="body2"
                  color="text.secondary"
                >
                  Entity ID
                </Typography>

                <Typography
                  sx={{ wordBreak: "break-all" }}
                >
                  {selectedLog.entityId ?? "Not available"}
                </Typography>
              </Box>

              <Box>
                <Typography
                  variant="body2"
                  color="text.secondary"
                >
                  User ID
                </Typography>

                <Typography
                  sx={{ wordBreak: "break-all" }}
                >
                  {selectedLog.userId ?? "System"}
                </Typography>
              </Box>

              <Box>
                <Typography
                  variant="body2"
                  color="text.secondary"
                >
                  IP Address
                </Typography>

                <Typography>
                  {selectedLog.ipAddress ??
                    "Not recorded"}
                </Typography>
              </Box>

              <Box>
                <Typography
                  variant="body2"
                  color="text.secondary"
                >
                  User Agent
                </Typography>

                <Typography
                  sx={{ wordBreak: "break-word" }}
                >
                  {selectedLog.userAgent ??
                    "Not recorded"}
                </Typography>
              </Box>

              <Box>
                <Typography
                  variant="body2"
                  color="text.secondary"
                >
                  Created At
                </Typography>

                <Typography>
                  {formatDate(selectedLog.createdAt)}
                </Typography>
              </Box>

              <Box>
                <Typography
                  variant="body2"
                  color="text.secondary"
                  sx={{ mb: 1 }}
                >
                  Old Values
                </Typography>

                <Box
                  component="pre"
                  sx={{
                    m: 0,
                    p: 2,
                    borderRadius: 2,
                    backgroundColor: "grey.100",
                    overflowX: "auto",
                    whiteSpace: "pre-wrap",
                    wordBreak: "break-word",
                    fontFamily: "monospace",
                    fontSize: 13,
                  }}
                >
                  {formatJson(selectedLog.oldValues)}
                </Box>
              </Box>

              <Box>
                <Typography
                  variant="body2"
                  color="text.secondary"
                  sx={{ mb: 1 }}
                >
                  New Values
                </Typography>

                <Box
                  component="pre"
                  sx={{
                    m: 0,
                    p: 2,
                    borderRadius: 2,
                    backgroundColor: "grey.100",
                    overflowX: "auto",
                    whiteSpace: "pre-wrap",
                    wordBreak: "break-word",
                    fontFamily: "monospace",
                    fontSize: 13,
                  }}
                >
                  {formatJson(selectedLog.newValues)}
                </Box>
              </Box>
            </Box>
          )}
        </DialogContent>

        <DialogActions>
          <Button onClick={handleCloseDetails}>
            Close
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}