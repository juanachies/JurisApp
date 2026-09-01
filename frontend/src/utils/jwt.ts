import type { UserRole } from '@/types/api'

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

function parseRole(value: unknown): UserRole | null {
  if (Array.isArray(value)) {
    for (const item of value) {
      const role = parseRole(item)
      if (role) return role
    }
    return null
  }
  if (value === 'User' || value === 'Lawyer' || value === 'Admin') return value
  return null
}

export function readJwtRole(token: string | null | undefined): UserRole | null {
  if (!token) return null
  try {
    const segment = token.split('.')[1]
    if (!segment) return null
    const padded = segment.replace(/-/g, '+').replace(/_/g, '/') + '='.repeat((4 - (segment.length % 4)) % 4)
    const payload = JSON.parse(atob(padded)) as Record<string, unknown>
    return parseRole(payload.role) ?? parseRole(payload[ROLE_CLAIM])
  } catch {
    return null
  }
}
