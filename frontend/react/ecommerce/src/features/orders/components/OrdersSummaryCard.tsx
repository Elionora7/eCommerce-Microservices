import { useOrders } from '@/features/orders/OrderHooks';
import { formatCurrency } from '@/utils/formatCurrency';
import { formatDate } from '@/utils/formatDate';

export default function OrdersSummaryCard() {
  const { data: orders = [] } = useOrders();

  const totalRevenue = orders.reduce((sum, order) => sum + order.totalBill, 0);
  const averageOrderValue = orders.length > 0 ? totalRevenue / orders.length : 0;
  const totalItems = orders.reduce((sum, order) => sum + order.orderItems.length, 0);

  return (
    <div className="bg-gradient-to-r from-purple-600 to-blue-600 rounded-2xl p-6 text-white shadow-lg">
      <h3 className="text-lg font-bold mb-4">Orders Summary</h3>
      
      <div className="grid grid-cols-2 gap-4">
        <div className="text-center">
          <div className="text-2xl font-bold">{orders.length}</div>
          <div className="text-sm opacity-90">Total Orders</div>
        </div>
        
        <div className="text-center">
          <div className="text-2xl font-bold">{totalItems}</div>
          <div className="text-sm opacity-90">Items Sold</div>
        </div>
        
        <div className="text-center">
          <div className="text-2xl font-bold">{formatCurrency(totalRevenue)}</div>
          <div className="text-sm opacity-90">Revenue</div>
        </div>
        
        <div className="text-center">
          <div className="text-2xl font-bold">{formatCurrency(averageOrderValue)}</div>
          <div className="text-sm opacity-90">Avg. Order</div>
        </div>
      </div>

      {orders.length > 0 && (
        <div className="mt-4 pt-4 border-t border-white/20">
          <div className="text-sm opacity-90">
            Latest: {formatDate(orders[0].orderDate)}
          </div>
        </div>
      )}
    </div>
  );
}