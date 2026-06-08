import { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { CloudLightning, Eye, EyeOff, AlertCircle } from "lucide-react";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import { loginThunk, clearError } from "../store/slices/authSlice";

/* ─── Ảnh slideshow cho right panel ────────────────────────────── */
const SLIDE_IMAGES = [
  new URL("../assets/loginPage_01.jpg", import.meta.url).href,
  new URL("../assets/loginPage_02.jpg", import.meta.url).href,
  new URL("../assets/loginPage_03.jpg", import.meta.url).href,
];

/* ─── Floating-label input ─────────────────────────────────────── */
interface FloatInputProps {
  label: string;
  type?: string;
  value: string;
  onChange: (v: string) => void;
  placeholder?: string;
  required?: boolean;
  rightSlot?: React.ReactNode;
}

function FloatInput({
  label, type = "text", value, onChange,
  placeholder = "", required, rightSlot,
}: FloatInputProps) {
  return (
    <div className="relative mb-5">
      <span
        className="absolute -top-2 left-3 px-1 text-[11.5px] font-medium z-10"
        style={{
          background: "white",
          color: "#6b7280",
          fontFamily: "'Inter', sans-serif",
        }}
      >
        {label}
      </span>
      <input
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        required={required}
        className="w-full h-12 px-3.5 text-sm outline-none transition-all"
        style={{
          border: "1.5px solid #d0d5e8",
          borderRadius: "10px",
          fontFamily: "'Inter', sans-serif",
          color: "#111827",
          background: "white",
          paddingRight: rightSlot ? "42px" : "14px",
        }}
        onFocus={(e) => (e.target.style.borderColor = "#4f6ef7")}
        onBlur={(e)  => (e.target.style.borderColor = "#d0d5e8")}
      />
      {rightSlot && (
        <div className="absolute right-3 top-1/2 -translate-y-1/2">
          {rightSlot}
        </div>
      )}
    </div>
  );
}

/* ─── Main page ─────────────────────────────────────────────────── */
export default function LoginPage() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { isLoading, error, isAuthenticated } = useAppSelector((s) => s.auth);

  const [email,    setEmail]    = useState("");
  const [password, setPassword] = useState("");
  const [showPass, setShowPass] = useState(false);
  const [slideIdx, setSlideIdx] = useState(0);

  // Auto-advance slideshow every 10s
  useEffect(() => {
    const timer = setInterval(() => {
      setSlideIdx((prev) => (prev + 1) % SLIDE_IMAGES.length);
    }, 10000);
    return () => clearInterval(timer);
  }, []);

  useEffect(() => {
    if (isAuthenticated) navigate("/weather", { replace: true });
  }, [isAuthenticated, navigate]);

  useEffect(() => {
    return () => { dispatch(clearError()); };
  }, [dispatch]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    dispatch(loginThunk({ email, password }));
  };

  return (
    <div
      className="min-h-[calc(100vh-57px)] flex items-center justify-center px-4 py-8"
      style={{ background: "#f5f7ff" }}
    >
      <div
        className="w-full max-w-3xl flex rounded-2xl overflow-hidden shadow-lg"
        style={{ background: "white" }}
      >
        {/* ── Left: Form ── */}
        <div className="flex-1 flex flex-col justify-center px-10 py-12">

          {/* Brand */}
          <div className="flex items-center gap-2 mb-9">
            <CloudLightning size={22} color="#4f6ef7" />
            <span
              className="text-[17px] font-bold"
              style={{ color: "#111827", fontFamily: "'Inter', sans-serif" }}
            >
              WeatherApp
            </span>
          </div>

          <h1
            className="text-[28px] font-extrabold mb-1.5"
            style={{ color: "#111827", fontFamily: "'Inter', sans-serif" }}
          >
            Welcome Back!
          </h1>
          <p
            className="text-sm mb-7"
            style={{ color: "#6b7280", fontFamily: "'Inter', sans-serif" }}
          >
            Please enter your login details below
          </p>

          {/* Error */}
          {error && (
            <div
              className="flex items-center gap-2 text-sm px-4 py-3 rounded-lg mb-5 border"
              style={{ background: "#fef2f2", borderColor: "#fecaca", color: "#dc2626" }}
            >
              <AlertCircle size={15} className="shrink-0" />
              {error}
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit}>
            <FloatInput
              label="Email"
              type="email"
              value={email}
              onChange={setEmail}
              placeholder="Enter the email"
              required
            />

            <FloatInput
              label="Password"
              type={showPass ? "text" : "password"}
              value={password}
              onChange={setPassword}
              placeholder="Enter the password"
              required
              rightSlot={
                <button
                  type="button"
                  onClick={() => setShowPass(!showPass)}
                  style={{ color: "#9ca3af", background: "none", border: "none", padding: 0, cursor: "pointer", display: "flex" }}
                >
                  {showPass ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              }
            />

            {/* Sign in */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full h-12 text-sm font-semibold flex items-center justify-center gap-2 transition-colors mb-5"
              style={{
                background: isLoading ? "#7b93f8" : "#4f6ef7",
                color: "white",
                borderRadius: "10px",
                border: "none",
                cursor: isLoading ? "not-allowed" : "pointer",
                fontFamily: "'Inter', sans-serif",
                fontSize: "15px",
              }}
              onMouseEnter={(e) => { if (!isLoading) e.currentTarget.style.background = "#3a5ae0"; }}
              onMouseLeave={(e) => { if (!isLoading) e.currentTarget.style.background = "#4f6ef7"; }}
            >
              {isLoading ? (
                <>
                  <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin" />
                  Signing in...
                </>
              ) : "Sign in"}
            </button>

            {/* Footer */}
            <p className="text-center text-sm" style={{ color: "#6b7280" }}>
              Don't have an account?{" "}
              <Link
                to="/register"
                className="font-semibold hover:underline"
                style={{ color: "#4f6ef7" }}
              >
                Sign Up
              </Link>
            </p>
          </form>
        </div>

        {/* ── Right: Slideshow panel ── */}
        <div
          className="hidden md:flex w-[46%] overflow-hidden"
          style={{ background: "#eef0fc" }}
        >
          <div className="login-slideshow">
            {SLIDE_IMAGES.map((src, i) => (
              <img
                key={src}
                src={src}
                alt={`Slide ${i + 1}`}
                className={`login-slideshow__img${i === slideIdx ? " login-slideshow__img--active" : ""}`}
              />
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}