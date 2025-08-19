import React, { useEffect, useState } from "react";
import {
  Grid,
  Card,
  TextField,
  Button,
  MenuItem,
  Alert,
  InputLabel,
  FormControl,
  Select,
  CircularProgress,
  Box,
  Typography,
} from "@mui/material";

// MD components / layout
import MDBox from "components/MDBox";
import MDTypography from "components/MDTypography";
import DashboardLayout from "examples/LayoutContainers/DashboardLayout";
import DashboardNavbar from "examples/Navbars/DashboardNavbar";
import Footer from "examples/Footer";

// API
import { insertQuestion, getAllCategories } from "api/questionApi";

function QuestionFormPage() {
  const [form, setForm] = useState({
    categoryId: "",
    questionText: "",
    option1: "",
    option2: "",
    option3: "",
    option4: "",
    correctOptionNumber: 1,
  });

  const [categories, setCategories] = useState([]);
  const [catLoading, setCatLoading] = useState(false);
  const [catError, setCatError] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState("");
  const [severity, setSeverity] = useState("success");

  // Load categories on mount
  useEffect(() => {
    const load = async () => {
      setCatLoading(true);
      setCatError("");
      try {
        const res = await getAllCategories(); // GET https://localhost:7181/api/Question/categories
        // backend returns array: [{ categoryId, categoryName }, ...]
        setCategories(res.data || []);
      } catch (err) {
        console.error("Failed to fetch categories:", err);
        setCatError("Failed to load categories. Please try again.");
      } finally {
        setCatLoading(false);
      }
    };
    load();
  }, []);

  // Auto-hide message after 3s
  useEffect(() => {
    if (message) {
      const t = setTimeout(() => setMessage(""), 3000);
      return () => clearTimeout(t);
    }
  }, [message]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    // Keep everything as string here; we’ll convert numbers on submit
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const canSubmit =
    form.categoryId &&
    form.questionText.trim() &&
    form.option1.trim() &&
    form.option2.trim() &&
    form.option3.trim() &&
    form.option4.trim();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage("");
    setSeverity("success");

    if (!canSubmit) {
      setSeverity("error");
      setMessage("Please fill all fields.");
      return;
    }

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
      setSubmitting(true);
      await insertQuestion(payload); // POST https://localhost:7181/api/Question/insert
      setSeverity("success");
      setMessage("✅ Question inserted!");

      // reset form
      setForm({
        categoryId: "",
        questionText: "",
        option1: "",
        option2: "",
        option3: "",
        option4: "",
        correctOptionNumber: 1,
      });
    } catch (err) {
      console.error(err);
      setSeverity("error");
      setMessage("❌ Insert failed");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <DashboardLayout>
      <DashboardNavbar />
      <MDBox mt={6} mb={3}>
        <Grid container justifyContent="center">
          <Grid item xs={12} md={8} lg={6}>
            <Card
              sx={{
                p: 5,
                boxShadow: "0px 8px 24px rgba(0,0,0,0.1)",
                borderRadius: 4,
                background: "linear-gradient(135deg, #f9f9f9, #ffffff)",
              }}
            >
              <MDTypography
                variant="h4"
                fontWeight="bold"
                textAlign="center"
                mb={4}
                sx={{
                  background: "linear-gradient(45deg, #3f51b5, #5c6bc0)",
                  WebkitBackgroundClip: "text",
                  WebkitTextFillColor: "transparent",
                }}
              >
                Create New Question
              </MDTypography>

              {/* Category Dropdown */}
              <form onSubmit={handleSubmit}>
                <MDBox display="flex" flexDirection="column" gap={3}>
                  <FormControl fullWidth required size="medium">
                    <InputLabel
                      id="category-select-label"
                      sx={{ fontSize: "1.05rem", fontWeight: 600 }}
                    >
                      Select Category
                    </InputLabel>
                    <Select
                      labelId="category-select-label"
                      name="categoryId"
                      value={form.categoryId}
                      onChange={handleChange}
                      label="Select Category"
                      sx={{ borderRadius: 2, height: 55 }}
                      disabled={catLoading}
                    >
                      {catLoading && (
                        <MenuItem disabled>
                          <Box display="flex" alignItems="center" gap={1}>
                            <CircularProgress size={18} />
                            <Typography variant="body2">Loading...</Typography>
                          </Box>
                        </MenuItem>
                      )}
                      {!catLoading && categories.length === 0 && (
                        <MenuItem disabled>No categories found</MenuItem>
                      )}
                      {!catLoading &&
                        categories.map((cat) => (
                          <MenuItem key={cat.categoryId} value={cat.categoryId}>
                            {cat.categoryName}
                          </MenuItem>
                        ))}
                    </Select>
                  </FormControl>

                  {catError && <Alert severity="error">{catError}</Alert>}

                  {/* Question text */}
                  <TextField
                    label="Question Text"
                    name="questionText"
                    value={form.questionText}
                    onChange={handleChange}
                    required
                    fullWidth
                  />

                  {/* Options 1-4 */}
                  {[1, 2, 3, 4].map((num) => (
                    <TextField
                      key={num}
                      label={`Option ${num}`}
                      name={`option${num}`}
                      value={form[`option${num}`]}
                      onChange={handleChange}
                      required
                      fullWidth
                    />
                  ))}

                  {/* Correct Option Dropdown */}
                  <FormControl fullWidth required size="medium">
                    <InputLabel
                      id="correct-option-label"
                      sx={{ fontSize: "1.05rem", fontWeight: 600 }}
                    >
                      Correct Option
                    </InputLabel>
                    <Select
                      labelId="correct-option-label"
                      name="correctOptionNumber"
                      value={form.correctOptionNumber}
                      onChange={handleChange}
                      label="Correct Option"
                      sx={{ borderRadius: 2, height: 55 }}
                    >
                      <MenuItem value={1}>Option 1</MenuItem>
                      <MenuItem value={2}>Option 2</MenuItem>
                      <MenuItem value={3}>Option 3</MenuItem>
                      <MenuItem value={4}>Option 4</MenuItem>
                    </Select>
                  </FormControl>

                  {/* Submit */}
                  <Button
                    type="submit"
                    variant="contained"
                    fullWidth
                    size="large"
                    disabled={!canSubmit || submitting || catLoading}
                    sx={{
                      mt: 1,
                      textTransform: "none",
                      fontWeight: "bold",
                      borderRadius: 3,
                      height: 55,
                      fontSize: "1.1rem",
                      background: "linear-gradient(45deg, #3f51b5, #5c6bc0)",
                      color: "#fff",
                      boxShadow: "0px 4px 14px rgba(63,81,181,0.4)",
                      "&:hover": {
                        background: "linear-gradient(45deg, #303f9f, #3949ab)",
                      },
                    }}
                  >
                    {submitting ? (
                      <CircularProgress size={24} sx={{ color: "#fff" }} />
                    ) : (
                      "Insert Question"
                    )}
                  </Button>

                  {message && (
                    <Alert severity={severity} sx={{ mt: 2, borderRadius: 2 }}>
                      {message}
                    </Alert>
                  )}
                </MDBox>
              </form>
            </Card>
          </Grid>
        </Grid>
      </MDBox>
      <Footer />
    </DashboardLayout>
  );
}

export default QuestionFormPage;
