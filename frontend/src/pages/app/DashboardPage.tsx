import { Link } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
  MessageSquare,
  FolderOpen,
  Sparkles,
  FileText,
  Plus,
} from 'lucide-react'
import { chatsApi, plansApi } from '@/lib/api'
import { useAuth } from '@/lib/auth/AuthContext'
import { DashboardWidget } from '@/components/domain/DashboardWidget'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { EmptyState } from '@/components/ui/EmptyState'
import { Skeleton } from '@/components/ui/Loading'
import { Alert } from '@/components/ui/Alert'
import { formatDate } from '@/lib/utils/format'

export function DashboardPage() {
  const { user } = useAuth()

  const { data: chats, isLoading: chatsLoading } = useQuery({
    queryKey: ['chats'],
    queryFn: chatsApi.list,
  })

  const { data: plan } = useQuery({
    queryKey: ['plans', 'current'],
    queryFn: plansApi.getCurrent,
  })

  const recentChats = chats?.slice(0, 5) ?? []
  const showLawyerBanner = user?.role === 'User'

  return (
    <div className="mx-auto max-w-7xl space-y-8">
      <div>
        <h2 className="font-heading text-2xl text-foreground md:text-3xl">
          Bienvenido, {user?.firstName}
        </h2>
        <p className="mt-1 text-muted-foreground">
          Tu espacio de trabajo legal está listo.
        </p>
      </div>

      {showLawyerBanner && (
        <Alert variant="info">
          Para usar expedientes y custom skills, completá tu{' '}
          <Link to="/app/settings" className="font-medium underline">
            perfil de abogado
          </Link>{' '}
          en configuración.
        </Alert>
      )}

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <DashboardWidget
          title="Consultas"
          value={chatsLoading ? '—' : (chats?.length ?? 0)}
          description="Conversaciones activas"
          icon={<MessageSquare className="h-5 w-5" aria-hidden="true" />}
        />
        <DashboardWidget
          title="Plan actual"
          value={plan?.planName ?? '—'}
          description={plan?.hasActiveSubscription ? 'Suscripción activa' : 'Plan gratuito'}
          icon={<Sparkles className="h-5 w-5" aria-hidden="true" />}
        />
        <DashboardWidget
          title="Documentos"
          value="En chats"
          description="Adjuntá archivos en consultas"
          icon={<FileText className="h-5 w-5" aria-hidden="true" />}
        />
        <DashboardWidget
          title="Expedientes"
          value="Organizá"
          description="Agrupá por caso"
          icon={<FolderOpen className="h-5 w-5" aria-hidden="true" />}
        />
      </div>

      <div className="flex flex-wrap gap-3">
        <Link to="/app/chats">
          <Button>
            <Plus className="h-4 w-4" aria-hidden="true" />
            Nueva consulta
          </Button>
        </Link>
        <Link to="/app/folders">
          <Button variant="secondary">Ver expedientes</Button>
        </Link>
      </div>

      <section>
        <h3 className="mb-4 text-lg font-medium text-foreground">Consultas recientes</h3>
        {chatsLoading ? (
          <div className="space-y-3">
            {Array.from({ length: 3 }).map((_, i) => (
              <Skeleton key={i} className="h-16" />
            ))}
          </div>
        ) : recentChats.length === 0 ? (
          <EmptyState
            icon={<MessageSquare className="h-6 w-6" aria-hidden="true" />}
            title="Sin consultas todavía"
            description="Creá tu primera consulta legal y empezá a trabajar con IA integrada."
            actionLabel="Crear consulta"
            onAction={() => (window.location.href = '/app/chats')}
          />
        ) : (
          <div className="space-y-3">
            {recentChats.map((chat) => (
              <Link key={chat.id} to={`/app/chats/${chat.id}`} className="block no-underline">
                <Card hover className="flex items-center justify-between">
                  <div>
                    <p className="font-medium text-foreground">{chat.title}</p>
                    <p className="text-xs text-muted-foreground">
                      {formatDate(chat.createdAt)}
                    </p>
                  </div>
                  <MessageSquare className="h-4 w-4 text-accent-secondary" aria-hidden="true" />
                </Card>
              </Link>
            ))}
          </div>
        )}
      </section>
    </div>
  )
}
