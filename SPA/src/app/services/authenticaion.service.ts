import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

export class LoginRequest {
  userName: string = '';
  password: string = '';

  constructor(userName: string, password: string) {
    this.userName = userName;
    this.password = password;
  }
}

export interface AuthenticationResponse {
  isAuthSuccessfully: boolean;
  errorMessage: string;
  token: TokenDto;
}

export interface RefreshTokenResponse {
  isSuccess: boolean;
  message: string;
  token: TokenDto;
}

export interface TokenDto {
  accessToken: string;
  refreshToken: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthenticaionService {

  private controller: string = 'Authentication';
  private apiUrl = environment.apiUrl;
  constructor(private _http: HttpClient) { }

  login(request: LoginRequest) {
    return this._http.post<AuthenticationResponse>(`${this.apiUrl}${this.controller}/login`, request, { withCredentials: true })
  }

  logout() {
    return this._http.post(`${this.apiUrl}${this.controller}/logout`, {}, { withCredentials: true })
  }

  refreshToken() {
    return this._http.post<RefreshTokenResponse>(`${this.apiUrl}${this.controller}/refresh`, {}, { withCredentials: true })
  }
}
