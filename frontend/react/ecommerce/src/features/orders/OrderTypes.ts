export interface CartItem {
  productID: string;
  name: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
  imgUrl: string;
  maxStock: number;
}

export interface Cart {
  items: CartItem[];
  total: number;
  itemCount: number;
}

export interface OrderItem {
  productID: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
  imgUrl: string;
}

export interface Order {
  _id: string;
  orderID: string;
  userID: string;
  orderDate: string;
  totalBill: number;
  orderItems: OrderItem[];
  status: 'pending' | 'completed' | 'cancelled';
}

export interface OrderAddRequest {
  UserID: string;
  OrderItems: {
    ProductID: string;
    UnitPrice: number;
    Quantity: number;
    TotalPrice: number;
    ImgUrl: string;
  }[];
  OrderDate?: string; 
  status?: string;

}

export interface OrderUpdateRequest {
  OrderID: string;
  UserID: string;
  OrderItems: {
    ProductID: string;
    UnitPrice: number;
    Quantity: number;
    TotalPrice: number;
    ImgUrl: string ; 
  }[];
  OrderDate?: string; 
  status?: string;
}

export interface OrderResponse {
  _id: string;
  orderID: string;
  userID: string;
  orderDate: string;
  totalBill: number;
  orderItems: OrderItem[];
}