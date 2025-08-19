import http from "./http";

export const publishAssignment = (payload) =>
  http.post("/api/assignments/publish", payload);
// payload: {
//   title, categoryId, groupId, startAt, endAt,
//   timeLimitSeconds, shuffleQuestions, shuffleOptions, maxAttempts, createdByStaffId
// }
