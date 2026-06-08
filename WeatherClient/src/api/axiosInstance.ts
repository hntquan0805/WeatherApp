import axios from "axios";

const axiosInstance = axios.create({
  baseURL: "http://localhost:5032/api", // đổi port theo project của bạn
  headers: { "Content-Type": "application/json" },
  timeout: 10000,
});

// ── Request interceptor: tự động gắn JWT vào header ──
axiosInstance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ── Response interceptor: xử lý token hết hạn ────────
axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token hết hạn → clear storage và redirect login
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      window.location.href = "/login";
    }
    return Promise.reject(error);
  }
);

export default axiosInstance;