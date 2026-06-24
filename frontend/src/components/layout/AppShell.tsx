import { useState } from 'react'
import { Outlet, useLocation } from 'react-router-dom'
import { Sidebar } from './Sidebar'
import { Topbar } from './Topbar'

const pageTitles: Record<string, string> = {
  '/app/dashboard': 'Dashboard',
  '/app/chats': 'Consultas legales',
  '/app/folders': 'Expedientes',
  '/app/skills': 'Custom Skills',
  '/app/plans': 'Planes y suscripción',
  '/app/settings': 'Configuración',
  '/app/admin/users': 'Administración de usuarios',
}

export function AppShell() {
  const [sidebarOpen, setSidebarOpen] = useState(false)
  const location = useLocation()

  const title =
    pageTitles[location.pathname] ??
    (location.pathname.startsWith('/app/chats/') ? 'Consulta legal' : '')

  return (
    <div className="flex min-h-screen bg-background">
      <Sidebar isOpen={sidebarOpen} onClose={() => setSidebarOpen(false)} />
      <div className="flex flex-1 flex-col min-w-0">
        <Topbar onMenuClick={() => setSidebarOpen(true)} title={title} />
        <main className="flex-1 overflow-auto p-4 md:p-6 lg:p-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
