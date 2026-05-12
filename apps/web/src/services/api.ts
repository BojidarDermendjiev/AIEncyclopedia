import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '../stores/authStore'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
  timeout: 30_000,
})

// Attach JWT to every request
apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = useAuthStore.getState().accessToken
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Refresh token on 401
let isRefreshing = false
let pendingQueue: Array<{ resolve: (v: unknown) => void; reject: (e: unknown) => void }> = []

apiClient.interceptors.response.use(
  (res) => res,
  async (error: AxiosError) => {
    const original = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    if (error.response?.status !== 401 || original._retry) {
      return Promise.reject(error)
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        pendingQueue.push({ resolve, reject })
      }).then(() => apiClient(original))
    }

    original._retry = true
    isRefreshing = true

    try {
      const refreshToken = useAuthStore.getState().refreshToken
      if (!refreshToken) throw new Error('No refresh token')

      const { data } = await axios.post(
        `${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'}/api/v1/auth/refresh`,
        { token: refreshToken },
      )

      const { accessToken, refreshToken: newRefreshToken } = data.data
      const user = useAuthStore.getState().user!
      useAuthStore.getState().setAuth(user, accessToken, newRefreshToken)

      pendingQueue.forEach((p) => p.resolve(null))
      pendingQueue = []
      return apiClient(original)
    } catch (err) {
      pendingQueue.forEach((p) => p.reject(err))
      pendingQueue = []
      useAuthStore.getState().clearAuth()
      window.location.href = '/login'
      return Promise.reject(err)
    } finally {
      isRefreshing = false
    }
  },
)

export const api = apiClient
