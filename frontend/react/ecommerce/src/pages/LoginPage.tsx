import React, { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { authAPI } from '@/features/auth/authAPI';
import type { LoginRequest, AuthResponse } from '@/features/auth/authTypes';
import { useNavigate } from 'react-router-dom';

const LoginPage: React.FC = () => {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [formError, setFormError] = useState('');
  
  // Use mutation for login
  const loginMutation = useMutation<AuthResponse, Error, LoginRequest>({
  mutationFn: authAPI.login,
  onSuccess: (data) => {
    // clear any old error
    setFormError('');

    // Save tokens and user data
    localStorage.setItem('token', data.token);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem(
      'refreshTokenExpiryTime',
      new Date(data.refreshTokenExpiryTime).toISOString()
    );

    localStorage.setItem(
      'user',
      JSON.stringify({
        email: data.email,
        name: data.name,
        gender: data.gender,
        userID: data.userID,
      })
    );

    // Redirect to dashboard
    navigate('/dashboard');
  },
  onError: (error) => {
    // show the friendly text returned by authAPI or fallback
    setFormError(
      error.message || 'Login failed. Please check your credentials.'
    );
    console.error('Login failed:', error.message);
  },
});


  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    loginMutation.mutate({ email, password });
  };

  return (
    <div className="flex justify-center items-center min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 px-4">
      <div className="bg-white p-6 md:p-8 rounded-xl shadow-lg w-full max-w-md border border-gray-200">
        <h2 className="text-2xl md:text-3xl font-bold mb-2 text-center text-gray-800">Welcome Back</h2>
        <p className="text-gray-600 text-center mb-6 md:mb-8">Sign in to your account</p>
        
        <form onSubmit={handleSubmit} className="space-y-4 md:space-y-6">
          <div>
            <label htmlFor="email" className="block text-sm font-medium mb-2 text-gray-700">
              Email
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full border border-gray-300 p-3 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition"
              placeholder="Enter your email"
            />
          </div>
          
          <div>
            <label htmlFor="password" className="block text-sm font-medium mb-2 text-gray-700">
              Password
            </label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full border border-gray-300 p-3 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition"
              placeholder="Enter your password"
            />
          </div>
          
          <button
            type="submit"
            disabled={loginMutation.isPending}
            className="w-full bg-blue-600 text-white p-3 rounded-md hover:bg-blue-700 disabled:opacity-50 transition flex items-center justify-center"
          >
            {loginMutation.isPending ? (
              <>
                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Logging in...
              </>
            ) : 'Login'}
          </button>
          
          {formError && (
              <div className="bg-red-50 text-red-700 p-3 rounded-lg flex items-center">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  className="h-5 w-5 mr-2"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                >
                  <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                    clipRule="evenodd"
                  />
                </svg>
                {formError}{' '}
                <a href="/register" className="ml-1 text-blue-600 hover:underline">
                  Register here
                </a>
              </div>
            )}

        </form>
        
        <div className="mt-6 text-center text-sm text-gray-600">
          <p>Don't have an account? <a href="/register" className="text-blue-600 hover:underline">Sign up</a></p>
        </div>
      </div>
    </div>
  );
};

export default LoginPage;