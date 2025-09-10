import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environment';
import { CartItem } from '../models/cart-item';
import { Observable } from 'rxjs';
import { OrderResponse } from '../models/order-response';
import { NewOrderRequest } from '../models/new-order-request';
import { UsersService } from './users.service';

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private ordersAPIURL: string = environment.ordersAPIURL;
  private cart: CartItem[] = [];
  private storageKey = 'cart';

  constructor(private http: HttpClient, private usersService: UsersService) {
    this.loadCartFromStorage();
  }

  private loadCartFromStorage(): void {
    try {
      const savedCart = localStorage.getItem(this.storageKey);
      if (savedCart) {
        this.cart = JSON.parse(savedCart);
      }
    } catch (error) {
      console.error('Error loading cart from storage:', error);
      this.cart = [];
    }
  }

  private saveCartToStorage(): void {
    try {
      localStorage.setItem(this.storageKey, JSON.stringify(this.cart));
    } catch (error) {
      console.error('Error saving cart to storage:', error);
    }
  }

  addCartItem(cartItem: CartItem): void {
    const existingItem = this.cart.find(item => item.productID === cartItem.productID);
    if (existingItem) {
      existingItem.quantity += cartItem.quantity || 1;
    } else {
      this.cart.push({ ...cartItem, quantity: cartItem.quantity || 1 });
    }
    this.saveCartToStorage();
  }

  incrementQuantity(productID: string): void {
    const item = this.cart.find(i => i.productID === productID);
    if (item) {
      item.quantity++;
      this.saveCartToStorage();
    }
  }

  decrementQuantity(productID: string): void {
    const item = this.cart.find(i => i.productID === productID);
    if (item && item.quantity > 1) {
      item.quantity--;
      this.saveCartToStorage();
    }
  }

  removeItemCompletely(productID: string): void {
    this.cart = this.cart.filter(item => item.productID !== productID);
    this.saveCartToStorage();
  }

  clearCartItems(): void {
    this.cart = [];
    this.saveCartToStorage();
  }

  getCartItems(): CartItem[] {
    return [...this.cart];
  }

  newOrder(): Observable<OrderResponse> {
    const newOrderRequest: NewOrderRequest = {
      userID: this.usersService.authResponse?.userID || '',
      orderDate: new Date(),
      orderItems: this.cart.map(cartItem => ({
        productID: cartItem.productID,
        productName: cartItem.productName,
        unitPrice: cartItem.unitPrice,
        quantity: cartItem.quantity,
        category: cartItem.category,
        imgUrl: cartItem.imgUrl || ''
      }))
    };

    return this.http.post<OrderResponse>(`${this.ordersAPIURL}`, newOrderRequest);
  }

  getOrders(): Observable<OrderResponse[]> {
    return this.http.get<OrderResponse[]>(`${this.ordersAPIURL}`);
  }

  getOrdersByUserID(userID: string): Observable<OrderResponse[]> {
    return this.http.get<OrderResponse[]>(`${this.ordersAPIURL}search/userid/${userID}`);
  }
}
