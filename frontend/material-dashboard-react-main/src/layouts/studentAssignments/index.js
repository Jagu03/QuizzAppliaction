import React, { useEffect, useState } from "react";
import {
  Grid,
  Card,
  Typography,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  Button,
  Snackbar,
  Alert,
} from "@mui/material";
import DashboardLayout from "examples/LayoutContainers/DashboardLayout";
import DashboardNavbar from "examples/Navbars/DashboardNavbar";
import Footer from "examples/Footer";
import { fetchStudentActiveAssignments } from "api/quizApi";
import { useNavigate } from "react-router-dom";

export default function StudentAssignmentsPage() {
  const [studentId, setStudentId] = useState(7772); // TODO: get from auth/session
  const [rows, setRows] = useState([]);
  const [err, setErr] = useState("");
  const nav = useNavigate();

  const load = async () => {
    try {
      const res = await fetchStudentActiveAssignments(studentId);
      const dt = res?.data?.data; // DataTable serialized as array of objects
      setRows(dt || []);
    } catch (e) {
      setErr(e.message);
    }
  };

  useEffect(() => {
    load(); /* eslint-disable-next-line */
  }, []);

  const goTake = (row) => {
    nav("/take-quiz", { state: { assignmentId: row.AssignmentId, studentId } });
  };

  return (
    <DashboardLayout>
      <DashboardNavbar />
      <Grid container justifyContent="center" mt={4}>
        <Grid item xs={12} md={10}>
          <Card sx={{ p: 3, borderRadius: 3 }}>
            <Typography variant="h5" fontWeight={700} mb={2}>
              My Active Quizzes
            </Typography>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Title</TableCell>
                  <TableCell>Start</TableCell>
                  <TableCell>End</TableCell>
                  <TableCell>Attempts</TableCell>
                  <TableCell>Action</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {(rows || []).map((r, i) => (
                  <TableRow key={i}>
                    <TableCell>{r.Title}</TableCell>
                    <TableCell>{new Date(r.StartAt).toLocaleString()}</TableCell>
                    <TableCell>{new Date(r.EndAt).toLocaleString()}</TableCell>
                    <TableCell>
                      {r.AttemptCount}/{r.MaxAttempts}
                    </TableCell>
                    <TableCell>
                      <Button variant="contained" size="small" onClick={() => goTake(r)}>
                        Start
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
                {(!rows || rows.length === 0) && (
                  <TableRow>
                    <TableCell colSpan={5} align="center">
                      No active assignments.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </Card>
        </Grid>
      </Grid>
      <Footer />

      <Snackbar open={!!err} autoHideDuration={4000} onClose={() => setErr("")}>
        <Alert severity="error" onClose={() => setErr("")}>
          {err}
        </Alert>
      </Snackbar>
    </DashboardLayout>
  );
}
