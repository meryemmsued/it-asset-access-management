import { useEffect, useState } from "react";
import {
  Alert,
  Box,
  Button,
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
  TableRow,
  TextField,
  Typography,
} from "@mui/material";

import {
  createAsset,
  getAssets,
} from "../../services/assetService";
import { getAssetCategories } from "../../services/assetCategoryService";

import type {
  Asset,
  AssetCategory,
  CreateAssetRequest,
} from "../../types/asset";

const initialForm: CreateAssetRequest = {
  assetCategoryId: "",
  assetCode: "",
  name: "",
  description: "",
  purchaseDate: null,
  purchasePrice: null,
  warrantyExpirationDate: null,

  serialNumber: null,
  manufacturer: null,
  model: null,
  location: null,
  condition: 0,

  licenseKey: null,
  version: null,
  licenseType: null,
  licenseStartDate: null,
  licenseExpirationDate: null,
  requestedAccessType: null,
  maximumUsers: null,
};

export default function AssetsPage() {
  const [assets, setAssets] = useState<Asset[]>([]);
  const [categories, setCategories] = useState<AssetCategory[]>([]);

  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);

  const [error, setError] = useState("");
  const [formError, setFormError] = useState("");

  const [searchTerm, setSearchTerm] = useState("");
  const [openDialog, setOpenDialog] = useState(false);
  const [successMessage, setSuccessMessage] = useState("");

  const [form, setForm] =
    useState<CreateAssetRequest>(initialForm);

  async function loadAssets() {
    const data = await getAssets();
    setAssets(data);
  }

  async function loadCategories() {
    const data = await getAssetCategories();
    setCategories(data);
  }

  useEffect(() => {
    async function loadPage() {
      try {
        await Promise.all([
          loadAssets(),
          loadCategories(),
        ]);
      } catch {
        setError("Failed to load asset data.");
      } finally {
        setLoading(false);
      }
    }

    loadPage();
  }, []);

  const selectedCategory = categories.find(
    (category) => category.id === form.assetCategoryId
  );

  const filteredAssets = assets.filter((asset) => {
    const search = searchTerm.toLowerCase();

    return (
      asset.assetCode.toLowerCase().includes(search) ||
      asset.name.toLowerCase().includes(search) ||
      asset.category.toLowerCase().includes(search)
    );
  });

  function handleOpenDialog() {
    setForm(initialForm);
    setFormError("");
    setOpenDialog(true);
  }

  function handleCloseDialog() {
    if (creating) {
      return;
    }

    setOpenDialog(false);
    setFormError("");
  }

  function updateForm<K extends keyof CreateAssetRequest>(
    field: K,
    value: CreateAssetRequest[K]
  ) {
    setForm((current) => ({
      ...current,
      [field]: value,
    }));
  }

  async function handleCreate() {
    setFormError("");

    if (!form.assetCategoryId) {
      setFormError("Please select an asset category.");
      return;
    }

    if (!form.assetCode.trim()) {
      setFormError("Asset code is required.");
      return;
    }

    if (!form.name.trim()) {
      setFormError("Asset name is required.");
      return;
    }

    setCreating(true);

    try {
      await createAsset(form);
      await loadAssets();

      setOpenDialog(false);
      setForm(initialForm);
      setSuccessMessage("Asset created successfully.");
    } catch {
      setFormError("Asset could not be created.");
    } finally {
      setCreating(false);
    }
  }

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
        Assets
      </Typography>

      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          gap: 2,
          mb: 3,
        }}
      >
        <TextField
          label="Search Assets"
          placeholder="Search by code, name or category..."
          value={searchTerm}
          onChange={(event) =>
            setSearchTerm(event.target.value)
          }
          sx={{
            width: 350,
          }}
        />

        <Button
          variant="contained"
          onClick={handleOpenDialog}
        >
          Add Asset
        </Button>
      </Box>

      <Paper>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Code</TableCell>
              <TableCell>Name</TableCell>
              <TableCell>Category</TableCell>
              <TableCell>Status</TableCell>
            </TableRow>
          </TableHead>

          <TableBody>
            {filteredAssets.length === 0 ? (
              <TableRow>
                <TableCell
                  colSpan={4}
                  align="center"
                >
                  No assets found.
                </TableCell>
              </TableRow>
            ) : (
              filteredAssets.map((asset) => (
                <TableRow key={asset.id}>
                  <TableCell>{asset.assetCode}</TableCell>
                  <TableCell>{asset.name}</TableCell>
                  <TableCell>{asset.category}</TableCell>
                  <TableCell>
                    {asset.status === 0
                      ? "Available"
                      : "Assigned"}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </Paper>

      <Dialog
        open={openDialog}
        onClose={handleCloseDialog}
        fullWidth
        maxWidth="md"
      >
        <DialogTitle>Create Asset</DialogTitle>

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
            label="Asset Category"
            value={form.assetCategoryId}
            onChange={(event) =>
              updateForm(
                "assetCategoryId",
                event.target.value
              )
            }
            required
          >
            {categories.map((category) => (
              <MenuItem
                key={category.id}
                value={category.id}
              >
                {category.name} ({category.assetType})
              </MenuItem>
            ))}
          </TextField>

          <TextField
            fullWidth
            margin="normal"
            label="Asset Code"
            value={form.assetCode}
            onChange={(event) =>
              updateForm("assetCode", event.target.value)
            }
            required
          />

          <TextField
            fullWidth
            margin="normal"
            label="Name"
            value={form.name}
            onChange={(event) =>
              updateForm("name", event.target.value)
            }
            required
          />

          <TextField
            fullWidth
            margin="normal"
            label="Description"
            multiline
            minRows={2}
            value={form.description}
            onChange={(event) =>
              updateForm("description", event.target.value)
            }
          />

          <TextField
            fullWidth
            margin="normal"
            label="Purchase Date"
            type="date"
            value={form.purchaseDate ?? ""}
            onChange={(event) =>
              updateForm(
                "purchaseDate",
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
            label="Purchase Price"
            type="number"
            value={form.purchasePrice ?? ""}
            onChange={(event) =>
              updateForm(
                "purchasePrice",
                event.target.value === ""
                  ? null
                  : Number(event.target.value)
              )
            }
          />

          <TextField
            fullWidth
            margin="normal"
            label="Warranty Expiration Date"
            type="date"
            value={form.warrantyExpirationDate ?? ""}
            onChange={(event) =>
              updateForm(
                "warrantyExpirationDate",
                event.target.value || null
              )
            }
            slotProps={{
                inputLabel: {
                shrink: true,
                }}}
          />

          {selectedCategory?.assetType === "Physical" && (
            <>
              <Typography
                variant="h6"
                sx={{ mt: 3 }}
              >
                Physical Asset Details
              </Typography>

              <TextField
                fullWidth
                margin="normal"
                label="Serial Number"
                value={form.serialNumber ?? ""}
                onChange={(event) =>
                  updateForm(
                    "serialNumber",
                    event.target.value || null
                  )
                }
              />

              <TextField
                fullWidth
                margin="normal"
                label="Manufacturer"
                value={form.manufacturer ?? ""}
                onChange={(event) =>
                  updateForm(
                    "manufacturer",
                    event.target.value || null
                  )
                }
              />

              <TextField
                fullWidth
                margin="normal"
                label="Model"
                value={form.model ?? ""}
                onChange={(event) =>
                  updateForm(
                    "model",
                    event.target.value || null
                  )
                }
              />

              <TextField
                fullWidth
                margin="normal"
                label="Location"
                value={form.location ?? ""}
                onChange={(event) =>
                  updateForm(
                    "location",
                    event.target.value || null
                  )
                }
              />

              <TextField
                select
                fullWidth
                margin="normal"
                label="Condition"
                value={form.condition ?? 0}
                onChange={(event) =>
                  updateForm(
                    "condition",
                    Number(event.target.value)
                  )
                }
              >
                <MenuItem value={0}>New</MenuItem>
                <MenuItem value={1}>Good</MenuItem>
                <MenuItem value={2}>Fair</MenuItem>
                <MenuItem value={3}>Poor</MenuItem>
              </TextField>
            </>
          )}

          {selectedCategory?.assetType === "Digital" && (
            <>
              <Typography
                variant="h6"
                sx={{ mt: 3 }}
              >
                Digital Asset Details
              </Typography>

              <TextField
                fullWidth
                margin="normal"
                label="License Key"
                value={form.licenseKey ?? ""}
                onChange={(event) =>
                  updateForm(
                    "licenseKey",
                    event.target.value || null
                  )
                }
              />

              <TextField
                fullWidth
                margin="normal"
                label="Version"
                value={form.version ?? ""}
                onChange={(event) =>
                  updateForm(
                    "version",
                    event.target.value || null
                  )
                }
              />

              <TextField
                fullWidth
                margin="normal"
                label="License Start Date"
                type="date"
                value={form.licenseStartDate ?? ""}
                onChange={(event) =>
                  updateForm(
                    "licenseStartDate",
                    event.target.value || null
                  )
                }
                slotProps={{
                    inputLabel: {
                    shrink: true,
                }}}
              />

              <TextField
                fullWidth
                margin="normal"
                label="License Expiration Date"
                type="date"
                value={form.licenseExpirationDate ?? ""}
                onChange={(event) =>
                  updateForm(
                    "licenseExpirationDate",
                    event.target.value || null
                  )
                }
                slotProps={{
                    inputLabel: {
                        shrink: true,
                    }   }}
            
              />

              <TextField
                fullWidth
                margin="normal"
                label="Maximum Users"
                type="number"
                value={form.maximumUsers ?? ""}
                onChange={(event) =>
                  updateForm(
                    "maximumUsers",
                    event.target.value === ""
                      ? null
                      : Number(event.target.value)
                  )
                }
              />
            </>
          )}
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
            onClick={handleCreate}
            disabled={creating}
          >
            {creating ? "Creating..." : "Create"}
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