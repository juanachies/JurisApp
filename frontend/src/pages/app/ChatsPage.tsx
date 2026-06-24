import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { MessageSquare, Plus } from 'lucide-react'
import { chatsApi } from '@/lib/api'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { EmptyState } from '@/components/ui/EmptyState'
import { Input } from '@/components/ui/Input'
import { Modal } from '@/components/ui/Modal'
import { Skeleton } from '@/components/ui/Loading'
import { formatDate } from '@/lib/utils/format'

export function ChatsPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [showCreate, setShowCreate] = useState(false)
  const [title, setTitle] = useState('')

  const { data: chats, isLoading } = useQuery({
    queryKey: ['chats'],
    queryFn: chatsApi.list,
  })

  const createMutation = useMutation({
    mutationFn: (t: string) => chatsApi.create({ title: t }),
    onSuccess: (chat) => {
      queryClient.invalidateQueries({ queryKey: ['chats'] })
      setShowCreate(false)
      setTitle('')
      navigate(`/app/chats/${chat.id}`)
    },
  })

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="font-heading text-2xl text-foreground">Consultas legales</h2>
          <p className="text-sm text-muted-foreground">
            Chats con IA para consultas jurídicas
          </p>
        </div>
        <Button onClick={() => setShowCreate(true)}>
          <Plus className="h-4 w-4" aria-hidden="true" />
          Nueva consulta
        </Button>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-20" />
          ))}
        </div>
      ) : !chats?.length ? (
        <EmptyState
          icon={<MessageSquare className="h-6 w-6" aria-hidden="true" />}
          title="Empezá tu primera consulta"
          description="Creá un chat, adjuntá documentos, aplicá skills y obtené respuestas de IA con contexto legal."
          actionLabel="Crear consulta"
          onAction={() => setShowCreate(true)}
        />
      ) : (
        <div className="space-y-3">
          {chats.map((chat) => (
            <Link key={chat.id} to={`/app/chats/${chat.id}`} className="block no-underline">
              <Card hover>
                <div className="flex items-center justify-between">
                  <div>
                    <h3 className="font-medium text-foreground">{chat.title}</h3>
                    <p className="mt-0.5 text-xs text-muted-foreground">
                      {formatDate(chat.createdAt)}
                    </p>
                  </div>
                  <MessageSquare className="h-5 w-5 text-accent-secondary" aria-hidden="true" />
                </div>
              </Card>
            </Link>
          ))}
        </div>
      )}

      <Modal
        isOpen={showCreate}
        onClose={() => setShowCreate(false)}
        title="Nueva consulta"
      >
        <form
          onSubmit={(e) => {
            e.preventDefault()
            if (title.trim()) createMutation.mutate(title.trim())
          }}
          className="space-y-4"
        >
          <Input
            label="Título"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="Ej: Consulta laboral"
            required
          />
          <div className="flex justify-end gap-3">
            <Button variant="secondary" type="button" onClick={() => setShowCreate(false)}>
              Cancelar
            </Button>
            <Button type="submit" isLoading={createMutation.isPending}>
              Crear
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  )
}
