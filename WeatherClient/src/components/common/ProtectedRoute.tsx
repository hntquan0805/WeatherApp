import { Navigate, Outlet } from "react-router-dom";
import { useAppSelector } from "../../store/hooks";
import LoadingSpinner from "./LoadingSpinner";

export default function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAppSelector((s) => s.auth);

  if (isLoading) return <LoadingSpinner />;

  return isAuthenticated
    ? <Outlet />
    : <Navigate to="/login" replace />;
}