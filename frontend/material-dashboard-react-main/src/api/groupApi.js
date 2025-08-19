import http from "./http";

export const createClassGroup = (payload) =>
  http.post("/api/groups/class", payload); // { classId, groupName, createdByStaffId }

export const createCustomGroup = (payload) =>
  http.post("/api/groups/custom", payload); // { groupName, createdByStaffId, studentIdsCsv }
