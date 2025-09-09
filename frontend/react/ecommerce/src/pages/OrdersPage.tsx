import { useOrdersByUserId } from '@/features/orders/OrderHooks';
import OrderCard from '@/features/orders/components/OrderCard';
import { formatCurrency } from '@/utils/formatCurrency';
import { useNavigate } from 'react-router-dom';

export default function OrdersPage() {
  const navigate = useNavigate();
  const userData = localStorage.getItem('user');
  const currentUserId = userData ? JSON.parse(userData).userID : null;
  
  const { data: ordersData, isLoading, isError } = useOrdersByUserId(currentUserId || '');
  
  // Use the data or fallback to empty array
  const orders = ordersData || [];
  
  const totalSpent = orders.reduce((sum: number, order: any) => sum + order.totalBill, 0);
  const totalItems = orders.reduce((sum: number, order: any) => sum + order.orderItems.length, 0);
  
  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600 text-lg">Failed to load orders</p>
          <p className="text-gray-600">Please try again later</p>
        </div>
      </div>
    );
  }
  
  // Check for unauthenticated users
  if (!currentUserId) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <p className="text-red-600 text-lg">Please login to view your orders</p>
          <button
            onClick={() => navigate('/login')}
            className="mt-4 bg-blue-600 text-white px-6 py-2 rounded-md hover:bg-blue-700 transition"
          >
            Go to Login
          </button>
        </div>
      </div>
    );
  } 
  
  return (
<div className="min-h-screen bg-gray-50 flex flex-col">
  {/* Header */}
  <header className="bg-white shadow-sm">
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div className="flex justify-between items-center h-16">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">My Orders</h1>
          <p className="text-sm text-gray-600">Your order history</p>
        </div>
        <button
          onClick={() => navigate('/dashboard')}
          className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition"
        >
          Continue Shopping
        </button>
          </div>
        </div>
      </header>

       <main className="flex-grow max-w-5xl w-full mx-auto px-4 py-8">
        {/* Orders Summary */}
        <div className="bg-gradient-to-r from-purple-600 to-blue-600 rounded-2xl p-6 text-white shadow-lg mb-8">
          <h2 className="text-xl font-bold mb-4 text-center sm:text-left">Order Summary</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="text-center">
              <div className="text-2xl font-bold">{orders.length}</div>
              <div className="text-sm opacity-90">Total Orders</div>
            </div>
            
            <div className="text-center">
              <div className="text-2xl font-bold">{totalItems}</div>
              <div className="text-sm opacity-90">Items Purchased</div>
            </div>
            
            <div className="text-center">
              <div className="text-2xl font-bold">{formatCurrency(totalSpent)}</div>
              <div className="text-sm opacity-90">Total Spent</div>
            </div>
          </div>
        </div>

        {/* Orders List */}
        <div className="space-y-6">
          {orders.length === 0 ? (
            <div className="bg-white rounded-2xl shadow-lg p-8 md:p-12 text-center">
              <svg className="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
              </svg>
              <h3 className="mt-4 text-lg font-medium text-gray-900">No orders yet</h3>
              <p className="mt-2 text-gray-600">Start shopping to see your orders here!</p>
              <button
                onClick={() => navigate('/dashboard')}
                className="mt-4 bg-blue-600 text-white px-6 py-2 rounded-md hover:bg-blue-700 transition"
              >
                Start Shopping
              </button>
            </div>
          ) : (
            orders.map((order : any) => (
              <OrderCard key={order.orderID} order={order} />
            ))
          )}
        </div>
      </main>
    </div>
  );
}