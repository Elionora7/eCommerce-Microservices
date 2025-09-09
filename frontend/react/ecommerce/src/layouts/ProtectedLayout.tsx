import { Navigate, Outlet } from 'react-router-dom';

export default function ProtectedLayout() {
  const isAuthenticated = localStorage.getItem('token');

  // Return Navigate directly if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Return Outlet if authenticated
  return <Outlet />;
}