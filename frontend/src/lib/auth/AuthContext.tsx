import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import {
  authApi,
  clearToken,
  getToken,
  setToken,
  usersApi,
  type LoginRequest,
  type RegisterRequest,
  type UserDto,
} from '@/lib/api'

interface AuthContextValue {
  user: UserDto | null
  isLoading: boolean
  isAuthenticated: boolean
  login: (data: LoginRequest) => Promise<void>
  register: (data: RegisterRequest) => Promise<void>
  logout: () => void
  refreshUser: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [user, setUser] = useState<UserDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const refreshUser = useCallback(async () => {
    const token = getToken()
    if (!token) {
      setUser(null)
      return
    }
    try {
      const me = await usersApi.getMe()
      setUser(me)
    } catch {
      clearToken()
      setUser(null)
    }
  }, [])

  useEffect(() => {
    refreshUser().finally(() => setIsLoading(false))
  }, [refreshUser])

  const login = useCallback(async (data: LoginRequest) => {
    const res = await authApi.login(data)
    setToken(res.token)
    setUser(res.user)
  }, [])

  const register = useCallback(async (data: RegisterRequest) => {
    const res = await authApi.register(data)
    setToken(res.token)
    setUser(res.user)
  }, [])

  const logout = useCallback(() => {
    clearToken()
    setUser(null)
    queryClient.clear()
  }, [queryClient])

  const value = useMemo(
    () => ({
      user,
      isLoading,
      isAuthenticated: !!user,
      login,
      register,
      logout,
      refreshUser,
    }),
    [user, isLoading, login, register, logout, refreshUser],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
