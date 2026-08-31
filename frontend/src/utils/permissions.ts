import type { LawyerProfileDto, UserDto, UserRole } from '@/types/api'

export function isAdmin(user?: UserDto | null) {
  return user?.role === 'Admin'
}

export function isLawyerRole(user?: UserDto | null) {
  return user?.role === 'Lawyer'
}

export function isVerifiedLawyer(user?: UserDto | null, profile?: LawyerProfileDto | null) {
  if (!user) return false
  if (user.role === 'Lawyer' && profile?.isVerified) return true
  return Boolean(profile?.isVerified && (user.role === 'Lawyer' || user.role === 'Admin'))
}

export function canManageCases(user?: UserDto | null, profile?: LawyerProfileDto | null) {
  return isVerifiedLawyer(user, profile)
}

export function canManageSkills(user?: UserDto | null, profile?: LawyerProfileDto | null) {
  return isVerifiedLawyer(user, profile)
}

export function canAccessAdmin(user?: UserDto | null) {
  return isAdmin(user)
}

export function isPaidPlan(type: string | undefined) {
  return type === 'Pro' || type === 'Max'
}

export function roleAllowsFolders(role?: UserRole) {
  return role === 'Lawyer' || role === 'Admin'
}
