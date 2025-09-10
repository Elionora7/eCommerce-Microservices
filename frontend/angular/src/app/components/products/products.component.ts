import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Product } from '../../models/product.model';
import { ProductsService } from '../../services/products.service';
import { UsersService } from '../../services/users.service';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatCardModule } from '@angular/material/card';
import { Router, RouterModule } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [
    CommonModule, 
    MatCardModule, 
    MatDividerModule, 
    MatButtonModule, 
    RouterModule, 
    MatTableModule, 
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class ProductsComponent {
  products: Product[] = []; 
  displayedColumns: string[] = ['name', 'category', 'unitPrice', 'quantity', 'actions']; 
  isLoading: boolean = true;

  constructor(
    private productsService: ProductsService, 
    public usersService: UsersService, 
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.isLoading = true;
    this.productsService.getProducts().subscribe({
      next: (response: Product[]) => { 
        this.products = response;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading products:', err);
        this.isLoading = false;
      }
    });
  }

  edit(product: Product): void { 
    this.router.navigate(['/products', 'edit', product.id]);
  }

  delete(product: Product): void { 
    this.router.navigate(['/products', 'delete', product.id]); 
  }

  viewDetails(product: Product): void {
    this.router.navigate(['/products', 'details', product.id]);
  }

  handleImageError(event: any): void {
      event.target.src = 'assets/placeholder.jpg';
  }
}