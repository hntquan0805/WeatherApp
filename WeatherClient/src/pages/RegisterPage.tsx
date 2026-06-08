import { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { CloudLightning, User, Mail, Eye, EyeOff, AlertCircle, Check } from "lucide-react";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import { registerThunk, clearError } from "../store/slices/authSlice";

// Validation rules
const passwordRules = [
  { label: "At least 6 characters", test: (p: string) => p.length >= 6 },
  { label: "One uppercase (A-Z)",   test: (p: string) => /[A-Z]/.test(p) },
  { label: "One lowercase (a-z)",   test: (p: string) => /[a-z]/.test(p) },
  { label: "One number (0-9)",      test: (p: string) => /\d/.test(p) },
];

/* ─── Floating-label input ─────────────────────────────────────── */
interface FloatInputProps {
  label: string;
  type?: string;
  name: string;
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  placeholder?: string;
  required?: boolean;
  minLength?: number;
  rightSlot?: React.ReactNode;
  hasError?: boolean;
}

function FloatInput({
  label, type = "text", name, value, onChange,
  placeholder = "", required, minLength, rightSlot, hasError,
}: FloatInputProps) {
  return (
    <div className="register-page__float-input">
      <span className="register-page__float-label">{label}</span>
      <input
        type={type}
        name={name}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        required={required}
        minLength={minLength}
        className="register-page__input"
        style={{
          borderColor: hasError ? "#f87171" : "#d0d5e8",
          paddingRight: rightSlot ? "42px" : "14px",
        }}
        onFocus={(e) => (e.target.style.borderColor = hasError ? "#f87171" : "#4f6ef7")}
        onBlur={(e)  => (e.target.style.borderColor = hasError ? "#f87171" : "#d0d5e8")}
      />
      {rightSlot && (
        <div className="register-page__input-right-slot">{rightSlot}</div>
      )}
    </div>
  );
}

/* ─── Main page ─────────────────────────────────────────────────── */
export default function RegisterPage() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { isLoading, error, isAuthenticated } = useAppSelector((s) => s.auth);

  const [form, setForm] = useState({
    username:        "",
    email:           "",
    password:        "",
    confirmPassword: "",
  });
  const [showPass,        setShowPass]        = useState(false);
  const [showConfirmPass, setShowConfirmPass] = useState(false);
  const [formError,       setFormError]       = useState("");

  useEffect(() => {
    if (isAuthenticated) navigate("/weather", { replace: true });
  }, [isAuthenticated, navigate]);

  useEffect(() => {
    return () => { dispatch(clearError()); };
  }, [dispatch]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
    setFormError("");
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (form.password !== form.confirmPassword) {
      setFormError("Passwords do not match.");
      return;
    }
    const allRulesPassed = passwordRules.every((r) => r.test(form.password));
    if (!allRulesPassed) {
      setFormError("Password does not meet all requirements.");
      return;
    }
    dispatch(registerThunk(form));
  };

  const displayError = formError || error;
  const confirmMismatch =
    form.confirmPassword.length > 0 && form.confirmPassword !== form.password;

  return (
    <div className="register-page">
      <div className="register-page__card">

        {/* ── Left: Form ── */}
        <div className="register-page__form-panel">

          {/* Brand */}
          <div className="register-page__brand">
            <CloudLightning size={20} color="#4f6ef7" />
            <span className="register-page__brand-name">WeatherApp</span>
          </div>

          <h1 className="register-page__title">Create account</h1>
          <p className="register-page__subtitle">Free, no credit card required</p>

          {/* Error */}
          {displayError && (
            <div className="register-page__error">
              <AlertCircle size={15} className="shrink-0" />
              {displayError}
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="register-page__form">

            {/* Username */}
            <FloatInput
              label="Username"
              name="username"
              value={form.username}
              onChange={handleChange}
              placeholder="Display name"
              required
              minLength={3}
              rightSlot={<User size={15} color="#9ca3af" />}
            />

            {/* Email */}
            <FloatInput
              label="Email"
              type="email"
              name="email"
              value={form.email}
              onChange={handleChange}
              placeholder="example@email.com"
              required
              rightSlot={<Mail size={15} color="#9ca3af" />}
            />

            {/* Password */}
            <FloatInput
              label="Password"
              type={showPass ? "text" : "password"}
              name="password"
              value={form.password}
              onChange={handleChange}
              placeholder="••••••••"
              required
              rightSlot={
                <button
                  type="button"
                  onClick={() => setShowPass(!showPass)}
                  className="register-page__eye-btn"
                >
                  {showPass ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              }
            />

            {/* Password rules */}
            {form.password && (
              <div className="register-page__rules">
                {passwordRules.map((rule) => {
                  const ok = rule.test(form.password);
                  return (
                    <div
                      key={rule.label}
                      className={`register-page__rule ${ok ? "register-page__rule--ok" : ""}`}
                    >
                      <Check size={12} />
                      {rule.label}
                    </div>
                  );
                })}
              </div>
            )}

            {/* Confirm Password */}
            <FloatInput
              label="Confirm Password"
              type={showConfirmPass ? "text" : "password"}
              name="confirmPassword"
              value={form.confirmPassword}
              onChange={handleChange}
              placeholder="••••••••"
              required
              hasError={confirmMismatch}
              rightSlot={
                <button
                  type="button"
                  onClick={() => setShowConfirmPass(!showConfirmPass)}
                  className="register-page__eye-btn"
                >
                  {showConfirmPass ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              }
            />
            {confirmMismatch && (
              <p className="register-page__confirm-hint">Passwords do not match</p>
            )}

            {/* Submit */}
            <button
              type="submit"
              disabled={isLoading}
              className="register-page__submit-btn"
            >
              {isLoading ? (
                <>
                  <div className="register-page__spinner" />
                  Creating account...
                </>
              ) : "Create account"}
            </button>
          </form>

          {/* Footer */}
          <p className="register-page__footer">
            Already have an account?{" "}
            <Link to="/login" className="register-page__footer-link">
              Sign in
            </Link>
          </p>
        </div>

        {/* ── Right: GIF panel ── */}
        <div className="register-page__gif-panel">
          <img
            src="/register-bg.gif"
            alt="WeatherApp animation"
            className="register-page__gif"
          />
        </div>

      </div>
    </div>
  );
}