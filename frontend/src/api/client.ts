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

  if (res.status === 401) {
    unauthorizedHandler?.()
    throw new ApiError('Tu sesión venció. Volvé a iniciar sesión.', 401, 'Unauthorized')
  }

  const text = await res.text()
  const data = text ? (JSON.parse(text) as unknown) : undefined

  if (!res.ok) {
    const err = data as ApiErrorBody | undefined
    throw new ApiError(
      err?.message || 'No pudimos completar la operación. Reintentá o volvé a intentarlo más tarde.',
      res.status,
      err?.code,
      data,
    )
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
