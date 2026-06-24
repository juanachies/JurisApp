import { apiClient } from './client'
import type {
  CreateFolderRequest,
  FolderDto,
  UpdateFolderRequest,
} from './types'

export const foldersApi = {
  list: () => apiClient<FolderDto[]>('/api/folders'),

  create: (data: CreateFolderRequest) =>
    apiClient<FolderDto>('/api/folders', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id: string, data: UpdateFolderRequest) =>
    apiClient<FolderDto>(`/api/folders/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  delete: (id: string) =>
    apiClient<void>(`/api/folders/${id}`, { method: 'DELETE' }),
}
