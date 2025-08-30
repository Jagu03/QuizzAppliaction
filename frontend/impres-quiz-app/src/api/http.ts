import axios from 'axios'

export const http = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL, // e.g. https://localhost:7166
  withCredentials: false
})

http.interceptors.response.use(
  r => r,
  err => {
    const msg = err?.response?.data?.message || err?.message
    console.error('API error:', msg, err?.response)
    return Promise.reject(err)
  }
)
