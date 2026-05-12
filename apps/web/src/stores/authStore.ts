import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface UserState {
  id: string
  email: string
  displayName: string
  avatarUrl?: string
  subscriptionTier: 'free' | 'pro' | 'enterprise'
  isAdmin: boolean
}

interface AuthStore {
  user: UserState | null
  accessToken: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  setAuth: (user: UserState, accessToken: string, refreshToken: string) => void
  clearAuth: () => void
  updateUser: (partial: Partial<UserState>) => void
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,

      setAuth: (user, accessToken, refreshToken) =>
        set({ user, accessToken, refreshToken, isAuthenticated: true }),

      clearAuth: () =>
        set({ user: null, accessToken: null, refreshToken: null, isAuthenticated: false }),

      updateUser: (partial) =>
        set((state) => ({ user: state.user ? { ...state.user, ...partial } : null })),
    }),
    {
      name: 'ai-encyclopedia-auth',
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
    },
  ),
)
