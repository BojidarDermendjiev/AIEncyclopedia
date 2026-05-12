import { apiClient } from './api'

export type PdfStyle = 'Academic' | 'Student' | 'Presentation' | 'Summary'

export interface UserPdfDto {
  id: string
  articleId: string
  articleTitle: string
  articleSlug: string
  style: PdfStyle
  fileSizeBytes: number
  downloadCount: number
  generatedAt: string
}

export const pdfsService = {
  generate: async (articleId: string, style: PdfStyle) => {
    const { data } = await apiClient.post<{ id: string; fileSizeBytes: number; exportStyle: string; generatedAt: string }>(
      '/api/v1/pdfs/generate',
      { articleId, style }
    )
    return data
  },

  download: async (pdfId: string): Promise<Blob> => {
    const { data } = await apiClient.get(`/api/v1/pdfs/${pdfId}/download`, { responseType: 'blob' })
    return data
  },

  getLibrary: async () => {
    const { data } = await apiClient.get<{ success: boolean; data: UserPdfDto[] }>('/api/v1/pdfs/library')
    return data.data
  },
}
