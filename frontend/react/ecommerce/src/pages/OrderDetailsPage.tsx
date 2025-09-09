import { useParams } from 'react-router-dom';
import { useOrder } from '@/features/orders/OrderHooks';
import { formatCurrency } from '@/utils/formatCurrency';
import { formatDateTime } from '@/utils/formatDate';

export default function OrderDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { data: order, isLoading, isError } = useOrder(id || '');

  const handlePrintInvoice = () => {
    const printContent = document.getElementById('order-details-content');
    const originalContents = document.body.innerHTML;
    
    if (printContent) {
      document.body.innerHTML = printContent.innerHTML;
      window.print();
      document.body.innerHTML = originalContents;
      window.location.reload();
    }
  };

  const handleContactSupport = () => {
    const subject = `Support Request for Order #${order?.orderID.slice(0, 8)}`;
    const body = `Hello Support Team,\n\nI need assistance with my order:\n\nOrder ID: ${order?.orderID}\nOrder Date: ${order ? formatDateTime(order.orderDate) : ''}\nTotal Amount: ${order ? formatCurrency(order.totalBill) : ''}\n\nPlease help me with: `;
    
    window.location.href = `mailto:support@CBSInstyleDesign.com?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`;
  };

  if (isLoading) return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="animate-pulse">
        <div className="h-8 bg-gray-200 rounded w-1/3 mb-4"></div>
        <div className="h-4 bg-gray-200 rounded w-1/4 mb-8"></div>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="h-80 bg-gray-200 rounded"></div>
          <div className="space-y-4">
            <div className="h-6 bg-gray-200 rounded"></div>
            <div className="h-4 bg-gray-200 rounded w-2/3"></div>
            <div className="h-8 bg-gray-200 rounded w-1/2"></div>
          </div>
        </div>
      </div>
    </div>
  );

  if (isError) return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
        <h2 className="text-xl font-semibold text-red-800 mb-2">Error loading order</h2>
        <p className="text-red-600">Could not load order details. Please try again later.</p>
      </div>
    </div>
  );

  if (!order) return (
    <div className="max-w-4xl mx-auto p-6">
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-6 text-center">
        <h2 className="text-xl font-semibold text-yellow-800 mb-2">Order not found</h2>
        <p className="text-yellow-600">The order you're looking for doesn't exist.</p>
      </div>
    </div>
  );

  return (
    <div className="max-w-4xl mx-auto p-6">
      {/* Printable content (hidden from normal view) */}
      <div id="order-details-content" className="hidden">
        <div className="p-6">
          {/* Invoice Header */}
          <div className="text-center mb-8">
            <h1 className="text-3xl font-bold text-gray-800">INVOICE</h1>
            <p className="text-gray-600">CBS Instyle Design</p>
            <p className="text-gray-600">48 Business Street, Sydney, Australia</p>
            <p className="text-gray-600">contact@CBSInstyleDesign.com | +1 (555) 123-4567</p>
          </div>

          {/* Order Information */}
          <div className="grid grid-cols-2 gap-6 mb-8">
            <div>
              <h2 className="text-lg font-semibold mb-2">Bill To:</h2>
              <p>User ID: {order.userID}</p>
              <p>Order #: {order.orderID}</p>
              <p>Date: {formatDateTime(order.orderDate)}</p>
            </div>
            <div className="text-right">
              <h2 className="text-lg font-semibold mb-2">Invoice Details</h2>
              <p>Invoice Date: {new Date().toLocaleDateString()}</p>
              <p>Status: Completed</p>
            </div>
          </div>

          {/* Order Items Table */}
          <h2 className="text-xl font-semibold mb-4">Order Items</h2>
          <table className="w-full border-collapse border border-gray-300 mb-6">
            <thead>
              <tr className="bg-gray-100">
                <th className="border border-gray-300 p-3 text-left">Product ID</th>
                <th className="border border-gray-300 p-3 text-left">Product Name</th>
                <th className="border border-gray-300 p-3 text-center">Quantity</th>
                <th className="border border-gray-300 p-3 text-right">Unit Price</th>
                <th className="border border-gray-300 p-3 text-right">Total Price</th>
              </tr>
            </thead>
            <tbody>
              {order.orderItems.map((item, index) => (
                <tr key={index} className={index % 2 === 0 ? 'bg-gray-50' : ''}>
                  <td className="border border-gray-300 p-3">{item.productID}</td>
                  <td className="border border-gray-300 p-3">
                    {item.productName || `Product ${item.productID.slice(0, 8)}`}
                  </td>
                  <td className="border border-gray-300 p-3 text-center">{item.quantity}</td>
                  <td className="border border-gray-300 p-3 text-right">{formatCurrency(item.unitPrice)}</td>
                  <td className="border border-gray-300 p-3 text-right font-semibold">
                    {formatCurrency(item.totalPrice)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* Order Summary */}
          <div className="grid grid-cols-2 gap-6">
            <div>
              <h2 className="text-lg font-semibold mb-2">Payment Information</h2>
              <p>Payment Method: Credit Card</p>
              <p>Payment Status: Paid</p>
            </div>
            <div className="text-right">
              <div className="space-y-2">
                <div className="flex justify-between">
                  <span className="font-semibold">Subtotal:</span>
                  <span>{formatCurrency(order.totalBill)}</span>
                </div>
                <div className="flex justify-between">
                  <span className="font-semibold">Tax (0%):</span>
                  <span>{formatCurrency(0)}</span>
                </div>
                <div className="flex justify-between border-t border-gray-300 pt-2 mt-2">
                  <span className="text-lg font-bold">Total:</span>
                  <span className="text-lg font-bold text-blue-600">{formatCurrency(order.totalBill)}</span>
                </div>
              </div>
            </div>
          </div>

          {/* Footer */}
          <div className="mt-12 pt-6 border-t border-gray-300 text-center text-sm text-gray-600">
            <p>Thank you for your business!</p>
            <p>If you have any questions, please contact support@CBSInstyleDesign.com</p>
            <p className="mt-2">Invoice generated on: {new Date().toLocaleString()}</p>
          </div>
        </div>
      </div>

      {/* Header */}
      <div className="bg-white rounded-2xl shadow-lg p-6 mb-6 border border-gray-200">
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
          <div>
            <h1 className="text-3xl font-bold text-gray-800">Order Details</h1>
            <p className="text-gray-600 mt-2">Order #{order.orderID.slice(0, 8)}</p>
          </div>
          <div className="text-right">
            <p className="text-2xl font-bold text-blue-600">{formatCurrency(order.totalBill)}</p>
            <p className="text-sm text-gray-500">{formatDateTime(order.orderDate)}</p>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Order Items */}
        <div className="bg-white rounded-2xl shadow-lg p-6 border border-gray-200">
          <h2 className="text-xl font-semibold text-gray-800 mb-4">Order Items ({order.orderItems.length})</h2>
          <div className="space-y-4">
            {order.orderItems.map((item, index) => (
              <OrderItemCard key={index} item={item} />
            ))}
          </div>
        </div>

        {/* Order Summary */}
        <div className="space-y-6">
          <div className="bg-white rounded-2xl shadow-lg p-6 border border-gray-200">
            <h2 className="text-xl font-semibold text-gray-800 mb-4">Order Summary</h2>
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-gray-600">Order ID:</span>
                <span className="font-medium">{order.orderID}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">User ID:</span>
                <span className="font-medium">{order.userID}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Order Date:</span>
                <span className="font-medium">{formatDateTime(order.orderDate)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-600">Total Items:</span>
                <span className="font-medium">{order.orderItems.length}</span>
              </div>
              <div className="flex justify-between text-lg font-semibold border-t pt-3 mt-3">
                <span>Total Amount:</span>
                <span className="text-blue-600">{formatCurrency(order.totalBill)}</span>
              </div>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="bg-white rounded-2xl shadow-lg p-6 border border-gray-200">
            <h2 className="text-xl font-semibold text-gray-800 mb-4">Actions</h2>
            <div className="grid grid-cols-2 gap-3">
              <button 
                onClick={handlePrintInvoice}
                className="bg-blue-600 text-white py-3 rounded-lg hover:bg-blue-700 transition flex items-center justify-center"
              >
                <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                </svg>
                Print Invoice
              </button>
              <button 
                onClick={handleContactSupport}
                className="bg-gray-100 text-gray-700 py-3 rounded-lg hover:bg-gray-200 transition flex items-center justify-center"
              >
                <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
                </svg>
                Contact Support
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function OrderItemCard({ item }: { item: any }) {
  return (
    <div className="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors">
      <div className="flex items-center space-x-4">
        <img
          src={item.imgUrl || 'https://via.placeholder.com/60'}
          alt={item.productName || 'Product'}
          className="w-16 h-16 rounded object-cover"
          onError={(e) => {
            (e.target as HTMLImageElement).src = 'https://via.placeholder.com/60';
          }}
        />
        <div>
          <h3 className="font-medium text-gray-800">
            {item.productName || `Product: ${item.productID.slice(0, 8)}...`}
          </h3>
          <p className="text-sm text-gray-600">Qty: {item.quantity}</p>
          <p className="text-sm text-gray-600">Price: {formatCurrency(item.unitPrice)}</p>
        </div>
      </div>
      <div className="text-right">
        <p className="font-semibold text-green-600">{formatCurrency(item.totalPrice)}</p>
      </div>
    </div>
  );
}