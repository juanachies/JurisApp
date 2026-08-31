import { apiClient } from '@/api/client'
import type {
  ApplyCustomSkillToChatRequest,
  CreateCustomSkillRequest,
  CustomSkillDto,
  UpdateCustomSkillRequest,
} from '@/types/api'

export const skillsApi = {
  list: () => apiClient<CustomSkillDto[]>('/api/custom-skills'),

  create: (data: CreateCustomSkillRequest) =>
    apiClient<CustomSkillDto>('/api/custom-skills', { method: 'POST', json: data }),

  update: (id: string, data: UpdateCustomSkillRequest) =>
    apiClient<CustomSkillDto>(`/api/custom-skills/${id}`, { method: 'PUT', json: data }),

  applyToChat: (data: ApplyCustomSkillToChatRequest) =>
    apiClient<void>('/api/custom-skills/apply', { method: 'POST', json: data }),

  removeFromChat: (data: ApplyCustomSkillToChatRequest) =>
    apiClient<void>('/api/custom-skills/remove', { method: 'POST', json: data }),

  activate: (id: string) =>
    apiClient<CustomSkillDto>(`/api/custom-skills/${id}/activate`, { method: 'POST' }),

  deactivate: (id: string) =>
    apiClient<CustomSkillDto>(`/api/custom-skills/${id}/deactivate`, { method: 'POST' }),

  delete: (id: string) => apiClient<void>(`/api/custom-skills/${id}`, { method: 'DELETE' }),
}
