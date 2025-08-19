import React, { useEffect, useState, useMemo } from "react";
import {
  Grid,
  Card,
  Typography,
  Button,
  RadioGroup,
  FormControlLabel,
  Radio,
  Snackbar,
  Alert,
  CircularProgress,
} from "@mui/material";
import DashboardLayout from "examples/LayoutContainers/DashboardLayout";
import DashboardNavbar from "examples/Navbars/DashboardNavbar";
import Footer from "examples/Footer";
import { useLocation, useNavigate } from "react-router-dom";
import { startAttempt, submitAnswer, finishAttempt } from "api/quizApi";

export default function TakeQuizPage() {
  const nav = useNavigate();
  const { state } = useLocation() || {};
  const assignmentId = state?.assignmentId;
  const studentId = state?.studentId;

  const [loading, setLoading] = useState(true);
  const [attemptId, setAttemptId] = useState(null);
  const [questions, setQuestions] = useState([]); // [{AttemptAnswerId, QuestionId, QuestionText}]
  const [optionsMap, setOptionsMap] = useState({}); // {QuestionId: [{OptionId, OptionText}]}
  const [answers, setAnswers] = useState({}); // {QuestionId: OptionId}
  const [ok, setOk] = useState("");
  const [err, setErr] = useState("");

  useEffect(() => {
    const boot = async () => {
      if (!assignmentId || !studentId) {
        setErr("Missing assignment context.");
        setLoading(false);
        return;
      }
      try {
        const res = await startAttempt(assignmentId, studentId);
        const data = res?.data?.data;
        const qs = data?.questions || [];
        const ops = data?.options || [];
        const map = {};
        ops.forEach((o) => {
          const qid = o.QuestionId;
          if (!map[qid]) map[qid] = [];
          map[qid].push({ OptionId: o.OptionId, OptionText: o.OptionText });
        });
        setAttemptId(data?.attemptId);
        setQuestions(qs);
        setOptionsMap(map);
      } catch (e) {
        setErr(e.message);
      } finally {
        setLoading(false);
      }
    };
    boot();
    // eslint-disable-next-line
  }, []);

  const allAnswered = useMemo(() => {
    if (!questions.length) return false;
    return questions.every((q) => !!answers[q.QuestionId]);
  }, [questions, answers]);

  const onChoose = async (qId, optId) => {
    setAnswers((p) => ({ ...p, [qId]: optId }));
    // save immediately
    try {
      await submitAnswer(attemptId, qId, optId);
    } catch (e) {
      setErr(e.message);
    }
  };

  const onFinish = async () => {
    if (!attemptId) return;
    try {
      const res = await finishAttempt(attemptId);
      const s = res?.data?.data?.score;
      const t = res?.data?.data?.total;
      setOk(`Submitted. Score: ${s}/${t}`);
      setTimeout(() => nav("/student-assignments"), 1200);
    } catch (e) {
      setErr(e.message);
    }
  };

  return (
    <DashboardLayout>
      <DashboardNavbar />
      <Grid container justifyContent="center" mt={4} mb={4}>
        <Grid item xs={12} md={10} lg={8}>
          <Card sx={{ p: 3, borderRadius: 3 }}>
            <Typography variant="h5" fontWeight={700} mb={2}>
              Take Quiz
            </Typography>
            {loading ? (
              <CircularProgress />
            ) : (
              <>
                {questions.map((q, idx) => (
                  <Card
                    key={q.AttemptAnswerId}
                    sx={{ p: 2, mb: 2, borderRadius: 2, background: "#fafafa" }}
                  >
                    <Typography fontWeight={700} mb={1}>
                      Q{idx + 1}. {q.QuestionText}
                    </Typography>
                    <RadioGroup
                      value={answers[q.QuestionId] || ""}
                      onChange={(e) => onChoose(q.QuestionId, Number(e.target.value))}
                    >
                      {(optionsMap[q.QuestionId] || []).map((op) => (
                        <FormControlLabel
                          key={op.OptionId}
                          value={op.OptionId}
                          control={<Radio />}
                          label={op.OptionText}
                        />
                      ))}
                    </RadioGroup>
                  </Card>
                ))}

                <Button
                  variant="contained"
                  disabled={!allAnswered || !questions.length}
                  onClick={onFinish}
                >
                  Finish Quiz
                </Button>
              </>
            )}
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
