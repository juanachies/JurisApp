import { Link } from 'react-router-dom'
import { LogOut, User } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { useAuth } from '@/lib/auth/AuthContext'
import { plansApi } from '@/lib/api'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { MobileNavButton } from './Sidebar'

interface TopbarProps {
  onMenuClick: () => void
  title?: string
}

export function Topbar({ onMenuClick, title }: TopbarProps) {
  const { user, logout } = useAuth()

  const { data: currentPlan } = useQuery({
    queryKey: ['plans', 'current'],
    queryFn: plansApi.getCurrent,
    enabled: !!user,
  })

  return (
    <header className="sticky top-0 z-30 flex h-16 items-center justify-between border-b border-border bg-background-alt/95 px-4 backdrop-blur-sm md:px-6">
      <div className="flex items-center gap-3">
        <MobileNavButton onClick={onMenuClick} />
        {title && (
          <h1 className="text-lg font-medium text-foreground md:text-xl">{title}</h1>
        )}
      </div>

      <div className="flex items-center gap-3">
        {currentPlan && (
          <Link to="/app/plans" className="no-underline">
            <Badge variant={currentPlan.planType === 'Free' ? 'default' : 'premium'}>
              {currentPlan.planName}
            </Badge>
          </Link>
        )}

        <div className="hidden items-center gap-2 text-sm text-muted-foreground sm:flex">
          <User className="h-4 w-4" aria-hidden="true" />
          <span>
            {user?.firstName} {user?.lastName}
          </span>
        </div>

        <Button variant="ghost" size="icon" onClick={logout} aria-label="Cerrar sesión">
          <LogOut className="h-4 w-4" />
        </Button>
      </div>
    </header>
  )
}
