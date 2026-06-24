import { apiClient } from './client'
import type {
  CreateLawyerProfileRequest,
  LawyerProfileDto,
  UpdateLawyerProfileRequest,
} from './types'

export const lawyerProfilesApi = {
  getMe: () => apiClient<LawyerProfileDto>('/api/lawyer-profiles/me'),

  create: (data: CreateLawyerProfileRequest) =>
    apiClient<LawyerProfileDto>('/api/lawyer-profiles', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  update: (data: UpdateLawyerProfileRequest) =>
    apiClient<LawyerProfileDto>('/api/lawyer-profiles/me', {
      method: 'PUT',
      body: JSON.stringify(data),
    }),
}
