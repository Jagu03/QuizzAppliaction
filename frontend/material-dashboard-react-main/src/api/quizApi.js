import http from "./http";

export const fetchStudentActiveAssignments = (studentId) =>
  http.get(`/api/quiz/student/${studentId}/active`);

export const startAttempt = (assignmentId, studentId) =>
  http.post(`/api/quiz/attempts/start`, { assignmentId, studentId });

export const submitAnswer = (attemptId, questionId, selectedOptionId) =>
  http.post(`/api/quiz/attempts/answer`, { attemptId, questionId, selectedOptionId });

export const finishAttempt = (attemptId) =>
  http.post(`/api/quiz/attempts/finish`, { attemptId });
