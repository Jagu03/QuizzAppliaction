import React, { useState, useEffect } from "react";
import Grid from "@mui/material/Grid";
import Card from "@mui/material/Card";
import TextField from "@mui/material/TextField";
import Button from "@mui/material/Button";
import CircularProgress from "@mui/material/CircularProgress";
import Alert from "@mui/material/Alert";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemText from "@mui/material/ListItemText";
import Typography from "@mui/material/Typography";
import Divider from "@mui/material/Divider";
import MenuItem from "@mui/material/MenuItem";
import Select from "@mui/material/Select";
import InputLabel from "@mui/material/InputLabel";
import FormControl from "@mui/material/FormControl";
import { keyframes } from "@emotion/react";

import MDBox from "components/MDBox";
import MDTypography from "components/MDTypography";
import DashboardLayout from "examples/LayoutContainers/DashboardLayout";
import DashboardNavbar from "examples/Navbars/DashboardNavbar";
import Footer from "examples/Footer";

import { getQuestionsByCategory, getAllCategories } from "api/questionApi";

// Animated color effect
const animateColor = keyframes`
  0%   { color: #e91e63; }
  25%  { color: #9c27b0; }
  50%  { color: #3f51b5; }
  75%  { color: #009688; }
  100% { color: #e91e63; }
`;

function CategoryQuestionsPage() {
  const [categoryId, setCategoryId] = useState("");
  const [categories, setCategories] = useState([]);
  const [questions, setQuestions] = useState([]);
  const [loading, setLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState("");

  useEffect(() => {
    const fetchCategories = async () => {
      try {
        const res = await getAllCategories();
        setCategories(res.data);
      } catch (err) {
        console.error("Failed to fetch categories", err);
      }
    };
    fetchCategories();
  }, []);

  const fetchQuestions = async () => {
    if (!categoryId) return;
    setLoading(true);
    setErrorMsg("");
    try {
      const res = await getQuestionsByCategory(categoryId);
      setQuestions(res.data);
    } catch (error) {
      console.error(error);
      setErrorMsg("Failed to fetch questions. Please try again.");
      setQuestions([]);
    }
    setLoading(false);
  };

  return (
    <DashboardLayout>
      <DashboardNavbar />
      <MDBox mt={6} mb={3}>
        <Grid container justifyContent="center">
          <Grid item xs={12} md={7} lg={5}>
            <Card
              sx={{
                p: 4,
                boxShadow: "0px 8px 24px rgba(0,0,0,0.1)",
                borderRadius: 4,
                background: "linear-gradient(135deg, #f0f0f0, #ffffff)",
                maxHeight: "80vh",
                overflow: "hidden",
              }}
            >
              <MDTypography
                variant="h4"
                fontWeight="bold"
                textAlign="center"
                mb={3}
                sx={{
                  background: "linear-gradient(45deg, #e91e63, #2196f3)",
                  WebkitBackgroundClip: "text",
                  WebkitTextFillColor: "transparent",
                }}
              >
                View Questions by Category
              </MDTypography>

              <MDBox display="flex" gap={2} mb={3}>
                <FormControl fullWidth size="medium" sx={{ minWidth: 120 }}>
                  <InputLabel
                    sx={{
                      fontSize: "1.1rem", // Increase label size
                      fontWeight: "bold",
                    }}
                  >
                    Select Category
                  </InputLabel>
                  <Select
                    value={categoryId}
                    onChange={(e) => setCategoryId(e.target.value)}
                    label="Select Category"
                    sx={{
                      borderRadius: 2,
                      height: 55, // Match Search button height
                      fontSize: "1rem",
                    }}
                  >
                    {categories.map((cat) => (
                      <MenuItem key={cat.categoryId} value={cat.categoryId}>
                        {cat.categoryName}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>

                <Button
                  onClick={fetchQuestions}
                  variant="contained"
                  size="large"
                  disabled={!categoryId || loading}
                  sx={{
                    fontWeight: "bold",
                    borderRadius: 2,
                    height: 55, // Same height as Select
                    width: "160px",
                    background: "linear-gradient(to right, #00c6ff, #0072ff)",
                    "&:hover": {
                      background: "linear-gradient(to right, #0072ff, #00c6ff)",
                    },
                  }}
                >
                  {loading ? <CircularProgress size={24} sx={{ color: "white" }} /> : "Search"}
                </Button>
              </MDBox>

              {errorMsg && (
                <Alert severity="error" sx={{ mb: 2, borderRadius: 2 }}>
                  {errorMsg}
                </Alert>
              )}

              <MDBox sx={{ maxHeight: "55vh", overflowY: "auto", pr: 1 }}>
                {questions.length > 0 ? (
                  <List>
                    {questions.map((q, idx) => (
                      <MDBox
                        key={q.questionId}
                        sx={{
                          mb: 3,
                          p: 3,
                          borderRadius: 3,
                          boxShadow: "0 4px 12px rgba(0,0,0,0.05)",
                          backgroundColor: "#fafafa",
                        }}
                      >
                        <Typography variant="h6" fontWeight="bold" gutterBottom>
                          Q{idx + 1}: {q.questionText}
                        </Typography>
                        <List dense>
                          {q.options.map((opt) => (
                            <ListItem
                              key={opt.optionId}
                              sx={{
                                backgroundColor:
                                  q.correctOptionId === opt.optionId ? "#e8f5e9" : "transparent",
                                borderRadius: 2,
                                mb: 1,
                              }}
                            >
                              <ListItemText
                                primary={
                                  <>
                                    {opt.optionText}
                                    {q.correctOptionId === opt.optionId && (
                                      <strong style={{ color: "#388e3c" }}> (Correct)</strong>
                                    )}
                                  </>
                                }
                              />
                            </ListItem>
                          ))}
                        </List>
                        <Divider sx={{ mt: 2 }} />
                        <Typography variant="body2" color="textSecondary" mt={1}>
                          Category: {q.categoryName}
                        </Typography>
                      </MDBox>
                    ))}
                  </List>
                ) : (
                  !loading && (
                    <Typography
                      variant="body1"
                      textAlign="center"
                      sx={{
                        animation: `${animateColor} 5s infinite`,
                        fontWeight: "bold",
                      }}
                    >
                      No questions found for this category.
                    </Typography>
                  )
                )}
              </MDBox>
            </Card>
          </Grid>
        </Grid>
      </MDBox>
      <Footer />
    </DashboardLayout>
  );
}

export default CategoryQuestionsPage;
