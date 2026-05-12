import { apiClient } from './api'

export interface LoginRequest { email: string; password: string }
export interface RegisterRequest { email: string; password: string; displayName: string }

export const authService = {
  login: (data: LoginRequest) =>
    apiClient.post('/api/v1/auth/login', data).then((r) => r.data),

  register: (data: RegisterRequest) =>
    apiClient.post('/api/v1/auth/register', data).then((r) => r.data),

  refresh: (token: string) =>
    apiClient.post('/api/v1/auth/refresh', { token }).then((r) => r.data),
}
