import { apiClient } from '@/api/client'
import type { AITaskDto, CreateAITaskRequest, UpdateAITaskPlanRequest } from '@/types/api'

export const tasksApi = {
  create: (data: CreateAITaskRequest) =>
    apiClient<AITaskDto>('/api/ai-tasks', { method: 'POST', json: data }),

  getById: (id: string) => apiClient<AITaskDto>(`/api/ai-tasks/${id}`),

  listByChat: (chatId: string) => apiClient<AITaskDto[]>(`/api/ai-tasks/chat/${chatId}`),

  updatePlan: (id: string, data: UpdateAITaskPlanRequest) =>
    apiClient<AITaskDto>(`/api/ai-tasks/${id}/plan`, { method: 'PUT', json: data }),

  approve: (id: string) => apiClient<AITaskDto>(`/api/ai-tasks/${id}/approve`, { method: 'POST' }),

  pause: (id: string) => apiClient<AITaskDto>(`/api/ai-tasks/${id}/pause`, { method: 'POST' }),

  resume: (id: string) => apiClient<AITaskDto>(`/api/ai-tasks/${id}/resume`, { method: 'POST' }),

  cancel: (id: string) => apiClient<AITaskDto>(`/api/ai-tasks/${id}/cancel`, { method: 'POST' }),
}
