import { apiClient } from './client'
import type {
  ApplyCustomSkillToChatRequest,
  CreateCustomSkillRequest,
  CustomSkillDto,
  UpdateCustomSkillRequest,
} from './types'

export const skillsApi = {
  list: () => apiClient<CustomSkillDto[]>('/api/custom-skills'),

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
