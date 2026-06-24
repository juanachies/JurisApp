import { apiClient } from './client'
import type {
  AITaskDetailDto,
  AITaskDto,
  CreateAITaskRequest,
  UpdateAITaskPlanRequest,
} from './types'

export const aiTasksApi = {
  listByChat: (chatId: string) =>
    apiClient<AITaskDto[]>(`/api/ai-tasks/chat/${chatId}`),

  getById: (id: string) => apiClient<AITaskDetailDto>(`/api/ai-tasks/${id}`),

  create: (data: CreateAITaskRequest) =>
    apiClient<AITaskDetailDto>('/api/ai-tasks', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  updatePlan: (id: string, data: UpdateAITaskPlanRequest) =>
    apiClient<AITaskDetailDto>(`/api/ai-tasks/${id}/plan`, {
      method: 'PUT',
      body: JSON.stringify(data),
    }),

  approve: (id: string) =>
    apiClient<AITaskDetailDto>(`/api/ai-tasks/${id}/approve`, {
      method: 'POST',
    }),

  pause: (id: string) =>
    apiClient<AITaskDetailDto>(`/api/ai-tasks/${id}/pause`, {
      method: 'POST',
    }),

  resume: (id: string) =>
    apiClient<AITaskDetailDto>(`/api/ai-tasks/${id}/resume`, {
      method: 'POST',
    }),

  cancel: (id: string) =>
    apiClient<AITaskDetailDto>(`/api/ai-tasks/${id}/cancel`, {
      method: 'POST',
    }),
}
