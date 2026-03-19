import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User, LoginRequest, LoginResponse, RegisterRequest } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly tokenKey = 'jwt_token';
  private readonly userKey = 'user_data';

  currentUser = signal<User | null>(null);
  isAuthenticated = signal<boolean>(false);

  constructor(private http: HttpClient) {
    this.loadUserFromStorage();
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        console.log('Login response:', response);

        const token = response.token;
        if (!token) {
          console.error('No token in response:', response);
          throw new Error('No token received from server');
        }

        this.setToken(token);

        const user: User = {
          id: response.userId,
          userName: response.userName,
          email: response.userName, // Backend login response doesn't include email
          role: response.roles && response.roles.length > 0
            ? (response.roles[0] as 'Admin' | 'Reviewer')
            : 'Reviewer'
        };

        console.log('Setting user:', user);
        this.setUser(user);
        this.isAuthenticated.set(true);
        console.log('Authentication state:', this.isAuthenticated());
      })
    );
  }

  register(userData: RegisterRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/register`, userData).pipe(
      tap(response => {
        console.log('Register response:', response);

        const token = response.token;
        if (!token) {
          console.error('No token in response:', response);
          throw new Error('No token received from server');
        }

        this.setToken(token);

        const user: User = {
          id: response.userId,
          userName: response.userName,
          email: userData.email,
          role: response.roles && response.roles.length > 0
            ? (response.roles[0] as 'Admin' | 'Reviewer')
            : userData.role
        };

        this.setUser(user);
        this.isAuthenticated.set(true);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUser.set(null);
    this.isAuthenticated.set(false);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getUser(): User | null {
    return this.currentUser();
  }

  isAdmin(): boolean {
    return this.currentUser()?.role === 'Admin';
  }

  isReviewer(): boolean {
    const user = this.currentUser();
    return user?.role === 'Reviewer' || user?.role === 'Admin';
  }

  private setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  private setUser(user: User): void {
    localStorage.setItem(this.userKey, JSON.stringify(user));
    this.currentUser.set(user);
  }

  private loadUserFromStorage(): void {
    const token = this.getToken();
    const userStr = localStorage.getItem(this.userKey);
    
    if (token && userStr) {
      try {
        const user = JSON.parse(userStr) as User;
        this.currentUser.set(user);
        this.isAuthenticated.set(true);
      } catch {
        this.logout();
      }
    }
  }
}
