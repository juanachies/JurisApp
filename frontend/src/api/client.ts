import type { ApiErrorBody } from '@/types/api'

const TOKEN_KEY = 'jurisapp.token'

export class ApiError extends Error {
  readonly status: number
  readonly code?: string
  readonly body?: unknown

  constructor(message: string, status: number, code?: string, body?: unknown) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.code = code
    this.body = body
  }
}

let unauthorizedHandler: (() => void) | null = null

export function onUnauthorized(handler: () => void) {
  unauthorizedHandler = handler
}

export function getToken() {
  return localStorage.getItem(TOKEN_KEY)
}

export function setToken(token: string | null) {
  if (token) localStorage.setItem(TOKEN_KEY, token)
  else localStorage.removeItem(TOKEN_KEY)
}

export function apiBase() {
  return import.meta.env.VITE_API_URL?.replace(/\/$/, '') ?? ''
}

export function fileUrl(storedName: string | null | undefined) {
  if (!storedName) return null
  if (storedName.startsWith('http')) return storedName
  return `${apiBase()}/uploads/${storedName}`
}

type RequestOptions = RequestInit & {
  json?: unknown
  formData?: FormData
}

export async function apiClient<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers = new Headers(options.headers)
  const token = getToken()
  if (token) headers.set('Authorization', `Bearer ${token}`)

  let body = options.body
  if (options.json !== undefined) {
    headers.set('Content-Type', 'application/json')
    body = JSON.stringify(options.json)
  } else if (options.formData) {
    body = options.formData
  }

  const { json: _json, formData: _form, ...rest } = options
  void _json
  void _form

  const res = await fetch(`${apiBase()}${path}`, {
    ...rest,
    headers,
    body,
  })

  const text = await res.text()
  let data: unknown
  if (text) {
    try {
      data = JSON.parse(text) as unknown
    } catch {
      data = undefined
    }
  }

  const err = data as ApiErrorBody | undefined
  const parsedMessage = readErrorMessage(data)

  if (res.status === 401) {
    if (err?.code === 'Unauthorized' && parsedMessage) {
      throw new ApiError(parsedMessage, 401, err.code, data)
    }
    unauthorizedHandler?.()
    throw new ApiError('Tu sesión venció. Volvé a iniciar sesión.', 401, 'Unauthorized', data)
  }

  if (!res.ok) {
    const fallback =
      res.status === 403
        ? 'No tenés permiso para esta acción. Si te verificaron como abogado recién, cerrá sesión y volvé a entrar para renovar el acceso.'
        : 'No pudimos completar la operación. Reintentá o volvé a intentarlo más tarde.'
    throw new ApiError(parsedMessage || fallback, res.status, err?.code, data)
  }

  return data as T
}

export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError
}

export function errorMessage(error: unknown, fallback = 'No pudimos completar la operación.') {
  if (isApiError(error)) return error.message
  if (error instanceof Error && error.message) return error.message
  return fallback
}

function readErrorMessage(data: unknown): string | undefined {
  if (!data || typeof data !== 'object') return undefined
  const body = data as Record<string, unknown>
  if (typeof body.message === 'string' && body.message.trim()) return body.message
  if (typeof body.title === 'string' && body.title.trim()) return body.title
  const errors = body.errors
  if (errors && typeof errors === 'object') {
    for (const value of Object.values(errors as Record<string, unknown>)) {
      if (typeof value === 'string' && value.trim()) return value
      if (Array.isArray(value) && typeof value[0] === 'string' && value[0].trim()) return value[0]
    }
  }
  return undefined
}
