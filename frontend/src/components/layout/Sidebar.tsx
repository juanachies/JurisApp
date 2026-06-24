import {
  MessageSquare,
  FolderOpen,
  Sparkles,
  LayoutDashboard,
  Wand2,
  CreditCard,
  Settings,
  Users,
  Menu,
  X,
  Scale,
} from 'lucide-react'
import { NavLink } from 'react-router-dom'
import { useAuth } from '@/lib/auth/AuthContext'
import { cn } from '@/lib/utils/cn'
import type { UserRole } from '@/lib/api'

interface NavItem {
  to: string
  label: string
  icon: typeof MessageSquare
  roles?: UserRole[]
}

const navItems: NavItem[] = [
  { to: '/app/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/app/chats', label: 'Consultas', icon: MessageSquare },
  { to: '/app/folders', label: 'Expedientes', icon: FolderOpen, roles: ['Lawyer', 'Admin'] },
  { to: '/app/skills', label: 'Skills', icon: Wand2, roles: ['Lawyer', 'Admin'] },
  { to: '/app/plans', label: 'Planes', icon: CreditCard },
  { to: '/app/settings', label: 'Configuración', icon: Settings },
  { to: '/app/admin/users', label: 'Usuarios', icon: Users, roles: ['Admin'] },
]

interface SidebarProps {
  isOpen?: boolean
  onClose?: () => void
}

export function Sidebar({ isOpen, onClose }: SidebarProps) {
  const { user } = useAuth()

  const visibleItems = navItems.filter(
    (item) => !item.roles || (user && item.roles.includes(user.role)),
  )

  return (
    <>
      {isOpen && (
        <div
          className="fixed inset-0 z-40 bg-foreground/40 md:hidden"
          onClick={onClose}
          aria-hidden="true"
        />
      )}
      <aside
        className={cn(
          'fixed inset-y-0 left-0 z-50 flex w-64 flex-col bg-sidebar text-sidebar-foreground transition-transform duration-300 md:static md:translate-x-0',
          isOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0',
        )}
        aria-label="Navegación principal"
      >
        <div className="flex h-16 items-center justify-between border-b border-white/10 px-5">
          <NavLink to="/app/dashboard" className="flex items-center gap-2 no-underline">
            <Scale className="h-6 w-6 text-accent" aria-hidden="true" />
            <span className="font-heading text-lg text-white">JurisApp</span>
          </NavLink>
          <button
            type="button"
            className="rounded-[10px] p-1 text-white/70 hover:text-white md:hidden focus-ring"
            onClick={onClose}
            aria-label="Cerrar menú"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <nav className="flex-1 overflow-y-auto px-3 py-4">
          <ul className="flex flex-col gap-1">
            {visibleItems.map((item) => (
              <li key={item.to}>
                <NavLink
                  to={item.to}
                  onClick={onClose}
                  className={({ isActive }) =>
                    cn(
                      'flex items-center gap-3 rounded-[10px] px-3 py-2.5 text-sm font-medium transition-colors no-underline focus-ring',
                      isActive
                        ? 'bg-accent-secondary/20 text-white border-l-[3px] border-accent pl-[9px]'
                        : 'text-white/70 hover:bg-white/8 hover:text-white',
                    )
                  }
                >
                  <item.icon className="h-4 w-4 shrink-0" aria-hidden="true" />
                  {item.label}
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>

        <div className="border-t border-white/10 p-4">
          <div className="flex items-center gap-2 text-xs text-white/50">
            <Sparkles className="h-3.5 w-3.5 text-ai" aria-hidden="true" />
            <span>IA integrada en flujos legales</span>
          </div>
        </div>
      </aside>
    </>
  )
}

export function MobileNavButton({ onClick }: { onClick: () => void }) {
  return (
    <button
      type="button"
      className="rounded-[10px] p-2 text-foreground hover:bg-muted md:hidden focus-ring"
      onClick={onClick}
      aria-label="Abrir menú"
    >
      <Menu className="h-5 w-5" />
    </button>
  )
}
