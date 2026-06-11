import axios from "axios";

const axiosInstance = axios.create({
  baseURL: "http://localhost:5032/api",
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
    const requestUrl = error.config?.url ?? "";
    const isAuthRequest = requestUrl.includes("/auth/login") || requestUrl.includes("/auth/register");

    if (error.response?.status === 401 && !isAuthRequest) {
      // Token hết hạn → clear storage và redirect login
      localStorage.removeItem("token");
      localStorage.removeItem("user");
      window.location.href = "/login";
    }

    // Xử lý rate limit
    if (error.response?.status === 429) {
      const retryAfter = error.response.headers["retry-after"];
      const seconds    = retryAfter ? parseInt(retryAfter) : 60;
      error.message    = `Quá nhiều yêu cầu. Vui lòng thử lại sau ${seconds} giây.`;
    }
    
    return Promise.reject(error);
  }
);

export default axiosInstance;