import { apiClient } from '@/api/client'
import type { AnalyzeDocumentRequest, DocumentAnalysisDto, DocumentDto } from '@/types/api'

export const documentsApi = {
  upload: (file: File, destination: { chatId?: string; folderId?: string }) => {
    const form = new FormData()
    form.append('file', file)
    if (destination.chatId) form.append('chatId', destination.chatId)
    if (destination.folderId) form.append('folderId', destination.folderId)
    return apiClient<DocumentDto>('/api/documents/upload', { method: 'POST', formData: form })
  },

  getById: (id: string) => apiClient<DocumentDto>(`/api/documents/${id}`),

  listByChat: (chatId: string) => apiClient<DocumentDto[]>(`/api/documents/chat/${chatId}`),

  listByFolder: (folderId: string) => apiClient<DocumentDto[]>(`/api/documents/folder/${folderId}`),

  analyze: (data: AnalyzeDocumentRequest) =>
    apiClient<DocumentAnalysisDto>('/api/documents/analyze', { method: 'POST', json: data }),
}
