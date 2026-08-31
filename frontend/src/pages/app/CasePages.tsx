import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { chatsApi, documentsApi, foldersApi } from '@/api'
import { queryKeys } from '@/api/queryKeys'
import { errorMessage } from '@/api/client'
import { AppPage, QueryError } from '@/components/layout/AppShell'
import { CreateChatModal } from '@/components/chat/CreateChatModal'
import { UploadDocumentModal } from '@/components/documents/UploadDocumentModal'
import { Alert } from '@/components/ui/Alert'
import { Button } from '@/components/ui/Button'
import { ConfirmDialog, Modal } from '@/components/ui/Modal'
import { EmptyState } from '@/components/ui/EmptyState'
import { Input } from '@/components/ui/Input'
import { PageHeader } from '@/components/ui/PageHeader'
import { Table, TBody, Td, Th, THead, Tr } from '@/components/ui/Table'
import { TableSkeleton } from '@/components/ui/Loading'
import { Textarea } from '@/components/ui/Textarea'
import { useToast } from '@/components/ui/Toast'
import { fileExtension } from '@/utils/format'
import { cn } from '@/utils/cn'

const folderSchema = z.object({
  name: z.string().min(1, 'El nombre es obligatorio'),
  legalContext: z.string().optional(),
})

export function CasesPage() {
  const navigate = useNavigate()
  const toast = useToast()
  const queryClient = useQueryClient()
  const [q, setQ] = useState('')
  const [createOpen, setCreateOpen] = useState(false)
  const [deleteId, setDeleteId] = useState<string | null>(null)

  const foldersQuery = useQuery({ queryKey: queryKeys.folders, queryFn: foldersApi.list })
  const chatsQuery = useQuery({ queryKey: queryKeys.chats, queryFn: chatsApi.list })

  const folders = (foldersQuery.data ?? []).filter((f) => {
    const hay = `${f.name} ${f.legalContext ?? ''}`.toLowerCase()
    return hay.includes(q.toLowerCase())
  })

  const remove = useMutation({
    mutationFn: (id: string) => foldersApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.folders })
      toast('Caso eliminado.')
      setDeleteId(null)
    },
  })

  return (
    <AppPage>
      <PageHeader
        title="Casos"
        description="Organizá documentos y conversaciones por asunto."
        actions={<Button onClick={() => setCreateOpen(true)}>Nuevo caso</Button>}
      />
      <input
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Buscar casos"
        className="mb-4 h-10 w-full max-w-md rounded-[8px] border border-border-strong bg-surface px-3 text-[14px]"
      />
      {foldersQuery.isLoading ? <TableSkeleton /> : null}
      {foldersQuery.isError ? (
        <QueryError message="No pudimos cargar tus casos." onRetry={() => foldersQuery.refetch()} />
      ) : null}
      {!foldersQuery.isLoading && folders.length === 0 ? (
        <EmptyState
          title="Todavía no creaste ningún caso."
          description="Agrupá documentos y conversaciones relacionadas en un mismo espacio."
          action={{ label: 'Nuevo caso', onClick: () => setCreateOpen(true) }}
        />
      ) : !foldersQuery.isLoading ? (
        <Table>
          <THead>
            <tr>
              <Th>Caso</Th>
              <Th>Descripción</Th>
              <Th>Chats</Th>
              <Th></Th>
            </tr>
          </THead>
          <TBody>
            {folders.map((folder) => {
              const chatCount = (chatsQuery.data ?? []).filter((c) => c.folderId === folder.id).length
              return (
                <Tr key={folder.id} onClick={() => navigate(`/app/cases/${folder.id}`)}>
                  <Td className="font-medium">{folder.name}</Td>
                  <Td className="max-w-sm truncate text-muted">{folder.legalContext || '—'}</Td>
                  <Td>{chatCount}</Td>
                  <Td>
                    <button
                      type="button"
                      className="text-[13px] text-danger"
                      onClick={(e) => {
                        e.stopPropagation()
                        setDeleteId(folder.id)
                      }}
                    >
                      Eliminar
                    </button>
                  </Td>
                </Tr>
              )
            })}
          </TBody>
        </Table>
      ) : null}

      <FolderFormModal
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onSaved={(id) => navigate(`/app/cases/${id}`)}
      />
      <ConfirmDialog
        open={Boolean(deleteId)}
        title="Eliminar caso"
        description="Se eliminará el caso según el comportamiento definido por JurisApp."
        confirmLabel="Eliminar"
        danger
        loading={remove.isPending}
        onConfirm={() => deleteId && remove.mutate(deleteId)}
        onClose={() => setDeleteId(null)}
      />
    </AppPage>
  )
}

