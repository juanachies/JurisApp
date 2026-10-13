import { useEffect, useRef, useState, type ReactNode } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
  ChevronDown,
  CreditCard,
  Folder,
  Home,
  LayoutDashboard,
  Menu,
  MessageSquare,
  PanelLeftClose,
  Shield,
  User,
  Wand2,
  X,
  LogOut,
} from 'lucide-react'
import { chatsApi, foldersApi, plansApi } from '@/api'
import { queryKeys } from '@/api/queryKeys'
import { useAuth } from '@/app/AuthContext'
import { Logo } from '@/components/brand/Logo'
import { Button } from '@/components/ui/Button'
import { cn } from '@/utils/cn'
import { fullName, planTypeLabel, roleLabel } from '@/utils/format'

type NavItem = { to: string; label: string; icon: typeof Home; end?: boolean }

export function AppShell() {
  const { user, canManageCases, isAdmin, logout, sessionNeedsRelogin } = useAuth()
  const [open, setOpen] = useState(false)
  const [collapsed, setCollapsed] = useState(false)
  const [userMenuOpen, setUserMenuOpen] = useState(false)
  const [chatExpanded, setChatExpanded] = useState(false)
  const [folderExpanded, setFolderExpanded] = useState(false)
  const userMenuRef = useRef<HTMLDivElement | null>(null)
  const navigate = useNavigate()

  useEffect(() => {
    if (!userMenuOpen) return

    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as Node
      if (userMenuRef.current && !userMenuRef.current.contains(target)) {
        setUserMenuOpen(false)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [userMenuOpen])

  const chatsQuery = useQuery({ queryKey: queryKeys.chats, queryFn: chatsApi.list })
  const foldersQuery = useQuery({
    queryKey: queryKeys.folders,
    queryFn: foldersApi.list,
    enabled: canManageCases,
  })
  const currentPlanQuery = useQuery({
    queryKey: queryKeys.currentPlan,
    queryFn: plansApi.current,
    retry: false,
  })

  const allChats = [...(chatsQuery.data ?? [])].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
  const visibleChats = allChats.slice(0, chatExpanded ? allChats.length : 5)
  const allFolders = (foldersQuery.data ?? []).slice().sort((a, b) => a.name.localeCompare(b.name))
  const visibleFolders = allFolders.slice(0, folderExpanded ? allFolders.length : 5)

  const sidebar = (
    <div className="flex h-full flex-col rounded-none bg-navy-950 text-white transition-all duration-300">
      <div
        className={cn(
          'flex h-16 shrink-0 items-center border-b border-white/10 px-4 transition-all duration-200',
          collapsed ? 'justify-center cursor-pointer hover:bg-white/5' : 'justify-between',
        )}
        onClick={() => {
          if (collapsed) setCollapsed(false)
        }}
        title={collapsed ? 'Expandir menú' : undefined}
      >
        <div className={cn('flex items-center', collapsed && 'pointer-events-none w-7 overflow-hidden')}>
          <Logo to="/app" light />
        </div>

        {!collapsed && (
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation()
              setCollapsed(true)
            }}
            className="flex size-8 items-center justify-center rounded-none bg-transparent text-white/70 transition-colors hover:bg-white/5 hover:text-white"
            aria-label="Colapsar sidebar"
          >
            <PanelLeftClose size={16} strokeWidth={1.8} />
          </button>
        )}
      </div>

      <nav className="flex-1 space-y-3 overflow-y-auto px-3 py-4 custom-scrollbar">
        <div className="space-y-1">
          <NavLink
            to="/app"
            end
            onClick={() => setOpen(false)}
            className={({ isActive }) => cn(
              'flex items-center gap-3 rounded-xl px-2.5 py-2.5 text-[14px] transition-all duration-200',
              isActive ? 'bg-white/10 font-medium text-white' : 'text-white/70 hover:bg-white/5 hover:text-white',
              collapsed && 'justify-center px-2',
            )}
            title="Inicio"
          >
            <Home size={17} strokeWidth={2} />
            {!collapsed && <span>Inicio</span>}
          </NavLink>
        </div>

        {canManageCases && (
          <div className="space-y-1">
            <NavLink
              to="/app/skills"
              onClick={() => setOpen(false)}
              className={({ isActive }) => cn(
                'flex items-center gap-3 rounded-xl px-2.5 py-2.5 text-[14px] transition-all duration-200',
                isActive ? 'bg-white/10 font-medium text-white' : 'text-white/70 hover:bg-white/5 hover:text-white',
                collapsed && 'justify-center px-2',
              )}
              title="Skills IA"
            >
              <Wand2 size={17} strokeWidth={2} />
              {!collapsed && <span>Skills IA</span>}
            </NavLink>
          </div>
        )}

        <div className="space-y-1.5">
          <NavLink
            to="/app/chats"
            onClick={() => setOpen(false)}
            className={({ isActive }) => cn(
              'flex items-center gap-3 rounded-xl px-2.5 py-2.5 text-[14px] transition-all duration-200',
              isActive ? 'bg-white/10 font-medium text-white' : 'text-white/70 hover:bg-white/5 hover:text-white',
              collapsed && 'justify-center px-2',
            )}
            title="Chats"
          >
            <MessageSquare size={17} strokeWidth={2} />
            {!collapsed && <span>Chats</span>}
          </NavLink>

          {!collapsed && allChats.length > 0 && (
            <div className="ml-5 space-y-1 border-l border-white/10 pl-3">
              {visibleChats.map((chat) => (
                <Link
                  key={chat.id}
                  to={`/app/chats/${chat.id}`}
                  onClick={() => setOpen(false)}
                  className="flex items-center gap-2 rounded-lg px-2.5 py-1.5 text-[13px] text-white/65 transition-colors hover:bg-white/5 hover:text-white"
                >
                  <span className="truncate">{chat.title}</span>
                </Link>
              ))}

              {allChats.length > 5 && (
                <button
                  type="button"
                  onClick={() => setChatExpanded((v) => !v)}
                  className="w-full rounded-lg px-2.5 py-1.5 text-left text-[12px] font-medium text-white/45 transition-colors hover:bg-white/5 hover:text-white"
                >
                  {chatExpanded ? 'Mostrar menos...' : 'Mostrar más...'}
                </button>
              )}
            </div>
          )}
        </div>

        {canManageCases && (
          <div className="space-y-1.5">
            <NavLink
              to="/app/cases"
              onClick={() => setOpen(false)}
              className={({ isActive }) => cn(
                'flex items-center gap-3 rounded-xl px-2.5 py-2.5 text-[14px] transition-all duration-200',
                isActive ? 'bg-white/10 font-medium text-white' : 'text-white/70 hover:bg-white/5 hover:text-white',
                collapsed && 'justify-center px-2',
              )}
              title="Carpetas"
            >
              <Folder size={17} strokeWidth={2} />
              {!collapsed && <span>Carpetas</span>}
            </NavLink>

            {!collapsed && allFolders.length > 0 && (
              <div className="ml-5 space-y-1 border-l border-white/10 pl-3">
                {visibleFolders.map((folder) => (
                  <Link
                    key={folder.id}
                    to={`/app/cases/${folder.id}`}
                    onClick={() => setOpen(false)}
                    className="flex items-center gap-2 rounded-lg px-2.5 py-1.5 text-[13px] text-white/65 transition-colors hover:bg-white/5 hover:text-white"
                  >
                    <span className="truncate">{folder.name}</span>
                  </Link>
                ))}

                {allFolders.length > 5 && (
                  <button
                    type="button"
                    onClick={() => setFolderExpanded((v) => !v)}
                    className="w-full rounded-lg px-2.5 py-1.5 text-left text-[12px] font-medium text-white/45 transition-colors hover:bg-white/5 hover:text-white"
                  >
                    {folderExpanded ? 'Mostrar menos...' : 'Mostrar más...'}
                  </button>
                )}
              </div>
            )}
          </div>
        )}

        {isAdmin && (
          <div className="space-y-1">
            <NavLink
              to="/admin"
              onClick={() => setOpen(false)}
              className={({ isActive }) => cn(
                'flex items-center gap-3 rounded-xl px-2.5 py-2.5 text-[14px] transition-all duration-200',
                isActive ? 'bg-white/10 font-medium text-white' : 'text-white/70 hover:bg-white/5 hover:text-white',
                collapsed && 'justify-center px-2',
              )}
              title="Administración"
            >
              <Shield size={17} strokeWidth={2} />
              {!collapsed && <span>Administración</span>}
            </NavLink>
          </div>
        )}
      </nav>

      <div className="border-t border-white/10 p-3">
        <div className="relative" ref={userMenuRef}>
          <button
            type="button"
            onClick={() => setUserMenuOpen((v) => !v)}
            className={cn(
              'flex w-full items-center gap-3 rounded-none bg-transparent p-1.5 text-left transition-all duration-200 hover:bg-white/[0.04]',
              collapsed && 'justify-center bg-transparent hover:bg-white/5',
            )}
          >
            <div className="flex size-8 shrink-0 items-center justify-center rounded-full bg-ember text-[10px] font-bold text-white shadow-sm shadow-black/20">
              {user ? `${user.firstName?.[0] ?? ''}${user.lastName?.[0] ?? ''}`.toUpperCase() : 'JC'}
            </div>

            {!collapsed && (
              <div className="flex min-w-0 flex-1 items-center justify-between gap-2">
                <div className="min-w-0">
                  <p className="truncate text-[13px] font-medium text-white">
                    {user ? fullName(user.firstName, user.lastName) : 'Juana Chies'}
                  </p>
                  <div className="mt-0.5 flex items-center gap-1.5">
                    <p className="truncate text-[11px] text-white/60">{user ? roleLabel(user.role) : 'Abogado'}</p>
                    <span className="rounded-full border border-pine/40 bg-pine/25 px-1.5 py-[1px] text-[9px] font-medium uppercase tracking-[0.08em] text-white">
                      {planTypeLabel(currentPlanQuery.data?.planType ?? 'Free')}
                    </span>
                  </div>
                </div>
                <ChevronDown size={14} className="shrink-0 text-white/50" />
              </div>
            )}
          </button>

          {userMenuOpen && (
            <div className={cn('absolute bottom-full z-20 mb-2 w-full min-w-[200px] rounded-[16px] border border-white/10 bg-navy-900 p-1.5 shadow-2xl shadow-black/40', collapsed ? 'left-0' : 'left-0 right-0')}>
              <Link to="/app/profile" className="flex items-center gap-2.5 rounded-lg px-2.5 py-2 text-[13px] text-white/90 transition-colors hover:bg-white/8 hover:text-white">
                <User size={15} className="text-white/60" /> Perfil
              </Link>
              <Link to="/app/subscription" className="flex items-center gap-2.5 rounded-lg px-2.5 py-2 text-[13px] text-white/90 transition-colors hover:bg-white/8 hover:text-white">
                <CreditCard size={15} className="text-white/60" /> Plan
              </Link>
              <div className="my-1 h-px bg-white/10" />
              <button
                type="button"
                className="flex w-full items-center gap-2.5 rounded-lg px-2.5 py-2 text-left text-[13px] text-red-400 transition-colors hover:bg-red-500/10 hover:text-red-300"
                onClick={() => {
                  logout()
                  navigate('/login')
                }}
              >
                <LogOut size={15} /> Cerrar sesión
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  )

  return (
    <div className="flex min-h-screen bg-canvas">
      <aside className={cn('sticky top-0 hidden h-screen shrink-0 md:block transition-all duration-300 rounded-none', collapsed ? 'w-[76px]' : 'w-[260px]')}>
        {sidebar}
      </aside>
      
      {open && (
        <div className="fixed inset-0 z-50 md:hidden">
          <button type="button" className="absolute inset-0 bg-navy-950/20 backdrop-blur-sm" aria-label="Cerrar menú" onClick={() => setOpen(false)} />
          <aside className="relative h-full w-[260px] rounded-none">{sidebar}</aside>
        </div>
      )}
      
      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex items-center gap-3 border-b border-border bg-surface px-4 py-3 md:hidden">
          <button type="button" onClick={() => setOpen(true)} aria-label="Abrir menú" className="rounded-[8px] p-1.5 hover:bg-subtle">
            {open ? <X size={18} /> : <Menu size={18} />}
          </button>
          <Logo to="/app" />
        </header>
        <main className="min-w-0 flex-1">
          {sessionNeedsRelogin && (
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
          )}
          <Outlet />
        </main>
      </div>
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
      <aside className="hidden w-[240px] shrink-0 bg-navy-950 text-white md:flex md:flex-col">
        <div className="border-b border-white/10 px-4 py-4">
          <Logo light to="/admin" />
          <p className="mt-3 text-[11px] font-medium uppercase tracking-[0.16em] text-white/45">Administración</p>
        </div>
        <nav className="flex-1 space-y-1 px-3 py-4">
          {items.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) =>
                cn(
                  'flex items-center gap-2.5 rounded-xl px-2.5 py-2.5 text-[14px] transition-colors',
                  isActive
                    ? 'bg-white/10 font-medium text-white'
                    : 'text-white/70 hover:bg-white/5 hover:text-white',
                )
              }
            >
              <item.icon size={16} />
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="space-y-2 border-t border-white/10 p-4">
          <Link to="/app" className="block text-[13px] text-sky-400 hover:text-white">
            Volver a JurisApp
          </Link>
          <button
            type="button"
            className="text-[13px] text-white/55 hover:text-white"
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
        <div className="border-b border-white/10 bg-navy-950 px-4 py-3 text-white md:hidden">
          <p className="font-semibold">Administración</p>
          <div className="mt-2 flex flex-wrap gap-3 text-[13px]">
            {items.map((item) => (
              <Link key={item.to} to={item.to} className="text-sky-400">
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
  return <div className={cn('px-5 py-8 lg:px-10', wide ? '' : 'mx-auto max-w-6xl')}>{children}</div>
}

export function QueryError({
  message,
  onRetry,
}: {
  message: string
  onRetry?: () => void
}) {
  return (
    <div className="rounded-[18px] border border-border bg-surface p-6 shadow-card">
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