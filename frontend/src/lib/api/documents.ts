import { apiClient } from './client'
import type {
  AnalyzeDocumentRequest,
  DocumentAnalysisDto,
  DocumentDto,
} from './types'

export const documentsApi = {
  getById: (id: string) => apiClient<DocumentDto>(`/api/documents/${id}`),

  listByChat: (chatId: string) =>
    apiClient<DocumentDto[]>(`/api/documents/chat/${chatId}`),

  upload: (file: File, chatId: string, folderId?: string) => {
    const form = new FormData()
    form.append('file', file)
    form.append('chatId', chatId)
    if (folderId) form.append('folderId', folderId)
    return apiClient<DocumentDto>('/api/documents/upload', {
      method: 'POST',
      body: form,
    })
  },

  analyze: (data: AnalyzeDocumentRequest) =>
    apiClient<DocumentAnalysisDto>('/api/documents/analyze', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
}
