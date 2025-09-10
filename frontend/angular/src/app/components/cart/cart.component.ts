import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CartItem } from '../../models/cart-item';
import { CartService } from '../../services/cart.service';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterModule } from '@angular/router';
import { UsersService } from '../../services/users.service';
import { OrderResponse } from '../../models/order-response';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatIconModule,
    MatButtonModule,
    RouterModule
  ],
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent implements OnInit {
  cartItems: CartItem[] = [];
  displayedColumns = ['image', 'productName', 'unitPrice', 'quantity', 'total'];
  cartItemsDataSource: MatTableDataSource<CartItem> = new MatTableDataSource<CartItem>([]);

  constructor(
    private cartService: CartService,
    public usersService: UsersService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCartItems();
  }

  private loadCartItems(): void {
    this.cartItems = this.cartService.getCartItems();
    this.cartItemsDataSource.data = this.cartItems;
  }

  increaseQuantity(item: CartItem): void {
    this.cartService.incrementQuantity(item.productID);
    this.cartItemsDataSource.data = [...this.cartItems];
  }

  decreaseQuantity(item: CartItem): void {
    if (item.quantity > 1) {
      this.cartService.decrementQuantity(item.productID);
      this.cartItemsDataSource.data = [...this.cartItems];
    }
  }

  removeCartItem(productID: string): void {
    this.cartService.removeItemCompletely(productID);
    this.cartItems = this.cartItems.filter(i => i.productID !== productID);
    this.cartItemsDataSource.data = [...this.cartItems];
  }

  getTotal(): number {
    return this.cartItems.reduce((total, item) => total + (item.unitPrice * item.quantity), 0);
  }

  handleImageError(event: any): void {
    if (event.target.src.includes('placeholder.jpg')) return;
    event.target.src = 'assets/placeholder.jpg';
  }

  placeOrder(): void {
    this.cartService.newOrder().subscribe({
      next: (response: OrderResponse) => {
        if (response) {
          this.cartService.clearCartItems();
          this.loadCartItems();
          this.router.navigate(['orders']);
        }
      },
      error: (err: any) => {
        console.error('Order error:', err);
      }
    });
  }
}
