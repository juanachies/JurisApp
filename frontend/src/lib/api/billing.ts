import { apiClient } from './client'
import type {
  CreateCheckoutSessionRequest,
  CreateCheckoutSessionResponse,
} from './types'

export const billingApi = {
  createCheckoutSession: (data: CreateCheckoutSessionRequest) =>
    apiClient<CreateCheckoutSessionResponse>(
      '/api/billing/create-checkout-session',
      {
        method: 'POST',
        body: JSON.stringify(data),
      },
    ),

  simulatePurchase: (data: CreateCheckoutSessionRequest) =>
    apiClient<void>('/api/billing/simulate-purchase', {
      method: 'POST',
      body: JSON.stringify(data),
    }),
}
