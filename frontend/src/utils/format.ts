import type {
  AITaskStatus,
  AITaskStepStatus,
  DocumentAnalysisType,
  LawyerVerificationStatus,
  PlanType,
  SubscriptionStatus,
  UserRole,
} from '@/types/api'

const dateFormatter = new Intl.DateTimeFormat('es-AR', {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
})

const dateTimeFormatter = new Intl.DateTimeFormat('es-AR', {
  day: 'numeric',
  month: 'short',
  hour: '2-digit',
  minute: '2-digit',
})

const currencyFormatter = new Intl.NumberFormat('es-AR', {
  style: 'currency',
  currency: 'USD',
  minimumFractionDigits: 0,
  maximumFractionDigits: 2,
})

export function formatDate(iso: string) {
  return dateFormatter.format(new Date(iso))
}

export function formatDateTime(iso: string) {
  return dateTimeFormatter.format(new Date(iso))
}

export function formatPrice(price: number) {
  if (price === 0) return 'Gratis'
  return currencyFormatter.format(price)
}

export function fullName(firstName: string, lastName: string) {
  return `${firstName} ${lastName}`.trim()
}

export function greetingForNow() {
  const hour = new Date().getHours()
  if (hour < 12) return 'Buenos días'
  if (hour < 19) return 'Buenas tardes'
  return 'Buenas noches'
}

export function roleLabel(role: UserRole) {
  switch (role) {
    case 'Admin':
      return 'Administrador'
    case 'Lawyer':
      return 'Abogado'
    default:
      return 'Usuario'
  }
}

export function verificationLabel(status: LawyerVerificationStatus) {
  switch (status) {
    case 'Pending':
      return 'Pendiente de verificación'
    case 'Verified':
      return 'Verificado'
    case 'Rejected':
      return 'Rechazada'
    default:
      return 'Sin solicitar'
  }
}

export function planTypeLabel(type: PlanType) {
  switch (type) {
    case 'Pro':
      return 'Pro'
    case 'Max':
      return 'Max'
    default:
      return 'Free'
  }
}

export function subscriptionLabel(status: SubscriptionStatus) {
  switch (status) {
    case 'Active':
      return 'Activa'
    case 'Cancelled':
      return 'Cancelada'
    default:
      return 'Vencida'
  }
}

export function analysisTypeLabel(type: DocumentAnalysisType) {
  switch (type) {
    case 'Summary':
      return 'Resumen'
    case 'RiskAnalysis':
      return 'Riesgos'
    case 'Recommendations':
      return 'Recomendaciones'
    case 'ContractReview':
      return 'Revisión contractual'
    case 'Custom':
      return 'Análisis personalizado'
  }
}

export function taskStatusLabel(status: AITaskStatus, isPaused?: boolean) {
  if (isPaused && status === 'InProgress') return 'Pausada'
  switch (status) {
    case 'AwaitingApproval':
      return 'Pendiente de aprobación'
    case 'InProgress':
      return 'En ejecución'
    case 'Completed':
      return 'Completada'
    case 'Failed':
      return 'Fallida'
    case 'Cancelled':
      return 'Cancelada'
    default:
      return 'Pendiente'
  }
}

export function stepStatusLabel(status: AITaskStepStatus) {
  switch (status) {
    case 'InProgress':
      return 'En curso'
    case 'Completed':
      return 'Completado'
    case 'Failed':
      return 'Falló'
    case 'Skipped':
      return 'Omitido'
    default:
      return 'Pendiente'
  }
}

export function parseLimits(limitsJson: string): { chats: number; documents: number; aiTasks: number } {
  try {
    const parsed = JSON.parse(limitsJson) as Record<string, unknown>
    return {
      chats: typeof parsed.chats === 'number' ? parsed.chats : 0,
      documents: typeof parsed.documents === 'number' ? parsed.documents : 0,
      aiTasks: typeof parsed.aiTasks === 'number' ? parsed.aiTasks : 0,
    }
  } catch {
    return { chats: 0, documents: 0, aiTasks: 0 }
  }
}

export function limitLabel(value: number) {
  if (value < 0) return 'Ilimitado'
  return String(value)
}

export function startOfDay(date: Date) {
  const copy = new Date(date)
  copy.setHours(0, 0, 0, 0)
  return copy
}

export function chatDateGroup(iso: string): 'Hoy' | 'Esta semana' | 'Anteriores' {
  const created = startOfDay(new Date(iso)).getTime()
  const today = startOfDay(new Date()).getTime()
  const day = 24 * 60 * 60 * 1000
  if (created === today) return 'Hoy'
  if (created > today - 7 * day) return 'Esta semana'
  return 'Anteriores'
}

export function fileExtension(name: string) {
  const idx = name.lastIndexOf('.')
  if (idx < 0) return ''
  return name.slice(idx + 1).toUpperCase()
}
