import { Link, useNavigate } from "react-router-dom";
import { LogOut } from "lucide-react";
import { useAppDispatch, useAppSelector } from "../../store/hooks";
import { logout } from "../../store/slices/authSlice";

export default function Navbar() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAppSelector((s) => s.auth);

  const handleLogout = () => {
    dispatch(logout());
    navigate("/login");
  };

  return (
    <nav className="navbar">
      <div className="navbar__inner">

        {/* Logo – thay CloudLightning bằng ảnh từ public */}
        <Link to="/weather" className="navbar__logo">
          {/* Điền tên file logo của bạn vào src, ví dụ: "/logo.png" */}
          <img
            src="/logo.png"
            alt="WeatherApp logo"
            className="navbar__logo-img"
          />
          WeatherApp
        </Link>

        {/* Right side */}
        {isAuthenticated ? (
          <div className="navbar__actions">
            <Link to="/profile" className="navbar__profile-link">
              {/* [Task 4] Avatar với viền đen mỏng */}
              <div className="navbar__avatar">
                {user?.username[0].toUpperCase()}
              </div>
              {/* [Task 4] Username với underline-slide animation */}
              <span className="navbar__username">{user?.username}</span>
            </Link>
            <button onClick={handleLogout} className="navbar__logout-btn">
              <LogOut size={15} />
              Sign out
            </button>
          </div>
        ) : (
          <div className="navbar__actions">
            <Link to="/login" className="navbar__login-link">
              Sign in
            </Link>
            <Link to="/register" className="navbar__register-btn">
              Sign up
            </Link>
          </div>
        )}
      </div>
    </nav>
  );
}