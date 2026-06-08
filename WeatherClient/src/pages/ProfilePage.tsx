import { useState } from "react";
import { User, Mail, Shield, Calendar, KeyRound, Loader2 } from "lucide-react";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import { updateUser } from "../store/slices/authSlice";
import { authApi } from "../api/authApi";
import type { UpdateUserDto } from "../types";

interface FloatInputProps {
  label: string;
  type?: string;
  name: string;
  value: string;
  onChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
  placeholder?: string;
  leftIcon?: React.ReactNode;
}

function FloatInput({ label, type = "text", name, value, onChange, placeholder = "", leftIcon }: FloatInputProps) {
  return (
    <div className="profile-page__float-input">
      <span
        className="profile-page__float-label"
        style={{ background: "#ffffff", color: "#111827" }}
      >
        {label}
      </span>
      {leftIcon && <div className="profile-page__input-left-icon">{leftIcon}</div>}
      <input
        type={type}
        name={name}
        value={value}
        onChange={onChange}
        placeholder={placeholder}
        className="profile-page__input"
        style={{ paddingLeft: leftIcon ? "38px" : "14px", background: "#ffffff" }}
        onFocus={(e) => (e.target.style.borderColor = "#4f6ef7")}
        onBlur={(e)  => (e.target.style.borderColor = "#d0d5e8")}
      />
    </div>
  );
}

const STAT_COLORS = [
  { bg: "rgba(207, 250, 254, 0.85)", icon: "#06b6d4", label: "#111827", value: "#164e63" }, // cyan    - Username
  { bg: "rgba(209, 250, 229, 0.85)", icon: "#10b981", label: "#111827", value: "#065f46" }, // emerald - Email
  { bg: "rgba(254, 215, 170, 0.85)", icon: "#f97316", label: "#111827", value: "#9a3412" }, // orange  - Role
  { bg: "rgba(237, 233, 254, 0.85)", icon: "#8b5cf6", label: "#111827", value: "#5b21b6" }, // violet  - Joined
];

