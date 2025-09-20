import { ordersAPI } from './OrdersAPI';
import type { Order, OrderAddRequest, OrderUpdateRequest,OrderItem } from './OrderTypes';

export class OrderService {
  // Get all orders
  static async getAllOrders(): Promise<Order[]> {
    return ordersAPI.list();
  }

  // Get order by ID
  static async getOrderById(id: string): Promise<Order> {
    return ordersAPI.getById(id);
  }

  // Get orders by user ID
  static async getOrdersByUserId(userId: string): Promise<Order[]> {
    return ordersAPI.getByUserId(userId);
  }

  // Get orders by product ID
  static async getOrdersByProductId(productId: string): Promise<Order[]> {
    return ordersAPI.getByProductId(productId);
  }

  // Get orders by date
  static async getOrdersByDate(date: string): Promise<Order[]> {
    return ordersAPI.getByDate(date);
  }

  // Create new order
  static async createOrder(orderData: OrderAddRequest): Promise<Order> {
    return ordersAPI.create(orderData);
  }

  // Update order
  static async updateOrder(id: string, orderData: OrderUpdateRequest): Promise<Order> {
    return ordersAPI.update(id, orderData);
  }

  // Delete order
  static async deleteOrder(id: string): Promise<void> {
    return ordersAPI.delete(id);
  }

  // Calculate total for order items (helper method)
  static calculateOrderTotal(items: OrderItem[]): number {
    return items.reduce((total, item) => total + item.totalPrice, 0);
  }

  // Validate order data (helper method)
  static validateOrder(orderData: OrderAddRequest): string[] {
    const errors: string[] = [];

    if (!orderData.UserID) {
      errors.push('User ID is required');
    }

    if (!orderData.OrderItems || orderData.OrderItems.length === 0) {
      errors.push('At least one order item is required');
    } else {
      orderData.OrderItems.forEach((item, index) => {
        if (!item.ProductID) {
          errors.push(`Product ID is required for item ${index + 1}`);
        }
        if (item.Quantity <= 0) {
          errors.push(`Quantity must be greater than 0 for item ${index + 1}`);
        }
        if (item.UnitPrice < 0) {
          errors.push(`Unit price cannot be negative for item ${index + 1}`);
        }
      });
    }

    return errors;
  }
}