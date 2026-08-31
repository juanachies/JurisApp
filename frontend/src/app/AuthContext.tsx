import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { authApi, lawyersApi, usersApi } from '@/api'
import { getToken, onUnauthorized, setToken } from '@/api/client'
import { queryKeys } from '@/api/queryKeys'
import { ApiError } from '@/api/client'
import type { AuthResponse, LawyerProfileDto, UserDto } from '@/types/api'
import { canManageCases, isAdmin, isVerifiedLawyer } from '@/utils/permissions'

type AuthContextValue = {
  user: UserDto | null
  profile: LawyerProfileDto | null
  isLoading: boolean
  isAuthenticated: boolean
  isAdmin: boolean
  isVerifiedLawyer: boolean
  canManageCases: boolean
  login: (email: string, password: string) => Promise<UserDto>
  register: (data: {
    firstName: string
    lastName: string
    email: string
    password: string
  }) => Promise<UserDto>
  applyAuth: (response: AuthResponse) => void
  logout: () => void
  refreshUser: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

function applyTheme(theme: UserDto['theme'] | undefined) {
  document.documentElement.dataset.theme = theme === 'Dark' ? 'dark' : 'bright'
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [token, setTokenState] = useState<string | null>(() => getToken())

  const persistToken = useCallback((next: string | null) => {
    setToken(next)
    setTokenState(next)
  }, [])

  useEffect(() => {
    onUnauthorized(() => {
      persistToken(null)
      queryClient.clear()
    })
  }, [persistToken, queryClient])

  const meQuery = useQuery({
    queryKey: queryKeys.me,
    queryFn: usersApi.me,
    retry: false,
    enabled: Boolean(token),
  })

  const profileQuery = useQuery({
    queryKey: queryKeys.lawyerProfile,
    queryFn: async () => {
      try {
        return await lawyersApi.getMe()
      } catch (error) {
        if (error instanceof ApiError && error.status === 404) return null
        throw error
      }
    },
    retry: false,
    enabled: Boolean(token) && Boolean(meQuery.data),
  })

  useEffect(() => {
    applyTheme(meQuery.data?.theme)
  }, [meQuery.data?.theme])

  const applyAuth = useCallback(
    (response: AuthResponse) => {
      persistToken(response.token)
      queryClient.setQueryData(queryKeys.me, response.user)
      applyTheme(response.user.theme)
    },
    [persistToken, queryClient],
  )

  const login = useCallback(
    async (email: string, password: string) => {
      const response = await authApi.login({ email, password })
      applyAuth(response)
      await queryClient.invalidateQueries({ queryKey: queryKeys.lawyerProfile })
      return response.user
    },
    [applyAuth, queryClient],
  )

  const register = useCallback(
    async (data: { firstName: string; lastName: string; email: string; password: string }) => {
      const response = await authApi.register(data)
      applyAuth(response)
      return response.user
    },
    [applyAuth],
  )

  const logout = useCallback(() => {
    persistToken(null)
    queryClient.clear()
    applyTheme('Bright')
  }, [persistToken, queryClient])

  const refreshUser = useCallback(async () => {
    await queryClient.invalidateQueries({ queryKey: queryKeys.me })
    await queryClient.invalidateQueries({ queryKey: queryKeys.lawyerProfile })
  }, [queryClient])

  const user = token ? (meQuery.data ?? null) : null
  const profile = user ? (profileQuery.data ?? null) : null
  const waitingMe = Boolean(token) && meQuery.isLoading
  const waitingProfile = Boolean(user) && profileQuery.isLoading

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      profile,
      isLoading: waitingMe || waitingProfile,
      isAuthenticated: Boolean(user),
      isAdmin: isAdmin(user),
      isVerifiedLawyer: isVerifiedLawyer(user, profile),
      canManageCases: canManageCases(user, profile),
      login,
      register,
      applyAuth,
      logout,
      refreshUser,
    }),
    [user, profile, waitingMe, waitingProfile, login, register, applyAuth, logout, refreshUser],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
