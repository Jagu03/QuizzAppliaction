import React, { useState } from "react";
import { Grid, Card, TextField, Button, Snackbar, Alert, Typography } from "@mui/material";
import DashboardLayout from "examples/LayoutContainers/DashboardLayout";
import DashboardNavbar from "examples/Navbars/DashboardNavbar";
import Footer from "examples/Footer";
import { createClassGroup } from "api/groupApi";

export default function GroupCreatePage() {
  const [classId, setClassId] = useState("");
  const [groupName, setGroupName] = useState("");
  const [createdByStaffId, setCreatedByStaffId] = useState("");
  const [busy, setBusy] = useState(false);
  const [ok, setOk] = useState("");
  const [err, setErr] = useState("");

  const submit = async () => {
    if (!classId || !groupName || !createdByStaffId) {
      setErr("All fields are required.");
      return;
    }
    setBusy(true);
    try {
      const res = await createClassGroup({
        classId: Number(classId),
        groupName: groupName.trim(),
        createdByStaffId: Number(createdByStaffId),
      });
      const id = res?.data?.data?.groupId;
      setOk(`Group created (ID: ${id})`);
      setClassId("");
      setGroupName("");
      setCreatedByStaffId("");
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
        <Grid item xs={12} md={8} lg={6}>
          <Card sx={{ p: 3, borderRadius: 3 }}>
            <Typography variant="h5" fontWeight={700} mb={2}>
              Create Class Group
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={12} md={4}>
                <TextField
                  label="Class Id"
                  value={classId}
                  onChange={(e) => setClassId(e.target.value)}
                  fullWidth
                  size="small"
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  label="Group Name"
                  value={groupName}
                  onChange={(e) => setGroupName(e.target.value)}
                  fullWidth
                  size="small"
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <TextField
                  label="Created By (StaffId)"
                  value={createdByStaffId}
                  onChange={(e) => setCreatedByStaffId(e.target.value)}
                  fullWidth
                  size="small"
                />
              </Grid>
              <Grid item xs={12}>
                <Button onClick={submit} variant="contained" disabled={busy}>
                  {busy ? "Saving..." : "Create Group"}
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
