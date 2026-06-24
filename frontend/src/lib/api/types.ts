export type UserRole = 'User' | 'Lawyer' | 'Admin'

export interface UserDto {
  id: string
  firstName: string
  lastName: string
  email: string
  role: UserRole
  isActive: boolean
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

export interface UpdateUserProfileRequest {
  firstName: string
  lastName: string
  email: string
}

export interface AdminUpdateUserRequest {
  firstName?: string
  lastName?: string
  email?: string
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

export type MessageRole = 'User' | 'Assistant' | 'System'

export interface MessageDto {
  id: string
  chatId: string
  role: MessageRole
  content: string
  date: string
  skillsUsed?: string[]
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
  chatId: string
  folderId?: string | null
  title: string
  url: string
}

export type DocumentAnalysisType = 'Summary' | 'RiskAnalysis' | 'ContractReview' | 'Custom'

export interface AnalyzeDocumentRequest {
  documentId: string
  type: DocumentAnalysisType
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

export interface LawyerProfileDto {
  id: string
  userId: string
  licenseNumber: string
  barAssociation: string
  province: string
  specialty: string
  isVerified: boolean
  verificationStatus: string
  verifiedAt?: string | null
}

export interface CreateLawyerProfileRequest {
  licenseNumber: string
  barAssociation: string
  province: string
  specialty: string
}

export interface UpdateLawyerProfileRequest {
  licenseNumber: string
  barAssociation: string
  province: string
  specialty: string
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

export type AITaskStatus =
  | 'Pending'
  | 'AwaitingApproval'
  | 'InProgress'
  | 'Completed'
  | 'Failed'
  | 'Cancelled'

export type AITaskStepStatus = 'Pending' | 'InProgress' | 'Completed' | 'Failed' | 'Skipped'

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
}

export interface AITaskDetailDto extends AITaskDto {
  steps: TaskStepDto[]
}

export interface CreateAITaskRequest {
  chatId: string
  description: string
}

export interface UpdateAITaskPlanStep {
  order: number
  title: string
  description: string
}

export interface UpdateAITaskPlanRequest {
  steps: UpdateAITaskPlanStep[]
}

export type PlanType = 'Free' | 'Pro' | 'Max'

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
  subscriptionStatus?: string | null
  startDate?: string | null
}

export interface CreateCheckoutSessionRequest {
  planId: string
}

export interface CreateCheckoutSessionResponse {
  url: string
}

export interface ApiError {
  message: string
  status: number
  data?: unknown
}
