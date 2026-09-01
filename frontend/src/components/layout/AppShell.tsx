import { useState, type ReactNode } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import {
  Briefcase,
  CreditCard,
  FileText,
  Home,
  LayoutDashboard,
  Menu,
  MessageSquare,
  Shield,
  User,
  Wand2,
  X,
} from 'lucide-react'
import { useAuth } from '@/app/AuthContext'
import { Logo } from '@/components/brand/Logo'
import { Button } from '@/components/ui/Button'
import { cn } from '@/utils/cn'
import { fullName, roleLabel } from '@/utils/format'

type NavItem = { to: string; label: string; icon: typeof Home; end?: boolean }

export function AppShell() {
  const { user, canManageCases, isAdmin, logout, sessionNeedsRelogin } = useAuth()
  const [open, setOpen] = useState(false)
  const navigate = useNavigate()

  const work: NavItem[] = [
    { to: '/app', label: 'Inicio', icon: Home, end: true },
    ...(canManageCases ? [{ to: '/app/cases', label: 'Casos', icon: Briefcase }] : []),
    { to: '/app/chats', label: 'Chats', icon: MessageSquare },
    { to: '/app/documents', label: 'Documentos', icon: FileText },
  ]

  const ia: NavItem[] = canManageCases ? [{ to: '/app/skills', label: 'Skills', icon: Wand2 }] : []

  const account: NavItem[] = [
    { to: '/app/subscription', label: 'Plan', icon: CreditCard },
    { to: '/app/profile', label: 'Perfil', icon: User },
  ]

  const sidebar = (
    <div className="flex h-full flex-col">
      <div className="px-4 py-4">
        <Logo to="/app" light />
      </div>
      <nav className="flex-1 space-y-6 overflow-y-auto px-3 pb-4">
        <NavGroup items={work} onNavigate={() => setOpen(false)} />
        {ia.length ? (
          <div>
            <p className="px-2 pb-2 text-[10px] font-medium uppercase tracking-wider text-white/35">IA</p>
            <NavGroup items={ia} onNavigate={() => setOpen(false)} />
          </div>
        ) : null}
        <div>
          <p className="px-2 pb-2 text-[10px] font-medium uppercase tracking-wider text-white/35">Cuenta</p>
          <NavGroup items={account} onNavigate={() => setOpen(false)} />
        </div>
        {isAdmin ? (
          <NavLink
            to="/admin"
            onClick={() => setOpen(false)}
            className="flex items-center gap-2 rounded-[8px] px-2 py-2 text-[14px] text-white/70 hover:bg-white/8 hover:text-white"
          >
            <Shield size={16} />
            Administración
          </NavLink>
        ) : null}
      </nav>
      <div className="border-t border-white/10 px-4 py-4">
        <p className="truncate text-[13px] font-medium text-white">
          {user ? fullName(user.firstName, user.lastName) : ''}
        </p>
        <p className="text-[12px] text-white/50">{user ? roleLabel(user.role) : ''}</p>
        <button
          type="button"
          className="mt-3 text-[13px] text-white/60 hover:text-white"
          onClick={() => {
            logout()
            navigate('/login')
          }}
        >
          Cerrar sesión
        </button>
      </div>
    </div>
  )

  return (
    <div className="flex min-h-screen bg-canvas">
      <aside className="hidden w-[240px] shrink-0 bg-navy-900 md:block">{sidebar}</aside>
      {open ? (
        <div className="fixed inset-0 z-50 md:hidden">
          <button type="button" className="absolute inset-0 bg-navy-950/50" aria-label="Cerrar menú" onClick={() => setOpen(false)} />
          <aside className="relative h-full w-[260px] bg-navy-900">{sidebar}</aside>
        </div>
      ) : null}
      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex items-center gap-3 border-b border-border bg-surface px-4 py-3 md:hidden">
          <button type="button" onClick={() => setOpen(true)} aria-label="Abrir menú" className="rounded-[8px] p-1.5 hover:bg-subtle">
            {open ? <X size={18} /> : <Menu size={18} />}
          </button>
          <Logo to="/app" />
        </header>
        <main className="min-w-0 flex-1">
          {sessionNeedsRelogin ? (
            <div className="border-b border-warning/20 bg-warning-bg px-5 py-3 text-[13px] text-warning">
              Tu cuenta ya tiene un rol nuevo, pero esta sesión se abrió antes.{' '}
              <button
                type="button"
                className="font-medium underline"
                onClick={() => {
                  logout()
                  navigate('/login')
                }}
              >
                Cerrar sesión y volver a entrar
              </button>{' '}
              para poder crear casos y usar el resto de funciones de abogado.
            </div>
          ) : null}
          <Outlet />
        </main>
      </div>
    </div>
  )
}

