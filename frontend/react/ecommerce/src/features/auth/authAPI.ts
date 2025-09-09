import axios from '@/api/axios';
import type { LoginRequest, RegisterRequest, AuthResponse } from './authTypes';

const USERS_BASE = import.meta.env.VITE_API_PATH_USERS || '/gateway/users';
const AUTH_BASE = `${USERS_BASE}/auth`;  // /users/auth

export const authAPI = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await axios.post<AuthResponse>(
      `${AUTH_BASE}/login`,  // /gateway/users/auth/login
      data
    );
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await axios.post<AuthResponse>(
      `${AUTH_BASE}/register`,  // /gateway/users/auth/register
      data
    );
    return response.data;
  },

  refreshToken: async (refreshToken: string): Promise<AuthResponse> => {
    const response = await axios.post<AuthResponse>(
      `${AUTH_BASE}/refresh-token`,///gateway/users/auth/refresh-token
      { refreshToken }
    );
    return response.data;
  }
};