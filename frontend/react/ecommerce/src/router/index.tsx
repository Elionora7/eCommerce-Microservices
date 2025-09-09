import { Routes, Route, Navigate } from 'react-router-dom';
import PublicLayout from '@/layouts/PublicLayout';
import ProtectedLayout from '@/layouts/ProtectedLayout';
import LoginPage from '@/pages/LoginPage';
import RegisterPage from '@/pages/RegisterPage';
import DashboardPage from '@/pages/DashboardPage';
import ProductDetails from '@/pages/ProductDetailsPage';
import OrderDetailsPage from '@/pages/OrderDetailsPage';
import OrdersPage from '@/pages/OrdersPage';
import ProfilePage from '@/pages/ProfilePage';

export default function AppRouter() {
  return (
      <Routes>
        <Route element={<PublicLayout />}>
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
        </Route>

        <Route element={<ProtectedLayout />}>
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/products/:id" element={<ProductDetails />} />
          <Route path="/orders" element={<OrdersPage />} /> 
          <Route path="/orders/:id" element={<OrderDetailsPage />} /> 
          <Route path="/profile" element={<ProfilePage />} /> 
        </Route>
      </Routes>
  );
}
