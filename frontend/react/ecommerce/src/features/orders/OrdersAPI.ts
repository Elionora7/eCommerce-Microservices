import axios from '@/api/axios';
import type { Order, OrderItem, OrderAddRequest, OrderUpdateRequest } from './OrderTypes';

const BASE = import.meta.env.VITE_API_PATH_ORDERS || '/gateway/orders';

const mapDtoToOrderItem = (d: any): OrderItem => ({
  productID: d.productID ?? d.productId ?? '',
  productName: d.productName ?? d.productName ?? '',
  unitPrice: Number.parseFloat(d.unitPrice ?? d.price ?? 0) || 0,
  quantity: Number.parseInt(d.quantity ?? 0, 10) || 0,
  totalPrice: Number.parseFloat(d.totalPrice ?? 0) || 0,
  imgUrl: d.imgUrl ?? d.imageUrl ?? '',
});

const mapDtoToOrder = (d: any): Order => ({
  _id: d._id ?? d.id ?? '',
  orderID: d.orderID ?? d.orderId ?? '',
  userID: d.userID ?? d.userId ?? '',
  orderDate: d.orderDate ?? '',
  totalBill: Number.parseFloat(d.totalBill ?? d.totalPrice ?? 0) || 0,
  orderItems: (d.orderItems ?? []).map(mapDtoToOrderItem),
  status: d.status ?? 'completed',//to add later to the backend -for now give default value
});

export const ordersAPI = {
  // Get all orders
  list: async (): Promise<Order[]> => {
    const res = await axios.get(BASE);
    return (res.data ?? []).map(mapDtoToOrder);
  },

  // Get order by ID
  getById: async (id: string): Promise<Order> => {
    const res = await axios.get(`${BASE}/search/orderid/${id}`);
    return mapDtoToOrder(res.data ?? {});
  },

  // Get orders by user ID
  getByUserId: async (userId: string): Promise<Order[]> => {
    const res = await axios.get(`${BASE}/search/userid/${userId}`);
    return (res.data ?? []).map(mapDtoToOrder);
  },

  // Get orders by product ID
  getByProductId: async (productId: string): Promise<Order[]> => {
    const res = await axios.get(`${BASE}/search/productid/${productId}`);
    return (res.data ?? []).map(mapDtoToOrder);
  },

  // Get orders by date
  getByDate: async (date: string): Promise<Order[]> => {
    const res = await axios.get(`${BASE}/search/orderDate/${date}`);
    return (res.data ?? []).map(mapDtoToOrder);
  },

  // Create new order
  create: async (orderData: OrderAddRequest): Promise<Order> => {
    const res = await axios.post(BASE, orderData);
    return mapDtoToOrder(res.data);
  },

  // Update order
  update: async (id: string, orderData: OrderUpdateRequest): Promise<Order> => {
    const res = await axios.put(`${BASE}/${id}`, orderData);
    return mapDtoToOrder(res.data);
  },

  // Delete order
  delete: async (id: string): Promise<void> => {
    await axios.delete(`${BASE}/${id}`);
  }
};