import { Link, useNavigate } from "react-router-dom";
import { LogOut, CloudLightning } from "lucide-react";
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

        {/* Logo */}
        <Link to="/weather" className="navbar__logo">
          <CloudLightning size={20} />
          WeatherApp
        </Link>

        {/* Right side */}
        {isAuthenticated ? (
          <div className="navbar__actions">
            <Link to="/profile" className="navbar__profile-link">
              <div className="navbar__avatar">
                {user?.username[0].toUpperCase()}
              </div>
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