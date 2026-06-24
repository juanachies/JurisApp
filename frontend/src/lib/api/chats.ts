import { apiClient } from './client'
import type {
  ChatDto,
  ChatSummaryDto,
  CreateChatRequest,
  MessageDto,
  SendMessageRequest,
} from './types'

export const chatsApi = {
  list: () => apiClient<ChatSummaryDto[]>('/api/chats'),

  getById: (id: string) => apiClient<ChatDto>(`/api/chats/${id}`),

  create: (data: CreateChatRequest) =>
    apiClient<ChatDto>('/api/chats', {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  sendMessage: (chatId: string, data: SendMessageRequest) =>
    apiClient<MessageDto>(`/api/chats/${chatId}/messages`, {
      method: 'POST',
      body: JSON.stringify(data),
    }),

  delete: (id: string) =>
    apiClient<void>(`/api/chats/${id}`, { method: 'DELETE' }),
}
