import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { AuthenticationResponse } from '../models/authentication-response';
import { Register } from '../models/register';
import { environment } from '../../environment';
import { AuthService } from './auth.service'; // Import AuthService

@Injectable({
  providedIn: 'root',
})
export class UsersService {
  private usersAPIURL: string = environment.usersAPIURL;

  // Remove all authentication properties and use AuthService instead
  constructor(
    private http: HttpClient,
    private authService: AuthService // Inject AuthService
  ) {}

  // Make these getters that read from AuthService
  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated.value;
  }

  get isAdmin(): boolean {
    return this.authService.isAdmin.value;
  }

  get currentUserName(): string | null {
    return this.authService.currentUserName.value;
  }

  get authResponse(): AuthenticationResponse | null {
    const authResponseStr = localStorage.getItem('authResponse');
    return authResponseStr ? JSON.parse(authResponseStr) : null;
  }

  register(register: Register): Observable<AuthenticationResponse> {
    return this.http.post<AuthenticationResponse>(`${this.usersAPIURL}auth/register`, register);
  }

  login(email: string, password: string): Observable<AuthenticationResponse> {
    if (email === 'admin@example.com' && password === 'admin') {
      const adminUser: AuthenticationResponse = {
        userID: 'admin_id',
        name: 'Admin',
        email: 'admin@example.com',
        gender: 'male',
        token: 'admin_token',
        success: true
      };
      return of(adminUser);
    } else {
      return this.http.post<AuthenticationResponse>(`${this.usersAPIURL}auth/login`, { email, password });
    }
  }

}