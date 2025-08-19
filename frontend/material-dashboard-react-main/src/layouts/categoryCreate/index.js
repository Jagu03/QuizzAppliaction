import React, { useState, useEffect, useMemo } from "react";
import {
  Grid,
  Card,
  TextField,
  Button,
  CircularProgress,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableRow,
  IconButton,
  Paper,
  Typography,
  InputAdornment,
  Box,
  Snackbar,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
} from "@mui/material";
import { Edit, Delete, Search } from "@mui/icons-material";
import DashboardLayout from "examples/LayoutContainers/DashboardLayout";
import DashboardNavbar from "examples/Navbars/DashboardNavbar";
import Footer from "examples/Footer";
import { fetchCategories, mergeCategory, deleteCategory } from "api/categoryApi";

function CategoryCreatePage() {
  const [categoryName, setCategoryName] = useState("");
  const [categoryId, setCategoryId] = useState(0);

  const [categories, setCategories] = useState([]);
  const [filterText, setFilterText] = useState("");

  const [loadingSave, setLoadingSave] = useState(false);
  const [deletingId, setDeletingId] = useState(null);
  const [initialLoading, setInitialLoading] = useState(false);

  const [successMsg, setSuccessMsg] = useState("");
  const [errorMsg, setErrorMsg] = useState("");

  const [confirmOpen, setConfirmOpen] = useState(false);
  const [toDelete, setToDelete] = useState(null);

  // fixed widths
  const COL_SN_WIDTH = 80;
  const COL_ACT_WIDTH = 120;
  const PAD = "10px 16px";

  const cellReset = {
    display: "table-cell !important",
    boxSizing: "border-box",
    padding: PAD,
    whiteSpace: "nowrap",
    verticalAlign: "middle",
    borderBottom: "1px solid rgba(224,224,224,1)",
  };

  const bodyCell = { ...cellReset };

  const loadCategories = async () => {
    setInitialLoading(true);
    try {
      const res = await fetchCategories();
      setCategories(res?.data ?? []);
    } catch {
      setErrorMsg("Failed to fetch categories.");
    } finally {
      setInitialLoading(false);
    }
  };

  useEffect(() => {
    loadCategories();
  }, []);

  const filtered = useMemo(() => {
    const q = filterText.trim().toLowerCase();
    if (!q) return categories;
    return categories.filter((c) =>
      (c.categoryName ?? "").toLowerCase().includes(q)
    );
  }, [categories, filterText]);

  const resetForm = () => {
    setCategoryId(0);
    setCategoryName("");
  };

  const handleSave = async () => {
    if (!categoryName.trim()) {
      setErrorMsg("Category name is required.");
      return;
    }
    setLoadingSave(true);
    try {
      const payload = { categoryId, categoryName: categoryName.trim() };
      const res = await mergeCategory(payload);
      setSuccessMsg(
        res?.data?.result ||
          (categoryId ? "Updated successfully." : "Created successfully.")
      );
      resetForm();
      await loadCategories();
    } catch {
      setErrorMsg("Error saving category.");
    } finally {
      setLoadingSave(false);
    }
  };

  const handleEdit = (category) => {
    setCategoryId(category.categoryId);
    setCategoryName(category.categoryName);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const askDelete = (category) => {
    setToDelete(category);
    setConfirmOpen(true);
  };

  const handleDelete = async () => {
    if (!toDelete) return;
    setDeletingId(toDelete.categoryId);
    try {
      const res = await deleteCategory(toDelete.categoryId);
      setSuccessMsg(res?.data?.result || "Deleted successfully.");
      await loadCategories();
    } catch {
      setErrorMsg("Error deleting category.");
    } finally {
      setDeletingId(null);
      setConfirmOpen(false);
      setToDelete(null);
    }
  };

  const handleClear = () => {
    resetForm();
    setFilterText("");
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter") handleSave();
  };

  return (
    <DashboardLayout>
      <DashboardNavbar />
      <Grid container justifyContent="center" mt={4}>
        <Grid item xs={12} md={10} lg={8}>
          <Card
            sx={{
              p: { xs: 2, md: 4 },
              borderRadius: 3,
              boxShadow: "0 6px 20px rgba(0,0,0,0.08)",
            }}
          >
            <Typography variant="h5" fontWeight={700} mb={2}>
              Create Category
            </Typography>

            {/* Form row */}
            <Grid container spacing={2} alignItems="center" mb={2}>
              <Grid item xs={12} md={5}>
                <Box display="flex" alignItems="center" gap={1}>
                  <Typography sx={{ fontSize: "0.95rem", fontWeight: 600, minWidth: 120 }}>
                    Category Name
                  </Typography>
                  <TextField
                    placeholder="Enter name"
                    value={categoryName}
                    onChange={(e) => setCategoryName(e.target.value)}
                    onKeyDown={handleKeyDown}
                    size="small"
                    fullWidth
                  />
                </Box>
              </Grid>

              <Grid item xs={6} md={2.5}>
                <Button
                  onClick={handleSave}
                  variant="contained"
                  fullWidth
                  disabled={loadingSave}
                  sx={{
                    fontWeight: 700,
                    textTransform: "uppercase",
                    background: "linear-gradient(90deg, #96f49bff)",
                    "&:hover": { background: "linear-gradient(90deg, #96f49bff)" },
                  }}
                >
                  {loadingSave ? (
                    <CircularProgress size={20} />
                  ) : categoryId === 0 ? (
                    "Create"
                  ) : (
                    "Update"
                  )}
                </Button>
              </Grid>

              <Grid item xs={6} md={2.5}>
                <Button
                  onClick={handleClear}
                  variant="outlined"
                  fullWidth
                  sx={{ fontWeight: 700, borderWidth: 2, "&:hover": { borderWidth: 2 } }}
                >
                  Clear
                </Button>
              </Grid>

              <Grid item xs={12} md={4}>
                <TextField
                  placeholder="Search by Category"
                  value={filterText}
                  onChange={(e) => setFilterText(e.target.value)}
                  size="small"
                  fullWidth
                  InputProps={{
                    endAdornment: (
                      <InputAdornment position="end">
                        <Search />
                      </InputAdornment>
                    ),
                  }}
                />
              </Grid>
            </Grid>

            {/* Table without headers */}
            <TableContainer component={Paper} sx={{ borderRadius: 2, overflow: "auto" }}>
              <Table
                size="small"
                sx={{
                  width: "100%",
                  tableLayout: "fixed",
                  borderCollapse: "separate",
                  borderSpacing: 0,
                }}
              >
                {/* Define column widths & keep alignment WITHOUT headers */}
                <colgroup>
                  <col style={{ width: `${COL_SN_WIDTH}px` }} />
                  <col /> {/* flexible */}
                  <col style={{ width: `${COL_ACT_WIDTH}px` }} />
                </colgroup>

                <TableBody>
                  {initialLoading ? (
                    <TableRow>
                      <TableCell colSpan={3} align="center" sx={bodyCell}>
                        <Box py={3} display="flex" alignItems="center" justifyContent="center" gap={1}>
                          <CircularProgress size={22} />
                          <Typography>Loading...</Typography>
                        </Box>
                      </TableCell>
                    </TableRow>
                  ) : filtered.length > 0 ? (
                    filtered.map((cat, idx) => (
                      <TableRow key={cat.categoryId} hover>
                        <TableCell sx={{ ...bodyCell, textAlign: "center" }}>
                          {idx + 1}
                        </TableCell>
                        <TableCell sx={bodyCell}>{cat.categoryName}</TableCell>
                        <TableCell sx={{ ...bodyCell, textAlign: "center" }}>
                          <IconButton onClick={() => handleEdit(cat)} title="Edit" size="small">
                            <Edit sx={{ color: "#f57c00" }} />
                          </IconButton>
                          <IconButton
                            onClick={() => askDelete(cat)}
                            title="Delete"
                            size="small"
                            disabled={deletingId === cat.categoryId}
                          >
                            {deletingId === cat.categoryId ? (
                              <CircularProgress size={18} />
                            ) : (
                              <Delete sx={{ color: "#f44336" }} />
                            )}
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))
                  ) : (
                    <TableRow>
                      <TableCell colSpan={3} align="center" sx={bodyCell}>
                        No categories found.
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Card>
        </Grid>
      </Grid>

      <Footer />

      {/* Snackbars */}
      <Snackbar
        open={!!successMsg}
        autoHideDuration={3000}
        onClose={() => setSuccessMsg("")}
        anchorOrigin={{ vertical: "top", horizontal: "center" }}
      >
        <Alert severity="success" onClose={() => setSuccessMsg("")} sx={{ width: "100%" }}>
          {successMsg}
        </Alert>
      </Snackbar>

      <Snackbar
        open={!!errorMsg}
        autoHideDuration={3000}
        onClose={() => setErrorMsg("")}
        anchorOrigin={{ vertical: "top", horizontal: "center" }}
      >
        <Alert severity="error" onClose={() => setErrorMsg("")} sx={{ width: "100%" }}>
          {errorMsg}
        </Alert>
      </Snackbar>

      {/* Delete Confirm */}
      <Dialog open={confirmOpen} onClose={() => setConfirmOpen(false)}>
        <DialogTitle>Delete Category</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete <strong>{toDelete?.categoryName}</strong>?
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmOpen(false)} disabled={!!deletingId}>
            Cancel
          </Button>
          <Button onClick={handleDelete} color="error" variant="contained" disabled={!!deletingId}>
            {deletingId ? <CircularProgress size={18} /> : "Delete"}
          </Button>
        </DialogActions>
      </Dialog>
    </DashboardLayout>
  );
}

export default CategoryCreatePage;
