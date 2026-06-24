import { apiClient } from './client'
import type { CurrentPlanDto, PlanDto } from './types'

export const plansApi = {
  list: () => apiClient<PlanDto[]>('/api/plans'),

  getCurrent: () => apiClient<CurrentPlanDto>('/api/plans/current'),

  subscribeFree: (planId: string) =>
    apiClient<void>(`/api/plans/${planId}/subscribe`, { method: 'POST' }),
}
