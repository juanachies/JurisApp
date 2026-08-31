export type UserRole = 'User' | 'Lawyer' | 'Admin'
export type UserTheme = 'Bright' | 'Dark'
export type MessageRole = 'User' | 'Assistant' | 'System'
export type DocumentAnalysisType =
  | 'Summary'
  | 'RiskAnalysis'
  | 'Recommendations'
  | 'ContractReview'
  | 'Custom'
export type AITaskStatus =
  | 'Pending'
  | 'AwaitingApproval'
  | 'InProgress'
  | 'Completed'
  | 'Failed'
  | 'Cancelled'
export type AITaskStepStatus = 'Pending' | 'InProgress' | 'Completed' | 'Failed' | 'Skipped'
export type LawyerVerificationStatus = 'NotSubmitted' | 'Pending' | 'Verified' | 'Rejected'
export type PlanType = 'Free' | 'Pro' | 'Max'
export type SubscriptionStatus = 'Active' | 'Cancelled' | 'Expired'

export interface UserDto {
  id: string
  firstName: string
  lastName: string
  email: string
  role: UserRole
  isActive: boolean
  isEmailVerified: boolean
  theme: UserTheme
  createdAt: string
}

export interface AuthResponse {
  token: string
  user: UserDto
}

export interface RegisterRequest {
  firstName: string
  lastName: string
  email: string
  password: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface VerifyEmailRequest {
  email: string
  code: string
}

export interface ResendVerificationRequest {
  email: string
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  token: string
  newPassword: string
}

export interface UpdateUserProfileRequest {
  firstName: string
  lastName: string
  theme: UserTheme
}

export interface AdminUpdateUserRequest {
  role?: UserRole
  isActive?: boolean
}

export interface ChatSummaryDto {
  id: string
  title: string
  createdAt: string
  folderId?: string | null
}

export interface ChatAppliedSkillDto {
  id: string
  name: string
}

export interface MessageDto {
  id: string
  chatId: string
  role: MessageRole
  content: string
  date: string
  skillsUsed: string[]
}

export interface ChatDto {
  id: string
  userId: string
  title: string
  folderId?: string | null
  appliedSkills: ChatAppliedSkillDto[]
  messages: MessageDto[]
}

export interface CreateChatRequest {
  title: string
  folderId?: string
}

export interface SendMessageRequest {
  content: string
}

export interface DocumentDto {
  id: string
  chatId?: string | null
  folderId?: string | null
  title: string
  url: string
}

export interface AnalyzeDocumentRequest {
  documentId: string
  type?: DocumentAnalysisType
  types?: DocumentAnalysisType[]
  customSkillIds?: string[]
}

export interface DocumentAnalysisDto {
  id: string
  documentId: string
  type: DocumentAnalysisType
  summary: string
  risks: string
  recommendations: string
  references: string
}

export interface FolderDto {
  id: string
  lawyerProfileId: string
  name: string
  legalContext?: string | null
}

export interface CreateFolderRequest {
  name: string
  legalContext?: string
}

export interface UpdateFolderRequest {
  name: string
  legalContext?: string
}

export interface CustomSkillDto {
  id: string
  lawyerProfileId: string
  name: string
  whenToUse: string
  instructions: string
  examples: string
  redFlags: string
  outputFormat: string
  isActive: boolean
}

export interface CreateCustomSkillRequest {
  lawyerProfileId: string
  name: string
  whenToUse: string
  instructions: string
  examples: string
  redFlags: string
  outputFormat: string
}

export interface UpdateCustomSkillRequest {
  name: string
  whenToUse: string
  instructions: string
  examples: string
  redFlags: string
  outputFormat: string
}

export interface ApplyCustomSkillToChatRequest {
  chatId: string
  customSkillId: string
}

export interface TaskStepDto {
  id: string
  order: number
  title: string
  description: string
  status: AITaskStepStatus
  result?: string | null
}

export interface AITaskDto {
  id: string
  chatId: string
  description: string
  status: AITaskStatus
  plan: string
  result?: string | null
  currentStepIndex: number
  isPaused: boolean
  steps: TaskStepDto[]
}

export interface CreateAITaskRequest {
  chatId: string
  description: string
}

export interface UpdateTaskStepRequest {
  order: number
  title: string
  description: string
}

export interface UpdateAITaskPlanRequest {
  steps: UpdateTaskStepRequest[]
}

export interface LawyerProfileDto {
  id: string
  userId: string
  licenseNumber: string
  barAssociation: string
  province: string
  specialty: string
  isVerified: boolean
  verificationStatus: LawyerVerificationStatus
  rejectionReason?: string | null
  verifiedAt?: string | null
  resolvedAt?: string | null
  licenseDocumentUrl?: string | null
}

export interface UpdateLawyerProfileRequest {
  licenseNumber: string
  barAssociation: string
  province: string
  specialty: string
}

export interface LawyerVerificationRequestSummaryDto {
  id: string
  userId: string
  userFirstName: string
  userLastName: string
  userEmail: string
  licenseNumber: string
  barAssociation: string
  province: string
  specialty: string
  verificationStatus: LawyerVerificationStatus
  createdAt: string
  verifiedAt?: string | null
  resolvedAt?: string | null
}

export interface LawyerVerificationRequestDetailDto {
  id: string
  userId: string
  userFirstName: string
  userLastName: string
  userEmail: string
  licenseNumber: string
  barAssociation: string
  province: string
  specialty: string
  verificationStatus: LawyerVerificationStatus
  isVerified: boolean
  rejectionReason?: string | null
  createdAt: string
  verifiedAt?: string | null
  resolvedAt?: string | null
  licenseDocumentUrl?: string | null
}

export interface RejectLawyerRequest {
  reason?: string
}

export interface PlanLimits {
  chats?: number
  documents?: number
  aiTasks?: number
}

export interface PlanDto {
  id: string
  name: string
  type: PlanType
  price: number
  limitsJson: string
}

export interface CurrentPlanDto {
  planId: string
  planName: string
  planType: PlanType
  price: number
  limitsJson: string
  hasActiveSubscription: boolean
  subscriptionStatus?: SubscriptionStatus | null
  startDate?: string | null
}

export interface SubscriptionDto {
  id: string
  userId: string
  planId: string
  startDate: string
  endDate?: string | null
  status: SubscriptionStatus
}

export interface CreatePlanRequest {
  name: string
  type: PlanType
  price: number
  limitsJson: string
}

export interface UpdatePlanRequest {
  name: string
  type: PlanType
  price: number
  limitsJson: string
}

export interface CreateCheckoutSessionRequest {
  planId: string
}

export interface CreateCheckoutSessionResponse {
  url: string
}

export interface ApiErrorBody {
  code?: string
  message?: string
}
