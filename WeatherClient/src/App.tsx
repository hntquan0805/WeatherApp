import { useEffect } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { restoreSession } from "./store/slices/authSlice";
import { useAppDispatch } from "./store/hooks";
import ProtectedRoute from "./components/common/ProtectedRoute";
import Navbar         from "./components/common/Navbar";
import LoginPage      from "./pages/LoginPage";
import RegisterPage   from "./pages/RegisterPage";
import WeatherPage    from "./pages/WeatherPage";
import ProfilePage    from "./pages/ProfilePage";

export default function App() {
  const dispatch = useAppDispatch();

  // Restore session một lần khi app mount
  useEffect(() => {
    dispatch(restoreSession());
  }, [dispatch]);

  return (
    <BrowserRouter>
      <div className="min-h-screen bg-gray-50">
        <Navbar />
        <Routes>
          <Route path="/login"    element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          <Route element={<ProtectedRoute />}>
            <Route path="/weather" element={<WeatherPage />} />
            <Route path="/profile" element={<ProfilePage />} />
          </Route>

          <Route path="/" element={<Navigate to="/weather" replace />} />
          <Route path="*" element={<Navigate to="/weather" replace />} />
        </Routes>
      </div>
    </BrowserRouter>
  );
}