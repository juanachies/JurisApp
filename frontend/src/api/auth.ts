import { apiClient } from '@/api/client'
import type {
  AuthResponse,
  ForgotPasswordRequest,
  LoginRequest,
  RegisterRequest,
  ResendVerificationRequest,
  ResetPasswordRequest,
  VerifyEmailRequest,
} from '@/types/api'

export const authApi = {
  register: (data: RegisterRequest) =>
    apiClient<AuthResponse>('/api/auth/register', { method: 'POST', json: data }),

  login: (data: LoginRequest) =>
    apiClient<AuthResponse>('/api/auth/login', { method: 'POST', json: data }),

  verifyEmail: (data: VerifyEmailRequest) =>
    apiClient<AuthResponse>('/api/auth/verify-email', { method: 'POST', json: data }),

  resendVerification: (data: ResendVerificationRequest) =>
    apiClient<void>('/api/auth/resend-verification', { method: 'POST', json: data }),

  forgotPassword: (data: ForgotPasswordRequest) =>
    apiClient<void>('/api/auth/forgot-password', { method: 'POST', json: data }),

  resetPassword: (data: ResetPasswordRequest) =>
    apiClient<void>('/api/auth/reset-password', { method: 'POST', json: data }),
}