function NavGroup({ items, onNavigate }: { items: NavItem[]; onNavigate: () => void }) {
  return (
    <div className="space-y-0.5">
      {items.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          end={item.end}
          onClick={onNavigate}
          className={({ isActive }) =>
            cn(
              'flex items-center gap-2 rounded-[8px] px-2 py-2 text-[14px] transition-colors',
              isActive
                ? 'border-l-2 border-sky-400 bg-white/10 text-white'
                : 'border-l-2 border-transparent text-white/70 hover:bg-white/8 hover:text-white',
            )
          }
        >
          <item.icon size={16} />
          {item.label}
        </NavLink>
      ))}
    </div>
  )
}

export function AdminShell() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const items: NavItem[] = [
    { to: '/admin', label: 'Resumen', icon: LayoutDashboard, end: true },
    { to: '/admin/users', label: 'Usuarios', icon: User },
    { to: '/admin/verifications', label: 'Verificaciones', icon: Shield },
    { to: '/admin/plans', label: 'Planes', icon: CreditCard },
  ]

  return (
    <div className="flex min-h-screen bg-canvas">
      <aside className="hidden w-[240px] shrink-0 border-r border-border bg-surface md:flex md:flex-col">
        <div className="px-4 py-4">
          <p className="text-[12px] font-medium uppercase tracking-wide text-muted">Administración</p>
          <p className="text-[16px] font-semibold text-navy-900">JurisApp</p>
        </div>
        <nav className="flex-1 space-y-0.5 px-3">
          {items.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-2 rounded-[8px] px-2 py-2 text-[14px]',
                  isActive ? 'bg-subtle font-medium text-navy-900' : 'text-muted hover:bg-subtle hover:text-ink',
                )
              }
            >
              <item.icon size={16} />
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="space-y-2 border-t border-border p-4">
          <Link to="/app" className="block text-[13px] text-blue-600 hover:underline">
            Volver a JurisApp
          </Link>
          <button
            type="button"
            className="text-[13px] text-muted hover:text-ink"
            onClick={() => {
              logout()
              navigate('/login')
            }}
          >
            Cerrar sesión
          </button>
        </div>
      </aside>
      <main className="min-w-0 flex-1">
        <div className="border-b border-border bg-surface px-4 py-3 md:hidden">
          <p className="font-semibold">Administración</p>
          <div className="mt-2 flex flex-wrap gap-2 text-[13px]">
            {items.map((item) => (
              <Link key={item.to} to={item.to} className="text-blue-600">
                {item.label}
              </Link>
            ))}
          </div>
        </div>
        <Outlet />
      </main>
    </div>
  )
}

export function AppPage({ children, wide }: { children: ReactNode; wide?: boolean }) {
  return <div className={cn('px-5 py-6 lg:px-8', wide ? '' : 'mx-auto max-w-6xl')}>{children}</div>
}

export function QueryError({
  message,
  onRetry,
}: {
  message: string
  onRetry?: () => void
}) {
  return (
    <div className="rounded-[12px] border border-border bg-surface p-6">
      <p className="text-[14px] text-ink">{message}</p>
      <p className="mt-1 text-[13px] text-muted">Reintentá en unos segundos.</p>
      {onRetry ? (
        <Button className="mt-4" variant="secondary" onClick={onRetry}>
          Reintentar
        </Button>
      ) : null}
    </div>
  )
}
