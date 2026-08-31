import { apiClient } from '@/api/client'
import type {
  CreatePlanRequest,
  CurrentPlanDto,
  PlanDto,
  SubscriptionDto,
  UpdatePlanRequest,
} from '@/types/api'

export const plansApi = {
  list: () => apiClient<PlanDto[]>('/api/plans'),

  current: () => apiClient<CurrentPlanDto>('/api/plans/current'),

  create: (data: CreatePlanRequest) =>
    apiClient<PlanDto>('/api/plans', { method: 'POST', json: data }),

  update: (id: string, data: UpdatePlanRequest) =>
    apiClient<PlanDto>(`/api/plans/${id}`, { method: 'PUT', json: data }),

  delete: (id: string) => apiClient<void>(`/api/plans/${id}`, { method: 'DELETE' }),

  subscribe: (planId: string) =>
    apiClient<SubscriptionDto>(`/api/plans/${planId}/subscribe`, { method: 'POST' }),

  change: (planId: string) =>
    apiClient<SubscriptionDto>(`/api/plans/${planId}/change`, { method: 'POST' }),

  cancel: () => apiClient<SubscriptionDto>('/api/plans/current/cancel', { method: 'POST' }),
}
