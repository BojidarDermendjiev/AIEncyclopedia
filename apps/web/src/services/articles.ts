import { apiClient } from './api'

export interface ArticlesParams {
  page?: number
  pageSize?: number
  categoryId?: string
  topicId?: string
  search?: string
  difficulty?: 'beginner' | 'intermediate' | 'advanced'
}

export const articlesService = {
  getAll: (params: ArticlesParams = {}) =>
    apiClient.get('/api/v1/articles', { params }).then((r) => r.data),

  getBySlug: (slug: string) =>
    apiClient.get(`/api/v1/articles/${slug}`).then((r) => r.data),
}
