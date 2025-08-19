import axios from "axios";

const http = axios.create({
  baseURL: process.env.REACT_APP_API_BASE || "https://localhost:7181",
});

http.interceptors.response.use(
  (res) => res,
  (err) => {
    const msg = err?.response?.data?.message || err.message || "Request failed";
    return Promise.reject(new Error(msg));
  }
);

export default http;
