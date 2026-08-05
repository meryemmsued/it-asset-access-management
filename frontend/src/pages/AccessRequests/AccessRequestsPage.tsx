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
  MenuItem,
  Paper,
  Snackbar,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
  Typography,
} from "@mui/material";

import {
  approveAccessRequest,
  cancelAccessRequest,
  createAccessRequest,
  getAllAccessRequests,
  rejectAccessRequest,
} from "../../services/accessRequestService";

import { getAssets } from "../../services/assetService";

import type {
  AccessRequestSummary,
  CreateAccessRequestRequest,
} from "../../types/accessRequest";

import type { Asset } from "../../types/asset";

const initialForm: CreateAccessRequestRequest = {
  assetId: "",
  requestedAccessType: "",
  reason: "",
  requestedStartDate: null,
  requestedEndDate: null,
};



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

  const [assets, setAssets] = useState<Asset[]>([]);

  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [processingDecision, setProcessingDecision] =
    useState(false);

  const [error, setError] = useState("");
  const [formError, setFormError] = useState("");
  const [decisionError, setDecisionError] = useState("");

  const [openDialog, setOpenDialog] = useState(false);
  const [approveDialogOpen, setApproveDialogOpen] =
    useState(false);
  const [rejectDialogOpen, setRejectDialogOpen] =
    useState(false);

  const [selectedRequestId, setSelectedRequestId] =
    useState("");
  const [comment, setComment] = useState("");

  const [successMessage, setSuccessMessage] =
    useState("");

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);

  const [form, setForm] =
    useState<CreateAccessRequestRequest>(initialForm);

  async function loadRequests(
    requestedPage = page,
    requestedPageSize = pageSize
  ) {
    const data = await getAllAccessRequests(
      requestedPage,
      requestedPageSize
    );

    setRequests(data.items);
    setTotalCount(data.totalCount);
  }

  useEffect(() => {
    async function loadPage() {
      setLoading(true);
      setError("");

      try {
        await loadRequests(page, pageSize);
      } catch {
        setError("Failed to load access requests.");
      } finally {
        setLoading(false);
      }
    }

    loadPage();
  }, [page, pageSize]);

  async function handleOpenDialog() {
    setForm(initialForm);
    setFormError("");
    setOpenDialog(true);

    try {
      const assetData = await getAssets(1, 100);
      setAssets(assetData.items);
    } catch {
      setFormError("Assets could not be loaded.");
    }
  }

  function handleCloseDialog() {
    if (creating) {
      return;
    }

    setOpenDialog(false);
    setForm(initialForm);
    setFormError("");
  }

  function updateForm<K extends keyof CreateAccessRequestRequest>(
    field: K,
    value: CreateAccessRequestRequest[K]
  ) {
    setForm((current) => ({
      ...current,
      [field]: value,
    }));
  }

  async function handleCreateRequest() {
    setFormError("");

    if (!form.assetId) {
      setFormError("Please select an asset.");
      return;
    }

    if (!form.requestedAccessType.trim()) {
      setFormError("Access type is required.");
      return;
    }

    if (!form.reason.trim()) {
      setFormError("Reason is required.");
      return;
    }

    if (
      form.requestedStartDate &&
      form.requestedEndDate &&
      form.requestedEndDate < form.requestedStartDate
    ) {
      setFormError(
        "End date cannot be earlier than start date."
      );
      return;
    }

    setCreating(true);

    try {
      await createAccessRequest(form);

      if (page === 1) {
        await loadRequests(1, pageSize);
      } else {
        setPage(1);
      }

      setOpenDialog(false);
      setForm(initialForm);
      setSuccessMessage(
        "Access request created successfully."
      );
    } catch {
      setFormError(
        "Access request could not be created."
      );
    } finally {
      setCreating(false);
    }
  }

  function openApproveDialog(id: string) {
    setSelectedRequestId(id);
    setComment("");
    setDecisionError("");
    setApproveDialogOpen(true);
  }

  function openRejectDialog(id: string) {
    setSelectedRequestId(id);
    setComment("");
    setDecisionError("");
    setRejectDialogOpen(true);
  }

  function closeApproveDialog() {
    if (processingDecision) {
      return;
    }

    setApproveDialogOpen(false);
    setSelectedRequestId("");
    setComment("");
    setDecisionError("");
  }

  function closeRejectDialog() {
    if (processingDecision) {
      return;
    }

    setRejectDialogOpen(false);
    setSelectedRequestId("");
    setComment("");
    setDecisionError("");
  }

  async function handleApprove() {
    setDecisionError("");
    setProcessingDecision(true);

    try {
      await approveAccessRequest(
        selectedRequestId,
        {
          comment: comment.trim() || null,
        }
      );

      await loadRequests(page, pageSize);

      setApproveDialogOpen(false);
      setSelectedRequestId("");
      setComment("");

      setSuccessMessage(
        "Request approved successfully."
      );
    } catch {
      setDecisionError(
        "Request could not be approved."
      );
    } finally {
      setProcessingDecision(false);
    }
  }

  async function handleReject() {
    setDecisionError("");

    if (!comment.trim()) {
      setDecisionError(
        "Comment is required when rejecting a request."
      );
      return;
    }

    setProcessingDecision(true);

    try {
      await rejectAccessRequest(
        selectedRequestId,
        {
          comment: comment.trim(),
        }
      );

      await loadRequests(page, pageSize);

      setRejectDialogOpen(false);
      setSelectedRequestId("");
      setComment("");

      setSuccessMessage(
        "Request rejected successfully."
      );
    } catch {
      setDecisionError(
        "Request could not be rejected."
      );
    } finally {
      setProcessingDecision(false);
    }
  }


  async function handleCancel(id: string) {
    try {
      await cancelAccessRequest(id);

      await loadRequests(page, pageSize);

      setSuccessMessage(
        "Request cancelled successfully."
      );
    } catch {
      setDecisionError(
        "Request could not be cancelled."
      );
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
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 3,
        }}
      >
        <Typography
          variant="h4"
          sx={{ fontWeight: 700 }}
        >
          Access Requests
        </Typography>

        <Button
          variant="contained"
          onClick={handleOpenDialog}
        >
          New Request
        </Button>
      </Box>

      <Paper>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Requested By</TableCell>
              <TableCell>Asset</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Created At</TableCell>
              <TableCell align="center">
                Actions
              </TableCell>
            </TableRow>
          </TableHead>

          <TableBody>
            {requests.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={5}
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

                    <TableCell align="center">
                      {request.canApprove && (
                        <>
                          <Button
                            size="small"
                            color="success"
                            onClick={() =>
                              openApproveDialog(request.id)
                            }
                          >
                            Approve
                          </Button>

                          <Button
                            size="small"
                            color="error"
                            onClick={() =>
                              openRejectDialog(request.id)
                            }
                          >
                            Reject
                          </Button>
                        </>
                      )}

                      {request.canCancel && (
                        <Button
                          size="small"
                          color="warning"
                          onClick={() =>
                            handleCancel(request.id)
                          }
                        >
                          Cancel
                        </Button>
                      )}

                      {!request.canApprove &&
                        !request.canCancel && (
                          <Typography
                            variant="body2"
                            color="text.secondary"
                          >
                            No actions
                          </Typography>
                        )}
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
        open={openDialog}
        onClose={handleCloseDialog}
        fullWidth
        maxWidth="sm"
      >
        <DialogTitle>
          Create Access Request
        </DialogTitle>

        <DialogContent>
          {formError && (
            <Alert
              severity="error"
              sx={{ mt: 1, mb: 2 }}
            >
              {formError}
            </Alert>
          )}

          <TextField
            select
            fullWidth
            margin="normal"
            label="Asset"
            value={form.assetId}
            onChange={(event) =>
              updateForm("assetId", event.target.value)
            }
            required
          >
            {assets.map((asset) => (
              <MenuItem
                key={asset.id}
                value={asset.id}
              >
                {asset.assetCode} - {asset.name}
              </MenuItem>
            ))}
          </TextField>

          <TextField
            select
            fullWidth
            margin="normal"
            label="Access Type"
            value={form.requestedAccessType}
            onChange={(event) =>
              updateForm(
                "requestedAccessType",
                event.target.value
              )
            }
            required
          >
            <MenuItem value="USE">Use</MenuItem>
            <MenuItem value="READ">Read</MenuItem>
            <MenuItem value="WRITE">Write</MenuItem>
            <MenuItem value="ADMIN">Admin</MenuItem>
          </TextField>

          <TextField
            fullWidth
            margin="normal"
            label="Reason"
            multiline
            minRows={3}
            value={form.reason}
            onChange={(event) =>
              updateForm("reason", event.target.value)
            }
            required
          />

          <TextField
            fullWidth
            margin="normal"
            label="Requested Start Date"
            type="date"
            value={form.requestedStartDate ?? ""}
            onChange={(event) =>
              updateForm(
                "requestedStartDate",
                event.target.value || null
              )
            }
            slotProps={{
              inputLabel: {
                shrink: true,
              },
            }}
          />

          <TextField
            fullWidth
            margin="normal"
            label="Requested End Date"
            type="date"
            value={form.requestedEndDate ?? ""}
            onChange={(event) =>
              updateForm(
                "requestedEndDate",
                event.target.value || null
              )
            }
            slotProps={{
              inputLabel: {
                shrink: true,
              },
            }}
          />
        </DialogContent>

        <DialogActions>
          <Button
            onClick={handleCloseDialog}
            disabled={creating}
          >
            Cancel
          </Button>

          <Button
            variant="contained"
            onClick={handleCreateRequest}
            disabled={creating}
          >
            {creating ? "Submitting..." : "Submit"}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={approveDialogOpen}
        onClose={closeApproveDialog}
        fullWidth
        maxWidth="sm"
      >
        <DialogTitle>
          Approve Access Request
        </DialogTitle>

        <DialogContent>
          {decisionError && (
            <Alert
              severity="error"
              sx={{ mt: 1, mb: 2 }}
            >
              {decisionError}
            </Alert>
          )}

          <TextField
            fullWidth
            margin="normal"
            label="Comment"
            multiline
            minRows={3}
            value={comment}
            onChange={(event) =>
              setComment(event.target.value)
            }
            helperText="Comment is optional."
          />
        </DialogContent>

        <DialogActions>
          <Button
            onClick={closeApproveDialog}
            disabled={processingDecision}
          >
            Cancel
          </Button>

          <Button
            variant="contained"
            color="success"
            onClick={handleApprove}
            disabled={processingDecision}
          >
            {processingDecision
              ? "Approving..."
              : "Approve"}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog
        open={rejectDialogOpen}
        onClose={closeRejectDialog}
        fullWidth
        maxWidth="sm"
      >
        <DialogTitle>
          Reject Access Request
        </DialogTitle>

        <DialogContent>
          {decisionError && (
            <Alert
              severity="error"
              sx={{ mt: 1, mb: 2 }}
            >
              {decisionError}
            </Alert>
          )}

          <TextField
            fullWidth
            margin="normal"
            label="Comment"
            multiline
            minRows={3}
            value={comment}
            onChange={(event) =>
              setComment(event.target.value)
            }
            required
            helperText="A rejection comment is required."
          />
        </DialogContent>

        <DialogActions>
          <Button
            onClick={closeRejectDialog}
            disabled={processingDecision}
          >
            Cancel
          </Button>

          <Button
            variant="contained"
            color="error"
            onClick={handleReject}
            disabled={processingDecision}
          >
            {processingDecision
              ? "Rejecting..."
              : "Reject"}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar
        open={Boolean(successMessage)}
        autoHideDuration={3000}
        onClose={() => setSuccessMessage("")}
        message={successMessage}
      />
    </Box>
  );
}