export function CaseDetailPage() {
  const { caseId } = useParams<{ caseId: string }>()
  const toast = useToast()
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<'resumen' | 'documentos' | 'chats'>('resumen')
  const [upload, setUpload] = useState(false)
  const [newChat, setNewChat] = useState(false)
  const [edit, setEdit] = useState(false)
  const [deleteOpen, setDeleteOpen] = useState(false)
  const navigate = useNavigate()

  const foldersQuery = useQuery({ queryKey: queryKeys.folders, queryFn: foldersApi.list })
  const folder = foldersQuery.data?.find((f) => f.id === caseId)
  const docsQuery = useQuery({
    queryKey: queryKeys.folderDocuments(caseId ?? ''),
    queryFn: () => documentsApi.listByFolder(caseId!),
    enabled: Boolean(caseId),
  })
  const chatsQuery = useQuery({ queryKey: queryKeys.chats, queryFn: chatsApi.list })
  const caseChats = (chatsQuery.data ?? []).filter((c) => c.folderId === caseId)

  const remove = useMutation({
    mutationFn: () => foldersApi.delete(caseId!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.folders })
      toast('Caso eliminado.')
      navigate('/app/cases')
    },
  })

  if (foldersQuery.isLoading) {
    return (
      <AppPage>
        <TableSkeleton rows={3} />
      </AppPage>
    )
  }
  if (!folder) {
    return (
      <AppPage>
        <QueryError message="No encontramos este caso." />
      </AppPage>
    )
  }

  return (
    <AppPage>
      <Link to="/app/cases" className="text-[13px] text-blue-600 hover:underline">
        ← Casos
      </Link>
      <div className="mt-3 mb-6 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-[28px] font-semibold">{folder.name}</h1>
          {folder.legalContext ? <p className="mt-1 max-w-2xl text-[14px] text-muted">{folder.legalContext}</p> : null}
        </div>
        <div className="flex flex-wrap gap-2">
          <Button variant="secondary" onClick={() => setEdit(true)}>
            Editar
          </Button>
          <Button variant="secondary" onClick={() => setNewChat(true)}>
            Chat nuevo
          </Button>
          <Button onClick={() => setUpload(true)}>Subir documento</Button>
        </div>
      </div>

      <div className="mb-5 flex gap-1 border-b border-border">
        {(['resumen', 'documentos', 'chats'] as const).map((id) => (
          <button
            key={id}
            type="button"
            className={cn(
              'border-b-2 px-3 py-2 text-[14px] capitalize',
              tab === id ? 'border-navy-900 font-medium text-ink' : 'border-transparent text-muted',
            )}
            onClick={() => setTab(id)}
          >
            {id}
          </button>
        ))}
      </div>

      {tab === 'resumen' ? (
        <div className="grid gap-6 lg:grid-cols-2">
          <div>
            <h2 className="mb-2 text-[16px] font-semibold">Documentos</h2>
            {(docsQuery.data ?? []).length === 0 ? (
              <p className="text-[14px] text-muted">Todavía no hay documentos en este caso.</p>
            ) : (
              <ul className="space-y-2 text-[14px]">
                {(docsQuery.data ?? []).map((doc) => (
                  <li key={doc.id}>
                    <Link to={`/app/documents/${doc.id}`} className="hover:underline">
                      {doc.title}
                    </Link>
                  </li>
                ))}
              </ul>
            )}
          </div>
          <div>
            <h2 className="mb-2 text-[16px] font-semibold">Chats</h2>
            {caseChats.length === 0 ? (
              <p className="text-[14px] text-muted">Todavía no hay conversaciones en este caso.</p>
            ) : (
              <ul className="space-y-2 text-[14px]">
                {caseChats.map((chat) => (
                  <li key={chat.id}>
                    <Link to={`/app/chats/${chat.id}`} className="hover:underline">
                      {chat.title}
                    </Link>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      ) : null}

      {tab === 'documentos' ? (
        docsQuery.isError ? (
          <QueryError message="No pudimos cargar los documentos." onRetry={() => docsQuery.refetch()} />
        ) : (docsQuery.data ?? []).length === 0 ? (
          <EmptyState
            title="No hay documentos todavía."
            description="Subí un archivo para asociarlo a este caso."
            action={{ label: 'Subir documento', onClick: () => setUpload(true) }}
          />
        ) : (
          <div className="divide-y divide-border rounded-[12px] border border-border bg-surface">
            {(docsQuery.data ?? []).map((doc) => (
              <div key={doc.id} className="flex items-center justify-between px-4 py-3">
                <div>
                  <p className="text-[14px] font-medium">{doc.title}</p>
                  <p className="text-[12px] text-muted">{fileExtension(doc.title) || 'Archivo'}</p>
                </div>
                <div className="flex gap-3 text-[13px]">
                  <Link to={`/app/documents/${doc.id}`} className="text-blue-600 hover:underline">
                    Analizar
                  </Link>
                </div>
              </div>
            ))}
          </div>
        )
      ) : null}

      {tab === 'chats' ? (
        caseChats.length === 0 ? (
          <EmptyState
            title="Todavía no hay conversaciones."
            description="Creá un chat asociado a este caso."
            action={{ label: 'Chat nuevo', onClick: () => setNewChat(true) }}
          />
        ) : (
          <div className="divide-y divide-border rounded-[12px] border border-border bg-surface">
            {caseChats.map((chat) => (
              <Link key={chat.id} to={`/app/chats/${chat.id}`} className="flex items-center justify-between px-4 py-3 hover:bg-subtle">
                <span className="text-[14px] font-medium">{chat.title}</span>
                <span className="text-[13px] text-blue-600">Continuar conversación →</span>
              </Link>
            ))}
          </div>
        )
      ) : null}

      <button type="button" className="mt-8 text-[13px] text-danger" onClick={() => setDeleteOpen(true)}>
        Eliminar caso
      </button>

      <UploadDocumentModal open={upload} onClose={() => setUpload(false)} lockFolderId={caseId} />
      <CreateChatModal open={newChat} onClose={() => setNewChat(false)} defaultFolderId={caseId} />
      <FolderFormModal
        open={edit}
        folderId={folder.id}
        initial={{ name: folder.name, legalContext: folder.legalContext ?? '' }}
        onClose={() => setEdit(false)}
      />
      <ConfirmDialog
        open={deleteOpen}
        title="Eliminar caso"
        description="Se eliminará el caso según el comportamiento definido por JurisApp."
        confirmLabel="Eliminar"
        danger
        loading={remove.isPending}
        onConfirm={() => remove.mutate()}
        onClose={() => setDeleteOpen(false)}
      />
    </AppPage>
  )
}

function FolderFormModal({
  open,
  onClose,
  folderId,
  initial,
  onSaved,
}: {
  open: boolean
  onClose: () => void
  folderId?: string
  initial?: { name: string; legalContext: string }
  onSaved?: (id: string) => void
}) {
  const queryClient = useQueryClient()
  const toast = useToast()
  const [error, setError] = useState<string | null>(null)
  const form = useForm<{ name: string; legalContext?: string }>({
    resolver: zodResolver(folderSchema),
    values: initial ?? { name: '', legalContext: '' },
  })

  const mutation = useMutation({
    mutationFn: (values: { name: string; legalContext?: string }) =>
      folderId ? foldersApi.update(folderId, values) : foldersApi.create(values),
    onSuccess: (folder) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.folders })
      toast(folderId ? 'Caso actualizado.' : 'Caso creado.')
      onSaved?.(folder.id)
      onClose()
    },
    onError: (err) => setError(errorMessage(err)),
  })

  return (
    <Modal
      open={open}
      title={folderId ? 'Editar caso' : 'Nuevo caso'}
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button loading={mutation.isPending} onClick={form.handleSubmit((v) => mutation.mutate(v))}>
            {folderId ? 'Guardar' : 'Crear caso'}
          </Button>
        </>
      }
    >
      {error ? <Alert className="mb-3">{error}</Alert> : null}
      <div className="space-y-4">
        <Input label="Nombre *" {...form.register('name')} error={form.formState.errors.name?.message} />
        <Textarea
          label="Descripción"
          hint="Contexto legal del asunto. No pedimos fuero, expediente ni plazos porque el sistema no los guarda."
          {...form.register('legalContext')}
        />
      </div>
    </Modal>
  )
}
