import { apiClient } from './api'

export interface SearchResult {
  id: string
  title: string
  slug: string
  summary: string | null
  topicTitle: string
  categoryName: string
  readingTimeMinutes: number
  publishedAt: string | null
  rank: number
}

export interface SearchResponse {
  items: SearchResult[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNext: boolean
  hasPrev: boolean
}

export const searchService = {
  search: async (q: string, page = 1, pageSize = 20) => {
    const { data } = await apiClient.get<{ success: boolean; data: SearchResponse }>('/api/v1/search', {
      params: { q, page, pageSize },
    })
    return data.data
  },
}
