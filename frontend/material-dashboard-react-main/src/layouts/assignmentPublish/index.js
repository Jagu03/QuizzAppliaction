import React, { useState } from "react";
import {
  Grid,
  Card,
  TextField,
  Button,
  Snackbar,
  Alert,
  Typography,
  FormControlLabel,
  Checkbox,
} from "@mui/material";
import DashboardLayout from "examples/LayoutContainers/DashboardLayout";
import DashboardNavbar from "examples/Navbars/DashboardNavbar";
import Footer from "examples/Footer";
import { publishAssignment } from "api/assignmentApi";

export default function AssignmentPublishPage() {
  const [title, setTitle] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [groupId, setGroupId] = useState("");
  const [startAt, setStartAt] = useState("");
  const [endAt, setEndAt] = useState("");
  const [timeLimitSeconds, setTimeLimitSeconds] = useState("");
  const [shuffleQ, setShuffleQ] = useState(true);
  const [shuffleO, setShuffleO] = useState(true);
  const [maxAttempts, setMaxAttempts] = useState(1);
  const [createdByStaffId, setCreatedByStaffId] = useState("");
  const [busy, setBusy] = useState(false);
  const [ok, setOk] = useState("");
  const [err, setErr] = useState("");

  const submit = async () => {
    if (!title || !categoryId || !groupId || !startAt || !endAt || !createdByStaffId) {
      setErr("Please fill all required fields.");
      return;
    }
    setBusy(true);
    try {
      const res = await publishAssignment({
        title,
        categoryId: Number(categoryId),
        groupId: Number(groupId),
        startAt, // e.g. "2025-08-10T10:00:00"
        endAt,
        timeLimitSeconds: timeLimitSeconds ? Number(timeLimitSeconds) : null,
        shuffleQuestions: shuffleQ,
        shuffleOptions: shuffleO,
        maxAttempts: Number(maxAttempts),
        createdByStaffId: Number(createdByStaffId),
      });
      const id = res?.data?.data?.assignmentId;
      setOk(`Published (ID: ${id})`);
      // reset if needed
    } catch (e) {
      setErr(e.message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <DashboardLayout>
      <DashboardNavbar />
      <Grid container justifyContent="center" mt={4}>
        <Grid item xs={12} md={10} lg={8}>
          <Card sx={{ p: 3, borderRadius: 3 }}>
            <Typography variant="h5" fontWeight={700} mb={2}>
              Publish Assignment
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={12} md={6}>
                <TextField
                  label="Title"
                  fullWidth
                  size="small"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                />
              </Grid>
              <Grid item xs={6} md={3}>
                <TextField
                  label="Category Id"
                  fullWidth
                  size="small"
                  value={categoryId}
                  onChange={(e) => setCategoryId(e.target.value)}
                />
              </Grid>
              <Grid item xs={6} md={3}>
                <TextField
                  label="Group Id"
                  fullWidth
                  size="small"
                  value={groupId}
                  onChange={(e) => setGroupId(e.target.value)}
                />
              </Grid>

              <Grid item xs={12} md={6}>
                <TextField
                  label="Start At"
                  type="datetime-local"
                  fullWidth
                  size="small"
                  value={startAt}
                  onChange={(e) => setStartAt(e.target.value)}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  label="End At"
                  type="datetime-local"
                  fullWidth
                  size="small"
                  value={endAt}
                  onChange={(e) => setEndAt(e.target.value)}
                />
              </Grid>

              <Grid item xs={6} md={3}>
                <TextField
                  label="Time Limit (sec)"
                  fullWidth
                  size="small"
                  value={timeLimitSeconds}
                  onChange={(e) => setTimeLimitSeconds(e.target.value)}
                />
              </Grid>
              <Grid item xs={6} md={3}>
                <TextField
                  label="Max Attempts"
                  fullWidth
                  size="small"
                  type="number"
                  value={maxAttempts}
                  onChange={(e) => setMaxAttempts(e.target.value)}
                />
              </Grid>
              <Grid item xs={12} md={3}>
                <FormControlLabel
                  control={
                    <Checkbox checked={shuffleQ} onChange={(e) => setShuffleQ(e.target.checked)} />
                  }
                  label="Shuffle Questions"
                />
              </Grid>
              <Grid item xs={12} md={3}>
                <FormControlLabel
                  control={
                    <Checkbox checked={shuffleO} onChange={(e) => setShuffleO(e.target.checked)} />
                  }
                  label="Shuffle Options"
                />
              </Grid>
              <Grid item xs={12} md={3}>
                <TextField
                  label="Created By (StaffId)"
                  fullWidth
                  size="small"
                  value={createdByStaffId}
                  onChange={(e) => setCreatedByStaffId(e.target.value)}
                />
              </Grid>

              <Grid item xs={12}>
                <Button onClick={submit} variant="contained" disabled={busy}>
                  {busy ? "Publishing..." : "Publish"}
                </Button>
              </Grid>
            </Grid>
          </Card>
        </Grid>
      </Grid>
      <Footer />

      <Snackbar open={!!ok} autoHideDuration={3000} onClose={() => setOk("")}>
        <Alert severity="success" onClose={() => setOk("")}>
          {ok}
        </Alert>
      </Snackbar>
      <Snackbar open={!!err} autoHideDuration={4000} onClose={() => setErr("")}>
        <Alert severity="error" onClose={() => setErr("")}>
          {err}
        </Alert>
      </Snackbar>
    </DashboardLayout>
  );
}