export default function ProfilePage() {
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((s) => s.auth);

  const [form, setForm] = useState<UpdateUserDto>({
    username:        user?.username ?? "",
    email:           user?.email   ?? "",
    currentPassword: "",
    newPassword:     "",
  });

  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage]     = useState<{ type: "success" | "error"; text: string } | null>(null);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
    setMessage(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setMessage(null);
    try {
      const payload: UpdateUserDto = {};
      if (form.username !== user?.username) payload.username = form.username;
      if (form.email    !== user?.email)    payload.email    = form.email;
      if (form.newPassword) {
        payload.currentPassword = form.currentPassword;
        payload.newPassword     = form.newPassword;
      }
      const { data: res } = await authApi.updateProfile(payload);
      if (!res.success) throw new Error(res.message);
      dispatch(updateUser(res.data));
      setMessage({ type: "success", text: "Updated successfully!" });
      setForm((prev) => ({ ...prev, currentPassword: "", newPassword: "" }));
    } catch (err: any) {
      setMessage({ type: "error", text: err.response?.data?.message ?? err.message });
    } finally {
      setIsLoading(false);
    }
  };

  const stats = [
    { icon: <User size={15} />,     label: "Username", value: user?.username },
    { icon: <Mail size={15} />,     label: "Email",    value: user?.email },
    { icon: <Shield size={15} />,   label: "Role",     value: user?.role },
    { icon: <Calendar size={15} />, label: "Joined",   value: new Date(user?.createdAt ?? "").toLocaleDateString("en-US") },
  ];

  return (
    <div className="profile-page">

      <style>{`
        @keyframes bubbleFloat1 {
          0%, 100% { transform: translate(0px, 0px) scale(1); }
          33%       { transform: translate(18px, -22px) scale(1.04); }
          66%       { transform: translate(-12px, 10px) scale(0.97); }
        }
        @keyframes bubbleFloat2 {
          0%, 100% { transform: translate(0px, 0px) scale(1); }
          40%       { transform: translate(-20px, 15px) scale(1.06); }
          75%       { transform: translate(14px, -18px) scale(0.95); }
        }
        @keyframes bubbleFloat3 {
          0%, 100% { transform: translate(0px, 0px) scale(1); }
          50%       { transform: translate(10px, 20px) scale(1.03); }
        }
        .profile-bg {
          position: fixed; inset: 0; z-index: 0;
          background: linear-gradient(135deg, #eef4fb 0%, #daeef9 50%, #eaf6fb 100%);
          overflow: hidden; pointer-events: none;
        }
        .profile-bubble {
          position: absolute; border-radius: 50%;
          filter: blur(2px); opacity: 0.55;
        }
        .profile-bubble-1 {
          width: 320px; height: 320px; top: -80px; left: -60px;
          background: radial-gradient(circle, rgba(184,223,245,0.7) 0%, rgba(147,205,237,0.3) 70%, transparent 100%);
          animation: bubbleFloat1 9s ease-in-out infinite;
        }
        .profile-bubble-2 {
          width: 240px; height: 240px; top: 40%; right: -60px;
          background: radial-gradient(circle, rgba(167,210,240,0.65) 0%, rgba(184,223,245,0.25) 65%, transparent 100%);
          animation: bubbleFloat2 12s ease-in-out infinite;
        }
        .profile-bubble-3 {
          width: 180px; height: 180px; bottom: 80px; left: 30%;
          background: radial-gradient(circle, rgba(200,232,248,0.6) 0%, rgba(184,223,245,0.2) 70%, transparent 100%);
          animation: bubbleFloat3 8s ease-in-out infinite;
        }
        .profile-bubble-4 {
          width: 120px; height: 120px; top: 25%; left: 55%;
          background: radial-gradient(circle, rgba(184,223,245,0.5) 0%, transparent 70%);
          animation: bubbleFloat2 14s ease-in-out infinite reverse;
        }
        .profile-bubble-5 {
          width: 90px; height: 90px; bottom: 20%; right: 20%;
          background: radial-gradient(circle, rgba(147,205,237,0.45) 0%, transparent 70%);
          animation: bubbleFloat1 11s ease-in-out infinite reverse;
        }
        .profile-page-wrap { position: relative; z-index: 1; }

        /* Two-column layout */
        .profile-two-col {
          display: grid;
          grid-template-columns: 1fr 1fr;
          gap: 32px;
          align-items: start;
        }
        @media (max-width: 700px) {
          .profile-two-col { grid-template-columns: 1fr; }
        }

        /* #3 — border và hint màu đen cho nền sáng */
        .profile-pw-divider {
          border-top: 1.5px solid #111827;
          padding-top: 16px;
          margin-top: 4px;
        }
        .profile-pw-hint {
          display: flex;
          align-items: center;
          gap: 6px;
          font-size: 11.5px;
          color: #111827;
          margin-bottom: 14px;
          font-weight: 500;
        }
      `}</style>

      <div className="profile-bg">
        <div className="profile-bubble profile-bubble-1" />
        <div className="profile-bubble profile-bubble-2" />
        <div className="profile-bubble profile-bubble-3" />
        <div className="profile-bubble profile-bubble-4" />
        <div className="profile-bubble profile-bubble-5" />
      </div>

      <div className="profile-page-wrap">
        <h1 className="profile-page__title">My Profile</h1>

        <div className="profile-two-col">

          {/* ── LEFT: Avatar + Stats bên dưới ── */}
          <div>
            {/* Avatar block */}
            <div style={{
              display: "flex", flexDirection: "column", alignItems: "center", gap: "10px",
              background: "#ffffff",
              borderRadius: "16px", padding: "24px",
              boxShadow: "0 2px 12px rgba(100,160,200,0.12)",
              marginBottom: "16px",
            }}>
              <div className="profile-page__avatar" style={{ width: 68, height: 68 }}>
                <span style={{ fontSize: 28, fontWeight: 800, color: "var(--color-primary)" }}>
                  {user?.username[0].toUpperCase()}
                </span>
              </div>
              <div style={{ textAlign: "center" }}>
                <p style={{ fontWeight: 700, fontSize: 15, color: "#111827" }}>{user?.username}</p>
                <p style={{ fontSize: 12.5, color: "#6b7280", marginTop: 2 }}>{user?.email}</p>
              </div>
            </div>

            {/* Stats: hàng dọc */}
            <div style={{ display: "flex", flexDirection: "column", gap: "10px" }}>
              {stats.map(({ icon, label, value }, i) => {
                const c = STAT_COLORS[i];
                return (
                  <div key={label} style={{
                    display: "flex", alignItems: "center", gap: "12px",
                    background: c.bg, backdropFilter: "blur(8px)",
                    borderRadius: "12px", padding: "12px 16px",
                    boxShadow: "0 1px 6px rgba(100,160,200,0.10)",
                  }}>
                    <span style={{ color: c.icon, flexShrink: 0 }}>{icon}</span>
                    <div style={{ minWidth: 0 }}>
                      <p style={{ fontSize: 11, color: "#111827", fontWeight: 600, marginBottom: 2 }}>{label}</p>
                      <p style={{ fontSize: 13, fontWeight: 700, color: c.value, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>{value}</p>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* ── RIGHT: Update form with wrapper ── */}
          <div style={{
            background: "#ffffff",
            borderRadius: "16px",
            padding: "24px",
            boxShadow: "0 2px 12px rgba(100,160,200,0.12)",
          }}>
            {/* #2 — form nằm cột phải */}
            <h2 style={{ fontSize: 15, fontWeight: 700, color: "#111827", marginBottom: 20 }}>
              Update information
            </h2>

            {message && (
              <div className={`profile-page__message ${
                message.type === "success" ? "profile-page__message--success" : "profile-page__message--error"
              }`} style={{ marginBottom: 16 }}>
                {message.text}
              </div>
            )}

            <form onSubmit={handleSubmit} className="profile-page__form">
              <FloatInput
                label="Username" name="username"
                value={form.username ?? ""} onChange={handleChange}
                leftIcon={<User size={14} color="#111827" />}
              />
              <FloatInput
                label="Email" type="email" name="email"
                value={form.email ?? ""} onChange={handleChange}
                leftIcon={<Mail size={14} color="#111827" />}
              />

              {/* #3 — border + hint màu đen */}
              <div className="profile-pw-divider">
                <p className="profile-pw-hint">
                  <KeyRound size={12} />
                  Change password (leave blank to keep current)
                </p>
                <FloatInput
                  label="Current password" type="password" name="currentPassword"
                  value={form.currentPassword ?? ""} onChange={handleChange}
                  placeholder="••••••••"
                />
                <FloatInput
                  label="New password" type="password" name="newPassword"
                  value={form.newPassword ?? ""} onChange={handleChange}
                  placeholder="••••••••"
                />
              </div>

              <button type="submit" disabled={isLoading} className="profile-page__submit-btn">
                {isLoading ? <><Loader2 size={15} className="animate-spin" />Saving...</> : "Save changes"}
              </button>
            </form>
          </div>

        </div>
      </div>
    </div>
  );
}