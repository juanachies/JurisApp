import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { chatsApi, foldersApi } from '@/api'
import { queryKeys } from '@/api/queryKeys'
import { errorMessage } from '@/api/client'
import { useAuth } from '@/app/AuthContext'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Modal } from '@/components/ui/Modal'
import { Select } from '@/components/ui/Select'
import { Alert } from '@/components/ui/Alert'
import { useToast } from '@/components/ui/Toast'

const schema = z.object({
  title: z.string().min(1, 'El título es obligatorio'),
  folderId: z.string().optional(),
})

export function CreateChatModal({
  open,
  onClose,
  defaultFolderId,
}: {
  open: boolean
  onClose: () => void
  defaultFolderId?: string
}) {
  const { canManageCases } = useAuth()
  const navigate = useNavigate()
  const toast = useToast()
  const queryClient = useQueryClient()
  const [error, setError] = useState<string | null>(null)
  const folders = useQuery({
    queryKey: queryKeys.folders,
    queryFn: foldersApi.list,
    enabled: open && canManageCases,
  })

  const form = useForm<{ title: string; folderId?: string }>({
    resolver: zodResolver(schema),
    values: { title: '', folderId: defaultFolderId ?? '' },
  })

  const mutation = useMutation({
    mutationFn: chatsApi.create,
    onSuccess: (chat) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.chats })
      toast('Conversación creada.')
      onClose()
      navigate(`/app/chats/${chat.id}`)
    },
    onError: (err) => setError(errorMessage(err, 'No pudimos crear el chat.')),
  })

  return (
    <Modal
      open={open}
      title="Nuevo chat"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button loading={mutation.isPending} onClick={form.handleSubmit((v) => mutation.mutate({
            title: v.title,
            folderId: v.folderId || undefined,
          }))}>
            Crear chat
          </Button>
        </>
      }
    >
      {error ? <Alert className="mb-3">{error}</Alert> : null}
      <div className="space-y-4">
        <Input label="Título" {...form.register('title')} error={form.formState.errors.title?.message} />
        {canManageCases ? (
          <Select label="Caso (opcional)" {...form.register('folderId')}>
            <option value="">Sin caso</option>
            {(folders.data ?? []).map((folder) => (
              <option key={folder.id} value={folder.id}>
                {folder.name}
              </option>
            ))}
          </Select>
        ) : null}
      </div>
    </Modal>
  )
}
