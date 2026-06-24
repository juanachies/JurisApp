import type { ApiError } from './types'

const TOKEN_KEY = 'jurisapp_token'

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function setToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token)
}

export function clearToken(): void {
  localStorage.removeItem(TOKEN_KEY)
}

const API_BASE = import.meta.env.VITE_API_URL ?? ''

function getErrorMessage(status: number, data: unknown): string {
  if (typeof data === 'string' && data) return data
  if (data && typeof data === 'object') {
    const obj = data as Record<string, unknown>
    if (typeof obj.message === 'string') return obj.message
    if (typeof obj.title === 'string') return obj.title
    if (Array.isArray(obj.errors)) return obj.errors.join(', ')
  }
  switch (status) {
    case 401:
      return 'No autorizado. Iniciá sesión nuevamente.'
    case 403:
      return 'No tenés permiso para esta acción.'
    case 404:
      return 'Recurso no encontrado.'
    case 409:
      return 'Conflicto: el recurso ya existe o no está disponible.'
    case 502:
      return 'Error del servicio de IA. Intentá nuevamente.'
    default:
      return 'Ocurrió un error inesperado.'
  }
}

export async function apiClient<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const headers: Record<string, string> = {
    ...(options.headers as Record<string, string>),
  }

  const token = getToken()
  if (token) {
    headers['Authorization'] = `Bearer ${token}`
  }

  if (options.body && !(options.body instanceof FormData)) {
    headers['Content-Type'] = 'application/json'
  }

  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  })

  const contentType = res.headers.get('content-type') ?? ''
  const text = await res.text()
  let data: unknown = null

  if (text) {
    data = contentType.includes('application/json')
      ? (() => {
          try {
            return JSON.parse(text)
          } catch {
            return text
          }
        })()
      : text
  }

  if (!res.ok) {
    const error: ApiError = {
      message: getErrorMessage(res.status, data),
      status: res.status,
      data,
    }
    throw error
  }

  return data as T
}
