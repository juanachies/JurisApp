import { apiClient } from '@/api/client'
import type {
  LawyerProfileDto,
  LawyerVerificationRequestDetailDto,
  LawyerVerificationRequestSummaryDto,
  LawyerVerificationStatus,
  RejectLawyerRequest,
  UpdateLawyerProfileRequest,
} from '@/types/api'

export const lawyersApi = {
  getMe: () => apiClient<LawyerProfileDto>('/api/lawyer-profiles/me'),

  create: (fields: {
    licenseNumber: string
    barAssociation: string
    province: string
    specialty: string
    licenseDocument: File
  }) => {
    const form = new FormData()
    form.append('licenseNumber', fields.licenseNumber)
    form.append('barAssociation', fields.barAssociation)
    form.append('province', fields.province)
    form.append('specialty', fields.specialty)
    form.append('licenseDocument', fields.licenseDocument)
    return apiClient<LawyerProfileDto>('/api/lawyer-profiles', { method: 'POST', formData: form })
  },

  updateMe: (data: UpdateLawyerProfileRequest) =>
    apiClient<LawyerProfileDto>('/api/lawyer-profiles/me', { method: 'PUT', json: data }),

  listRequests: (status?: LawyerVerificationStatus) => {
    const query = status ? `?status=${encodeURIComponent(status)}` : ''
    return apiClient<LawyerVerificationRequestSummaryDto[]>(`/api/lawyer-profiles/requests${query}`)
  },

  getRequest: (id: string) =>
    apiClient<LawyerVerificationRequestDetailDto>(`/api/lawyer-profiles/requests/${id}`),

  approve: (id: string) =>
    apiClient<LawyerProfileDto>(`/api/lawyer-profiles/requests/${id}/approve`, { method: 'POST' }),

  reject: (id: string, data: RejectLawyerRequest) =>
    apiClient<LawyerProfileDto>(`/api/lawyer-profiles/requests/${id}/reject`, {
      method: 'POST',
      json: data,
    }),
}
