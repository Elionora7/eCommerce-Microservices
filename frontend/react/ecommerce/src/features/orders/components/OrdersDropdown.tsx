import { useState, useRef, useEffect } from 'react';
import { useOrders } from '@/features/orders/OrderHooks';
import type { Order } from '@/features/orders/OrderTypes';
import { formatCurrency } from '@/utils/formatCurrency';
import { formatDateTime } from '@/utils/formatDate';

export default function OrdersDropdown() {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const { data: orders = [] } = useOrders();

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const recentOrders = orders.slice(0, 5);

  return (
    <div className="relative" ref={dropdownRef}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 text-gray-600 hover:text-blue-600 transition-colors"
      >
        <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
        </svg>
        {orders.length > 0 && (
          <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs font-bold rounded-full h-5 w-5 flex items-center justify-center">
            {orders.length}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="absolute right-0 mt-2 w-96 bg-white rounded-lg shadow-xl border border-gray-200 z-50">
          <div className="p-4 border-b border-gray-100">
            <h3 className="font-semibold text-gray-800">Recent Orders</h3>
            <p className="text-sm text-gray-600">{orders.length} total orders</p>
          </div>

          <div className="max-h-96 overflow-y-auto">
            {recentOrders.length === 0 ? (
              <div className="p-4 text-center text-gray-500">
                No orders yet
              </div>
            ) : (
              recentOrders.map((order) => (
                <OrderItem key={order.orderID} order={order} />
              ))
            )}
          </div>

          <div className="p-4 border-t border-gray-100 bg-gray-50">
            <div className="flex justify-between items-center">
              <span className="text-sm text-gray-600">Total value</span>
              <span className="font-semibold text-blue-600">
                {formatCurrency(orders.reduce((total, order) => total + order.totalBill, 0))}
              </span>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function OrderItem({ order }: { order: Order }) {
  return (
    <div className="p-4 border-b border-gray-100 last:border-b-0 hover:bg-gray-50 transition-colors">
      <div className="flex justify-between items-start mb-2">
        <div className="flex-1">
          <h4 className="font-medium text-gray-800 text-sm">
            Order #{order.orderID.slice(0, 8)}
          </h4>
          <p className="text-xs text-gray-500">{formatDateTime(order.orderDate)}</p>
        </div>
        <span className="font-semibold text-blue-600 text-sm">
          {formatCurrency(order.totalBill)}
        </span>
      </div>
      
      <div className="flex items-center justify-between text-xs text-gray-600">
        <span>{order.orderItems.length} items</span>
        <span>User: {order.userID.slice(0, 8)}...</span>
      </div>
    </div>
  );
}