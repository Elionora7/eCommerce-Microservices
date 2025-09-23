import axios from 'axios';

// Central Axios instance with global settings
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_GATEWAY_URL, // http://localhost:4500
  headers: {
    'Content-Type': 'application/json',
  },
});

// Global interceptors (auth, errors, etc.)
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem(import.meta.env.VITE_AUTH_TOKEN_NAME);
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});


// Add response interceptor to handle auth errors
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // Only redirect for other protected endpoints, not login/register
    if (
      error.response?.status === 401 &&
      !error.config.url?.includes('/auth/login') &&
      !error.config.url?.includes('/auth/register')
    ) {
      localStorage.removeItem(import.meta.env.VITE_AUTH_TOKEN_NAME);
      localStorage.removeItem('userID');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);


export default apiClient;