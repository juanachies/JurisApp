import { apiClient } from '@/api/client'
import type { ChatDto, ChatSummaryDto, CreateChatRequest, MessageDto, SendMessageRequest } from '@/types/api'

export const chatsApi = {
  list: () => apiClient<ChatSummaryDto[]>('/api/chats'),

  getById: (id: string) => apiClient<ChatDto>(`/api/chats/${id}`),

  create: (data: CreateChatRequest) =>
    apiClient<ChatDto>('/api/chats', { method: 'POST', json: data }),

  sendMessage: (id: string, data: SendMessageRequest) =>
    apiClient<MessageDto>(`/api/chats/${id}/messages`, { method: 'POST', json: data }),

  delete: (id: string) => apiClient<void>(`/api/chats/${id}`, { method: 'DELETE' }),
}
