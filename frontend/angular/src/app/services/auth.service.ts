import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { AuthenticationResponse } from '../models/authentication-response';
import { environment } from '../../environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.usersAPIURL}auth/`;
  private tokenSubject = new BehaviorSubject<string | null>(this.getToken());
  private refreshTokenSubject = new BehaviorSubject<string | null>(this.getRefreshToken());
  public isAuthenticated = new BehaviorSubject<boolean>(this.isLoggedIn());
  public isAdmin = new BehaviorSubject<boolean>(this.getIsAdmin());
  public currentUserName = new BehaviorSubject<string | null>(this.getCurrentUserName());

  constructor(private http: HttpClient, private router: Router) {}

  login(credentials: { email: string; password: string }): Observable<AuthenticationResponse> {
    return this.http.post<AuthenticationResponse>(`${this.apiUrl}login`, credentials).pipe(
      tap(response => {
        this.setAuthData(response, response.token);
        this.tokenSubject.next(response.token);
      })
    );
  }

  register(userData: any): Observable<AuthenticationResponse> {
    return this.http.post<AuthenticationResponse>(`${this.apiUrl}register`, userData).pipe(
      tap(response => {
        this.setAuthData(response, response.token);
        this.tokenSubject.next(response.token);
      })
    );
  }

  refreshToken(refreshToken: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}refresh`, { refreshToken }).pipe(
      tap(response => {
        if (response.token) {
          localStorage.setItem('token', response.token);
          this.tokenSubject.next(response.token);
        }
      })
    );
  }

   setAuthData(response: AuthenticationResponse, token: string): void {
    localStorage.setItem('token', token);
    localStorage.setItem('authResponse', JSON.stringify(response));
    localStorage.setItem('currentUserName', response.name);
    localStorage.setItem('isAdmin', (response.userID === 'admin_id').toString());
    
    this.isAuthenticated.next(true);
    this.currentUserName.next(response.name);
    this.isAdmin.next(response.userID === 'admin_id');
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

  getIsAdmin(): boolean {
    const isAdmin = localStorage.getItem('isAdmin');
    return isAdmin ? isAdmin.toLowerCase() === 'true' : false;
  }

  getCurrentUserName(): string | null {
    return localStorage.getItem('currentUserName');
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('authResponse');
    localStorage.removeItem('currentUserName');
    localStorage.removeItem('isAdmin');
    
    this.tokenSubject.next(null);
    this.isAuthenticated.next(false);
    this.currentUserName.next(null);
    this.isAdmin.next(false);
    
    this.router.navigate(['/auth/login']);
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    return !!token;
  }


}