export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  gender: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  name: string;
  gender: string;
  userID: string;
  success: boolean;
  refreshToken: string;
  refreshTokenExpiryTime: string;
}
