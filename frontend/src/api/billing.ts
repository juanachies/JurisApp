import { apiClient } from '@/api/client'
import type {
  CreateCheckoutSessionRequest,
  CreateCheckoutSessionResponse,
  SubscriptionDto,
} from '@/types/api'

export const billingApi = {
  createCheckoutSession: (data: CreateCheckoutSessionRequest) =>
    apiClient<CreateCheckoutSessionResponse>('/api/billing/create-checkout-session', {
      method: 'POST',
      json: data,
    }),

  simulatePurchase: (data: CreateCheckoutSessionRequest) =>
    apiClient<SubscriptionDto>('/api/billing/simulate-purchase', {
      method: 'POST',
      json: data,
    }),

  confirmCheckout: (sessionId?: string | null) =>
    apiClient<SubscriptionDto | undefined>('/api/billing/confirm', {
      method: 'POST',
      json: { sessionId: sessionId || undefined },
    }),
}
