import { apiClient } from '@/api/client'
import type { AdminUpdateUserRequest, UpdateUserProfileRequest, UserDto } from '@/types/api'

export const usersApi = {
  me: () => apiClient<UserDto>('/api/users/me'),

  updateMe: (data: UpdateUserProfileRequest) =>
    apiClient<UserDto>('/api/users/me', { method: 'PUT', json: data }),

  list: () => apiClient<UserDto[]>('/api/users'),

  getById: (id: string) => apiClient<UserDto>(`/api/users/${id}`),

  adminUpdate: (id: string, data: AdminUpdateUserRequest) =>
    apiClient<UserDto>(`/api/users/${id}`, { method: 'PUT', json: data }),
}
