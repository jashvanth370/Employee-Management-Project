import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

interface LoginResponse {
  token: string;
  userId: number;
  email: string;
  name: string;
}

interface JwtPayload {
  unique_name: string;
  UserId: string;
  exp: number;
}

@Injectable({ providedIn: 'root' })
export class Auth {
  private baseUrl = 'https://localhost:5224/api/Auth';
  private tokenKey = 'jwtToken';

  constructor(private http: HttpClient) { }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getUserId(): number | null {
    const token = this.getToken();
    if (!token) return null;

    const decoded: JwtPayload = jwtDecode(token);
    return parseInt(decoded.UserId);
  }

  register(user: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/register`, user);
  }

  login(user: any): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/login`, user);
  }

  googleLogin(token: string): Observable<LoginResponse> {
    console.log("google");
    return this.http.post<LoginResponse>(`${this.baseUrl}/google`, { token });
  }

  microsoftLogin(token: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/microsoft`, { token });
  }

  saveToken(token: string) {
    localStorage.setItem(this.tokenKey, token);
  }

  logout() {
    localStorage.removeItem(this.tokenKey);
  }
}