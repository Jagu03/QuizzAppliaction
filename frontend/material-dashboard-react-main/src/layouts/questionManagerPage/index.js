// src/pages/QuestionManager.jsx
import React, { useEffect, useMemo, useState } from "react";
import {
  Card,
  Grid,
  Button,
  TextField,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  IconButton,
  MenuItem,
  Select,
  InputLabel,
  FormControl,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Typography,
  Tooltip,
  Paper,
  Box,
  Checkbox,
} from "@mui/material";
import { keyframes } from "@emotion/react";
import { Edit, Delete, Search } from "@mui/icons-material";
import DashboardLayout from "examples/LayoutContainers/DashboardLayout";
import DashboardNavbar from "examples/Navbars/DashboardNavbar";
import Footer from "examples/Footer";
import {
  fetchQuestions,
  getAllCategories,
  insertQuestion,
  updateQuestion,
  deleteQuestion,
} from "api/questionApi";

const CONTROL_HEIGHT = 48;

const gradientSlide = keyframes`
  0% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
  100% { background-position: 0% 50%; }
`;

const sxGradientText = (from, to) => ({
  background: `linear-gradient(90deg, ${from}, ${to})`,
  WebkitBackgroundClip: "text",
  WebkitTextFillColor: "transparent",
});

function QuestionManager() {
  const [categories, setCategories] = useState([]);
  const [catLoading, setCatLoading] = useState(false);

  const [rows, setRows] = useState([]); // raw payload (grouped or flat)
  const [listLoading, setListLoading] = useState(false);

  const [filterCat, setFilterCat] = useState("");
  const [searchText, setSearchText] = useState("");

  const [editingId, setEditingId] = useState(0);
  const [form, setForm] = useState({
    categoryId: "",
    questionText: "",
    option1: "",
    option2: "",
    option3: "",
    option4: "",
    correctOptionNumber: 1,
  });

  const [msg, setMsg] = useState("");
  const [sev, setSev] = useState("success");
  const [confirm, setConfirm] = useState({ open: false, id: 0 });
  const [selected, setSelected] = useState([]); // checkboxes

  // Auto-hide alerts
  useEffect(() => {
    if (!msg) return;
    const t = setTimeout(() => setMsg(""), 2500);
    return () => clearTimeout(t);
  }, [msg]);

  // Load categories
  useEffect(() => {
    (async () => {
      try {
        setCatLoading(true);
        const res = await getAllCategories();
        setCategories(res.data || []);
      } catch {
        setSev("error");
        setMsg("Failed to load categories");
      } finally {
        setCatLoading(false);
      }
    })();
  }, []);

  // Load questions
  const loadList = async (categoryId) => {
    setListLoading(true);
    try {
      const res = await fetchQuestions(categoryId || undefined);
      setRows(res.data || []);
    } catch {
      setSev("error");
      setMsg("Failed to load questions");
    } finally {
      setListLoading(false);
    }
  };

  useEffect(() => {
    setSelected([]);
    loadList(filterCat || undefined);
  }, [filterCat]);

  // Normalize payload into consistent shape (handles grouped OR flat API)
  const questions = useMemo(() => {
    const text = searchText.toLowerCase();

    // grouped when we see an object with an 'options' array
    const looksGrouped = Array.isArray(rows) && rows.some((r) => Array.isArray(r?.options));

    let data = [];

    if (looksGrouped) {
      // Normalize grouped: filter by search, clean/sort options
      data = rows
        .filter(
          (q) =>
            !text ||
            String(q.questionText || "")
              .toLowerCase()
              .includes(text)
        )
        .map((q) => ({
          questionId: q.questionId,
          questionText: q.questionText,
          categoryId: q.categoryId,
          categoryName: q.categoryName,
          correctOptionId: q.correctOptionId,
          // sort by optionId & drop blanks
          options: (q.options || [])
            .filter((o) => o && String(o.optionText || "").trim() !== "")
            .sort((a, b) => (a.optionId ?? 0) - (b.optionId ?? 0)),
        }));
    } else {
      // Flat -> group by questionId
      const map = new Map();
      for (const r of rows) {
        if (
          text &&
          !String(r.questionText || "")
            .toLowerCase()
            .includes(text)
        )
          continue;
        let q = map.get(r.questionId);
        if (!q) {
          q = {
            questionId: r.questionId,
            questionText: r.questionText,
            categoryId: r.categoryId,
            categoryName: r.categoryName,
            correctOptionId: r.correctOptionId,
            options: [],
          };
          map.set(r.questionId, q);
        }
        q.options.push({ optionId: r.optionId, optionText: r.optionText });
      }
      data = [...map.values()].map((q) => ({
        ...q,
        options: (q.options || [])
          .filter((o) => o && String(o.optionText || "").trim() !== "")
          .sort((a, b) => (a.optionId ?? 0) - (b.optionId ?? 0)),
      }));
    }

    return data;
  }, [rows, searchText]);

  // Helpers
  const startCreate = () => {
    setEditingId(0);
    setForm({
      categoryId: "",
      questionText: "",
      option1: "",
      option2: "",
      option3: "",
      option4: "",
      correctOptionNumber: 1,
    });
  };

  const startEdit = (q) => {
    // q.options already normalized & sorted
    setEditingId(q.questionId);
    setForm({
      categoryId: q.categoryId,
      questionText: q.questionText,
      option1: q.options?.[0]?.optionText || "",
      option2: q.options?.[1]?.optionText || "",
      option3: q.options?.[2]?.optionText || "",
      option4: q.options?.[3]?.optionText || "",
      // map correctOptionId to 1..4
      correctOptionNumber:
        (q.options?.findIndex((o) => o.optionId === q.correctOptionId) ?? -1) + 1 || 1,
    });
  };

  const canSubmit =
    form.categoryId &&
    form.questionText.trim() &&
    form.option1.trim() &&
    form.option2.trim() &&
    form.option3.trim() &&
    form.option4.trim();

  const submit = async () => {
    const payload = {
      CategoryId: parseInt(form.categoryId, 10),
      QuestionText: form.questionText.trim(),
      Option1: form.option1.trim(),
      Option2: form.option2.trim(),
      Option3: form.option3.trim(),
      Option4: form.option4.trim(),
      CorrectOptionNumber: parseInt(form.correctOptionNumber, 10),
    };
    try {
      if (editingId === 0) {
        await insertQuestion(payload);
        setSev("success");
        setMsg("Inserted");
      } else {
        await updateQuestion({ QuestionId: editingId, ...payload });
        setSev("success");
        setMsg("Updated");
      }
      startCreate();
      await loadList(filterCat || undefined);
    } catch (e) {
      console.error(e);
      setSev("error");
      setMsg("Save failed");
    }
  };

  const doDelete = async () => {
    try {
      const res = await deleteQuestion(confirm.id);
      setSev("success");
      setMsg(res?.data?.result || "Deleted");
      setConfirm({ open: false, id: 0 });
      await loadList(filterCat || undefined);
    } catch (e) {
      console.error(e);
      setSev("error");
      setMsg("Delete failed");
    }
  };

  // selection (checkboxes)
  const allIds = questions.map((q) => q.questionId);
  const allChecked = selected.length > 0 && selected.length === allIds.length;
  const indeterminate = selected.length > 0 && selected.length < allIds.length;
  const toggleAll = (e) => setSelected(e.target.checked ? allIds : []);
  const toggleOne = (id) =>
    setSelected((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));

  // helper for the “1. … • 2. …” line (no undefined)
  const optionsLine = (q) =>
    (q.options || [])
      .filter((o) => o?.optionText?.trim())
      .map((o, i) => `${i + 1}. ${o.optionText}`)
      .join("  •  ");

  return (
    <DashboardLayout>
      <DashboardNavbar />

      <Grid container justifyContent="center" mt={4} mb={3}>
        <Grid item xs={12} md={11} lg={10}>
          {msg && (
            <Alert
              severity={sev}
              sx={{ mb: 2, borderRadius: 2, boxShadow: "0 6px 20px rgba(0,0,0,0.08)" }}
            >
              {msg}
            </Alert>
          )}

          {/* FILTER (white card) */}
          <Card
            elevation={0}
            sx={{
              p: 3,
              borderRadius: 3,
              mb: 3,
              border: "1px solid #ececec",
              backgroundColor: "#fff",
            }}
          >
            <Typography
              variant="h6"
              fontWeight={800}
              mb={2}
              sx={sxGradientText("#6366f1", "#10b981")}
            >
              Filter
            </Typography>

            <Grid container spacing={2} alignItems="center">
              <Grid item xs={12} md={4}>
                <FormControl fullWidth size="small" disabled={catLoading}>
                  <InputLabel id="filter-cat">Category</InputLabel>
                  <Select
                    labelId="filter-cat"
                    value={filterCat}
                    label="Category"
                    onChange={(e) => setFilterCat(e.target.value)}
                    sx={{ height: CONTROL_HEIGHT, borderRadius: 2 }}
                  >
                    <MenuItem value="">All</MenuItem>
                    {categories.map((c) => (
                      <MenuItem key={c.categoryId} value={c.categoryId}>
                        {c.categoryName}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} md={4}>
                <TextField
                  fullWidth
                  size="small"
                  label="Search question"
                  value={searchText}
                  onChange={(e) => setSearchText(e.target.value)}
                  InputProps={{ endAdornment: <Search fontSize="small" /> }}
                  sx={{ "& .MuiInputBase-root": { height: CONTROL_HEIGHT, borderRadius: 2 } }}
                />
              </Grid>

              <Grid item xs={12} md={4} textAlign={{ xs: "left", md: "right" }}>
                <Button
                  variant="contained"
                  onClick={startCreate}
                  sx={{
                    height: CONTROL_HEIGHT,
                    borderRadius: 2,
                    px: 3,
                    fontWeight: 700,
                    background: "linear-gradient(90deg, #22c55e, #16a34a)",
                    boxShadow: "0 8px 16px rgba(34,197,94,0.25)",
                    "&:hover": { background: "linear-gradient(90deg, #16a34a, #15803d)" },
                  }}
                >
                  New Question
                </Button>
              </Grid>
            </Grid>
          </Card>

          {/* FORM */}
          <Card
            elevation={0}
            sx={{
              p: 3,
              borderRadius: 3,
              mb: 3,
              border: "1px solid #ececec",
              backgroundColor: "#fff",
            }}
          >
            <Typography
              variant="h6"
              fontWeight={800}
              mb={2}
              sx={sxGradientText("#0ea5e9", "#6366f1")}
            >
              {editingId === 0 ? "Create Question" : `Update Question #${editingId}`}
            </Typography>

            <Grid container spacing={2}>
              <Grid item xs={12} md={4}>
                <FormControl fullWidth size="small">
                  <InputLabel id="form-cat">Select Category</InputLabel>
                  <Select
                    labelId="form-cat"
                    name="categoryId"
                    label="Select Category"
                    value={form.categoryId}
                    onChange={(e) => setForm({ ...form, categoryId: e.target.value })}
                    sx={{ height: CONTROL_HEIGHT, borderRadius: 2 }}
                  >
                    {categories.map((c) => (
                      <MenuItem key={c.categoryId} value={c.categoryId}>
                        {c.categoryName}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} md={8}>
                <TextField
                  fullWidth
                  size="small"
                  label="Question Text"
                  value={form.questionText}
                  onChange={(e) => setForm({ ...form, questionText: e.target.value })}
                  sx={{ "& .MuiInputBase-root": { height: CONTROL_HEIGHT, borderRadius: 2 } }}
                />
              </Grid>

              {[1, 2, 3, 4].map((n) => (
                <Grid item xs={12} md={3} key={n}>
                  <TextField
                    fullWidth
                    size="small"
                    label={`Option ${n}`}
                    value={form[`option${n}`]}
                    onChange={(e) => setForm({ ...form, [`option${n}`]: e.target.value })}
                    sx={{ "& .MuiInputBase-root": { height: CONTROL_HEIGHT, borderRadius: 2 } }}
                  />
                </Grid>
              ))}

              <Grid item xs={12} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel id="correct-opt">Correct Option</InputLabel>
                  <Select
                    labelId="correct-opt"
                    label="Correct Option"
                    value={form.correctOptionNumber}
                    onChange={(e) => setForm({ ...form, correctOptionNumber: e.target.value })}
                    sx={{ height: CONTROL_HEIGHT, borderRadius: 2 }}
                  >
                    <MenuItem value={1}>Option 1</MenuItem>
                    <MenuItem value={2}>Option 2</MenuItem>
                    <MenuItem value={3}>Option 3</MenuItem>
                    <MenuItem value={4}>Option 4</MenuItem>
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} md={9} textAlign={{ xs: "left", md: "right" }}>
                <Button
                  onClick={submit}
                  variant="contained"
                  disabled={!canSubmit}
                  sx={{
                    height: CONTROL_HEIGHT,
                    borderRadius: 2,
                    px: 3,
                    fontWeight: 800,
                    mr: 1.5,
                    background:
                      editingId === 0
                        ? "linear-gradient(90deg, #22c55e, #16a34a)"
                        : "linear-gradient(90deg, #6366f1, #4f46e5)",
                    boxShadow:
                      editingId === 0
                        ? "0 8px 16px rgba(34,197,94,0.25)"
                        : "0 8px 16px rgba(99,102,241,0.25)",
                    "&:hover": {
                      background:
                        editingId === 0
                          ? "linear-gradient(90deg, #16a34a, #15803d)"
                          : "linear-gradient(90deg, #4f46e5, #4338ca)",
                    },
                  }}
                >
                  {editingId === 0 ? "Insert Question" : "Update Question"}
                </Button>

                <Button
                  variant="outlined"
                  onClick={startCreate}
                  sx={{
                    height: CONTROL_HEIGHT,
                    borderRadius: 2,
                    px: 3,
                    fontWeight: 700,
                    borderColor: "#cbd5e1",
                    color: "#475569",
                    "&:hover": { borderColor: "#94a3b8", color: "#0f172a" },
                  }}
                >
                  Clear
                </Button>
              </Grid>
            </Grid>
          </Card>

          {/* VIEW QUESTIONS */}
          <Card
            component={Paper}
            elevation={0}
            sx={{
              p: 0,
              borderRadius: 3,
              border: "1px solid #ececec",
              overflow: "hidden",
              backgroundColor: "#fff",
            }}
          >
            <Box sx={{ px: 3, py: 2, borderBottom: "1px solid #ececec", backgroundColor: "#fff" }}>
              <Typography variant="h6" fontWeight={800} sx={sxGradientText("#0ea5e9", "#6366f1")}>
                View Questions
              </Typography>
            </Box>

            {listLoading ? (
              <Grid container justifyContent="center" p={4}>
                <CircularProgress />
              </Grid>
            ) : (
              <Box sx={{ maxHeight: 520, overflow: "auto" }}>
                <Table size="small" stickyHeader>
                  <TableHead>
                    <TableRow
                      sx={{
                        "& th": {
                          backgroundColor: "#f8fafc",
                          fontWeight: 800,
                          color: "#0f172a",
                          borderBottom: "1px solid #e2e8f0",
                        },
                      }}
                    >
                      {/* <TableCell padding="checkbox">
                        <Checkbox
                          checked={selected.length > 0 && selected.length === questions.length}
                          indeterminate={selected.length > 0 && selected.length < questions.length}
                          onChange={toggleAll}
                        />
                      </TableCell> */}
                      {/* <TableCell width={70}>S.No</TableCell>
                      <TableCell width={80}>ID</TableCell>
                      <TableCell>Question</TableCell>
                      <TableCell width={220}>Category</TableCell>
                      <TableCell align="right" width={140}>
                        Actions
                      </TableCell> */}
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {questions.map((q, idx) => (
                      <TableRow
                        key={q.questionId}
                        hover
                        sx={{
                          transition: "background-color .2s ease",
                          "&:nth-of-type(odd)": { backgroundColor: "#fcfcfd" },
                          "&:hover": { backgroundColor: "#f5f7fb" },
                        }}
                      >
                        <TableCell padding="checkbox">
                          <Checkbox
                            checked={selected.includes(q.questionId)}
                            onChange={() => toggleOne(q.questionId)}
                          />
                        </TableCell>
                        <TableCell>{idx + 1}</TableCell>
                        <TableCell>{q.questionId}</TableCell>
                        <TableCell>
                          <Typography fontWeight={600} sx={{ color: "#111827" }}>
                            {q.questionText}
                          </Typography>
                          <Typography variant="caption" sx={{ color: "#64748b" }}>
                            {optionsLine(q)}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Box
                            component="span"
                            sx={{
                              display: "inline-block",
                              px: 1,
                              py: 0.25,
                              borderRadius: 2,
                              fontSize: 12,
                              fontWeight: 700,
                              color: "#065f46",
                              backgroundColor: "#d1fae5",
                              border: "1px solid #a7f3d0",
                            }}
                          >
                            {q.categoryName}
                          </Box>
                        </TableCell>
                        <TableCell align="right">
                          <Tooltip title="Edit">
                            <IconButton
                              onClick={() => startEdit(q)}
                              sx={{ "& svg": { color: "#f59e0b" }, mr: 0.5 }}
                            >
                              <Edit />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Delete">
                            <IconButton
                              color="error"
                              onClick={() => setConfirm({ open: true, id: q.questionId })}
                              sx={{ "& svg": { color: "#ef4444" } }}
                            >
                              <Delete />
                            </IconButton>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))}

                    {questions.length === 0 && (
                      <TableRow>
                        <TableCell colSpan={6} align="center" sx={{ py: 6 }}>
                          <Typography
                            fontWeight={900}
                            sx={{
                              fontSize: 18,
                              background:
                                "linear-gradient(90deg, #ec4899, #8b5cf6, #06b6d4, #22c55e)",
                              WebkitBackgroundClip: "text",
                              WebkitTextFillColor: "transparent",
                              backgroundSize: "300% 300%",
                              animation: `${gradientSlide} 5s ease infinite`,
                            }}
                          >
                            No records found
                          </Typography>
                          <Typography variant="body2" sx={{ color: "#94a3b8", mt: 0.5 }}>
                            Select a category or change keywords to see results.
                          </Typography>
                        </TableCell>
                      </TableRow>
                    )}
                  </TableBody>
                </Table>
              </Box>
            )}
          </Card>
        </Grid>
      </Grid>

      {/* DELETE CONFIRM */}
      <Dialog
        open={confirm.open}
        onClose={() => setConfirm({ open: false, id: 0 })}
        PaperProps={{ sx: { borderRadius: 3 } }}
      >
        <DialogTitle sx={{ fontWeight: 800 }}>Delete Question</DialogTitle>
        <DialogContent sx={{ color: "#475569" }}>
          Are you sure you want to delete this question? This action cannot be undone.
        </DialogContent>
        <DialogActions sx={{ p: 2.5 }}>
          <Button onClick={() => setConfirm({ open: false, id: 0 })}>Cancel</Button>
          <Button
            color="error"
            variant="contained"
            onClick={doDelete}
            sx={{ borderRadius: 2, fontWeight: 700 }}
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>

      <Footer />
    </DashboardLayout>
  );
}

export default QuestionManager;
