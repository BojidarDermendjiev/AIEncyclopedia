import { apiClient } from './api'

export interface UserProfile {
  id: string
  email: string
  displayName: string
  avatarUrl: string | null
  subscriptionTier: string
  isAdmin: boolean
  createdAt: string
}

export interface BookmarkDto {
  articleId: string
  title: string
  slug: string
  summary: string | null
  readingTimeMinutes: number
  bookmarkedAt: string
}

export interface ReadingHistoryDto {
  articleId: string
  title: string
  slug: string
  progressPercent: number
  lastReadAt: string
}

export const usersService = {
  getProfile: async () => {
    const { data } = await apiClient.get<{ success: boolean; data: UserProfile }>('/api/v1/users/me')
    return data.data
  },

  getBookmarks: async () => {
    const { data } = await apiClient.get<{ success: boolean; data: BookmarkDto[] }>('/api/v1/users/bookmarks')
    return data.data
  },

  addBookmark: async (articleId: string) => {
    await apiClient.post(`/api/v1/users/bookmarks/${articleId}`)
  },

  removeBookmark: async (articleId: string) => {
    await apiClient.delete(`/api/v1/users/bookmarks/${articleId}`)
  },

  getHistory: async (limit = 20) => {
    const { data } = await apiClient.get<{ success: boolean; data: ReadingHistoryDto[] }>('/api/v1/users/history', {
      params: { limit },
    })
    return data.data
  },

  recordHistory: async (articleId: string, progressPercent: number) => {
    await apiClient.post('/api/v1/users/history', { articleId, progressPercent })
  },
}
