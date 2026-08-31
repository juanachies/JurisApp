import { apiClient } from '@/api/client'
import type { CreateFolderRequest, FolderDto, UpdateFolderRequest } from '@/types/api'

export const foldersApi = {
  list: () => apiClient<FolderDto[]>('/api/folders'),

  create: (data: CreateFolderRequest) =>
    apiClient<FolderDto>('/api/folders', { method: 'POST', json: data }),

  update: (id: string, data: UpdateFolderRequest) =>
    apiClient<FolderDto>(`/api/folders/${id}`, { method: 'PUT', json: data }),

  delete: (id: string) => apiClient<void>(`/api/folders/${id}`, { method: 'DELETE' }),
}
