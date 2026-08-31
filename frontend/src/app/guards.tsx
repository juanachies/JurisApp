import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '@/app/AuthContext'
import { Spinner } from '@/components/ui/Loading'

export function BootScreen() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-canvas">
      <div className="flex items-center gap-3 text-muted">
        <Spinner />
        <span className="text-[14px]">Cargando JurisApp…</span>
      </div>
    </div>
  )
}

export function RequireAuth() {
  const { isAuthenticated, isLoading } = useAuth()
  const location = useLocation()

  if (isLoading) return <BootScreen />
  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname, session: true }} />
  }
  return <Outlet />
}

export function GuestOnly() {
  const { isAuthenticated, isLoading } = useAuth()
  if (isLoading) return <BootScreen />
  if (isAuthenticated) return <Navigate to="/app" replace />
  return <Outlet />
}

export function RequireLawyer() {
  const { canManageCases, isLoading } = useAuth()
  if (isLoading) return <BootScreen />
  if (!canManageCases) return <Navigate to="/app/professional-verification" replace />
  return <Outlet />
}

export function RequireAdmin() {
  const { isAdmin, isLoading } = useAuth()
  if (isLoading) return <BootScreen />
  if (!isAdmin) return <Navigate to="/app" replace />
  return <Outlet />
}
