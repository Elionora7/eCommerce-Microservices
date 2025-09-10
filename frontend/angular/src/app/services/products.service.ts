import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map,tap } from 'rxjs/operators';
import { environment } from '../../environment';
import { Product } from '../models/product.model'; 
import { ProductUpdateRequest } from '../models/product-update-request';
import { NewProductRequest } from '../models/new-product-request';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root',
})
export class ProductsService {
  private productsAPIURL: string = environment.productsAPIURL;

  constructor(private http: HttpClient, private authService: AuthService) {}

  // Helper method to map API response to Product format
  private mapToProduct(data: any): Product {
    return {
      id: data.productID || data.productId || data.id || '',
      name: data.productName || data.name || '—',
      category: data.category || '—',
      unitPrice: Number.parseFloat(data.unitPrice || data.price || 0) || 0,
      quantity: Number.parseInt(
        data.quantityInStock || data.stockQuantity || data.stock || data.quantity || 0, 
        10
      ) || 0,
      imgUrl: data.imgUrl || data.imageUrl || ''
    };
  }

  //Get all products
  getProducts(): Observable<Product[]> {
    return this.http.get<any[]>(`${this.productsAPIURL}`).pipe(
      tap((products: any[]) => console.log('API Response:', products)), // Properly typed
      map((products: any[]) => products.map((product: any) => this.mapToProduct(product)))
    );
  }
  

  // Search products
  searchProducts(searchString: string): Observable<Product[]> {
    return this.http.get<any[]>(`${this.productsAPIURL}search/${searchString}`).pipe(
      map(products => products.map(product => this.mapToProduct(product)))
    );
  }

  // Get product by ID
  getProductByProductID(productID: string): Observable<Product> {
    return this.http.get<any>(`${this.productsAPIURL}search/product-id/${productID}`).pipe(
      map(product => this.mapToProduct(product))
    );
  }

  // Get product by ID (alternative method name for consistency)
  getProductById(id: string): Observable<Product> {
    return this.getProductByProductID(id);
  }

  // Update product
  updateProduct(productUpdateRequest: ProductUpdateRequest): Observable<any> {
    return this.http.put<any>(`${this.productsAPIURL}`, productUpdateRequest);
  }

  // Delete product
  deleteProduct(productID: string): Observable<boolean> {
    return this.http.delete<boolean>(`${this.productsAPIURL}${productID}`);
  }

  // Create new product
  createProduct(newProductRequest: NewProductRequest): Observable<any> {
    return this.http.post<any>(`${this.productsAPIURL}`, newProductRequest);
  }
}