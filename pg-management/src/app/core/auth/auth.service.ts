import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginResponse {
  token: string;
  userName?: string;
  email?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private baseUrl = environment.apiBaseUrl;
  private readonly tokenKey = 'pg_token';
  private readonly userKey = 'pg_user';

  currentUser = signal<string | null>(this.getUser());

  login(email: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/auth/login`, { email, password }).pipe(
      tap((res) => {
        const token = res?.token || res?.data?.token || (typeof res === 'string' ? res : null);
        if (typeof token === 'string') {
          localStorage.setItem(this.tokenKey, token);
        }
        const user = res?.userName || res?.data?.userName || (email ? email.split('@')[0] : 'Admin');
        localStorage.setItem(this.userKey, user);
        this.currentUser.set(user);
      })
    );
  }

  register(req: any): Observable<any> {
    return this.http.post<any>(`${this.baseUrl}/auth/register`, req);
  }

  logout(): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.userKey);
    }
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getToken(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem(this.tokenKey);
    }
    return null;
  }

  private getUser(): string | null {
    if (typeof window !== 'undefined' && window.localStorage) {
      return localStorage.getItem(this.userKey) || 'Admin User';
    }
    return 'Admin User';
  }
}
