import { Link } from 'react-router-dom';
import type { Order } from '../OrderTypes';
import { formatCurrency } from '@/utils/formatCurrency';
import { formatDateTime } from '@/utils/formatDate';

interface OrderCardProps {
  order: Order;
}

export default function OrderCard({ order }: OrderCardProps) {
  return (
    <Link to={`/orders/${order.orderID}`} className="block hover:no-underline">
      <div className="bg-white border border-gray-200 rounded-lg p-6 shadow-sm hover:shadow-md transition-shadow hover:border-blue-300">
        <div className="flex justify-between items-start mb-4">
          <div>
            <h3 className="font-semibold text-gray-800 hover:text-blue-600 transition-colors">
              Order #{order.orderID.slice(0, 8)}
            </h3>
            <p className="text-sm text-gray-600">User: {order.userID.slice(0, 8)}...</p>
          </div>
          <div className="text-right">
            <p className="text-lg font-bold text-blue-600">{formatCurrency(order.totalBill)}</p>
            <p className="text-sm text-gray-500">{formatDateTime(order.orderDate)}</p>
          </div>
        </div>

        <div className="border-t border-gray-100 pt-4">
          <h4 className="font-medium text-gray-700 mb-3">Order Items ({order.orderItems.length})</h4>
          <div className="space-y-3">
            {order.orderItems.slice(0, 3).map((item, index) => ( // Show only first 3 items
              <div key={index} className="flex items-center justify-between text-sm">
                <div className="flex items-center space-x-3">
                  {item.imgUrl && (
                    <img
                      src={item.imgUrl}
                      alt= {item.productName || `Product: ${item.productID.slice(0, 8)}`}
                      className="w-10 h-10 rounded object-cover"
                      onError={(e) => {
                        (e.target as HTMLImageElement).src = 'https://via.placeholder.com/40';
                      }}
                    />
                  )}
                  <div>
                    <p className="font-medium text-gray-800">Product:  {item.productName || `Product: ${item.productID.slice(0, 8)}...`}</p>
                    <p className="text-gray-600">Qty: {item.quantity}</p>
                  </div>
                </div>
                <div className="text-right">
                  <p className="font-medium text-gray-800">{formatCurrency(item.unitPrice)}</p>
                  <p className="text-green-600">Total: {formatCurrency(item.totalPrice)}</p>
                </div>
              </div>
            ))}
            {order.orderItems.length > 3 && (
              <p className="text-sm text-gray-500 text-center">
                + {order.orderItems.length - 3} more items...
              </p>
            )}
          </div>
        </div>
      </div>
    </Link>
  );
}