import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '@/lib/auth/AuthContext'
import type { UserRole } from '@/lib/api'
import { Spinner } from '@/components/ui/Loading'

export function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAuth()
  const location = useLocation()

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center app-background">
        <Spinner size="lg" />
      </div>
    )
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return <Outlet />
}

export function GuestRoute() {
  const { isAuthenticated, isLoading } = useAuth()
  const location = useLocation()
  const from = (location.state as { from?: { pathname: string } })?.from?.pathname ?? '/app/dashboard'

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center app-background">
        <Spinner size="lg" />
      </div>
    )
  }

  if (isAuthenticated) {
    return <Navigate to={from} replace />
  }

  return <Outlet />
}

interface RoleRouteProps {
  roles: UserRole[]
}

export function RoleRoute({ roles }: RoleRouteProps) {
  const { user, isLoading } = useAuth()

  if (isLoading) {
    return (
      <div className="flex min-h-[40vh] items-center justify-center">
        <Spinner />
      </div>
    )
  }

  if (!user || !roles.includes(user.role)) {
    return <Navigate to="/app/dashboard" replace />
  }

  return <Outlet />
}
