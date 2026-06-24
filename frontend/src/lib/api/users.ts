import { apiClient } from './client'
import type {
  AdminUpdateUserRequest,
  UpdateUserProfileRequest,
  UserDto,
} from './types'

export const usersApi = {
  getMe: () => apiClient<UserDto>('/api/users/me'),

  updateMe: (data: UpdateUserProfileRequest) =>
    apiClient<UserDto>('/api/users/me', {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  list: () => apiClient<UserDto[]>('/api/users'),

  getById: (id: string) => apiClient<UserDto>(`/api/users/${id}`),

  adminUpdate: (id: string, data: AdminUpdateUserRequest) =>
    apiClient<UserDto>(`/api/users/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  adminDelete: (id: string) =>
    apiClient<void>(`/api/users/${id}`, { method: 'DELETE' }),
}
