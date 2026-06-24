import { apiClient } from './client'
import type {
  ApplyCustomSkillToChatRequest,
  CreateCustomSkillRequest,
  CustomSkillDto,
  UpdateCustomSkillRequest,
} from './types'

export const skillsApi = {
  listMine: () => apiClient<CustomSkillDto[]>('/api/custom-skills/me'),

  create: (data: CreateCustomSkillRequest) =>
    apiClient<CustomSkillDto>('/api/custom-skills', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (id: string, data: UpdateCustomSkillRequest) =>
    apiClient<CustomSkillDto>(`/api/custom-skills/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  activate: (id: string) =>
    apiClient<void>(`/api/custom-skills/${id}/activate`, { method: 'POST' }),

  deactivate: (id: string) =>
    apiClient<void>(`/api/custom-skills/${id}/deactivate`, { method: 'POST' }),

  applyToChat: (data: ApplyCustomSkillToChatRequest) =>
    apiClient<void>('/api/custom-skills/apply', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  removeFromChat: (data: ApplyCustomSkillToChatRequest) =>
    apiClient<void>('/api/custom-skills/remove', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  delete: (id: string) =>
    apiClient<void>(`/api/custom-skills/${id}`, { method: 'DELETE' }),
}
