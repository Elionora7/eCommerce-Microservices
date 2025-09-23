import apiClient from '@/api/axios';          
import axios from 'axios';                    
import type { LoginRequest, RegisterRequest, AuthResponse } from './authTypes';

const USERS_BASE = import.meta.env.VITE_API_PATH_USERS || '/gateway/users';
const AUTH_BASE = `${USERS_BASE}/auth`; // /gateway/users/auth

export const authAPI = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    try {
      const response = await apiClient.post<AuthResponse>(`${AUTH_BASE}/login`, data);  // /gateway/users/auth/login
      return response.data;
    } catch (err: unknown) {
    
      if (axios.isAxiosError(err) && err.response?.status === 401) {
        throw new Error('User does not exist. Please sign up first.');
      }
      // Re-throw any other errors
      throw err;
    }
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>(`${AUTH_BASE}/register`, data);
    return response.data; // /gateway/users/auth/register
  },

  refreshToken: async (refreshToken: string): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>(
      `${AUTH_BASE}/refresh-token`,
      { refreshToken }
    );
    return response.data;
  }
};
