import { useEffect, useMemo, useRef, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, MoreHorizontal, Paperclip, PanelRight } from 'lucide-react'
import {
  chatsApi,
  documentsApi,
  foldersApi,
  skillsApi,
  tasksApi,
} from '@/api'
import { ApiError, errorMessage } from '@/api/client'
import { queryKeys } from '@/api/queryKeys'
import { useAuth } from '@/app/AuthContext'
import { CreateChatModal } from '@/components/chat/CreateChatModal'
import { UploadDocumentModal } from '@/components/documents/UploadDocumentModal'
import { TaskPlanCard } from '@/components/tasks/TaskPlanCard'
import { Alert } from '@/components/ui/Alert'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { ConfirmDialog } from '@/components/ui/Modal'
import { EmptyState } from '@/components/ui/EmptyState'
import { MarkdownBody, AiDisclaimer } from '@/components/ui/MarkdownBody'
import { Skeleton } from '@/components/ui/Loading'
import { Textarea } from '@/components/ui/Textarea'
import { useToast } from '@/components/ui/Toast'
import { QueryError } from '@/components/layout/AppShell'
import type { MessageDto } from '@/types/api'
import { chatDateGroup, formatDate } from '@/utils/format'
import { cn } from '@/utils/cn'

export function ChatsPage() {
  const navigate = useNavigate()
  const [open, setOpen] = useState(false)
  const [q, setQ] = useState('')
  const { canManageCases } = useAuth()
  const chatsQuery = useQuery({ queryKey: queryKeys.chats, queryFn: chatsApi.list })
  const foldersQuery = useQuery({
    queryKey: queryKeys.folders,
    queryFn: foldersApi.list,
    enabled: canManageCases,
    retry: false,
  })

  const chats = (chatsQuery.data ?? []).filter((c) => c.title.toLowerCase().includes(q.toLowerCase()))
  const grouped = useMemo(() => {
    const groups: Record<string, typeof chats> = { Hoy: [], 'Esta semana': [], Anteriores: [] }
    for (const chat of [...chats].sort((a, b) => b.createdAt.localeCompare(a.createdAt))) {
      groups[chatDateGroup(chat.createdAt)].push(chat)
    }
    return groups
  }, [chats])

  return (
    <div className="px-5 py-6 lg:px-8">
      <div className="mb-6 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-[28px] font-semibold">Chats</h1>
          <p className="mt-1 text-[14px] text-muted">Conversaciones con contexto de documentos y skills.</p>
        </div>
        <Button onClick={() => setOpen(true)}>+ Nuevo chat</Button>
      </div>
      <input
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Buscar por título"
        className="mb-4 h-10 w-full max-w-md rounded-[8px] border border-border-strong bg-surface px-3 text-[14px]"
      />
      {chatsQuery.isLoading ? <Skeleton className="h-64" /> : null}
      {chatsQuery.isError ? (
        <QueryError message="No pudimos cargar tus chats." onRetry={() => chatsQuery.refetch()} />
      ) : null}
      {!chatsQuery.isLoading && chats.length === 0 ? (
        <EmptyState
          title="Todavía no tenés conversaciones."
          description="Creá un chat para empezar a trabajar con JurisApp."
          action={{ label: 'Nuevo chat', onClick: () => setOpen(true) }}
        />
      ) : (
        <div className="divide-y divide-border rounded-[12px] border border-border bg-surface">
          {(['Hoy', 'Esta semana', 'Anteriores'] as const).map((group) =>
            grouped[group].length ? (
              <div key={group}>
                <p className="bg-subtle px-4 py-2 text-[12px] font-medium uppercase tracking-wide text-muted">
                  {group}
                </p>
                {grouped[group].map((chat) => {
                  const folder = foldersQuery.data?.find((f) => f.id === chat.folderId)
                  return (
                    <button
                      key={chat.id}
                      type="button"
                      className="flex w-full items-center justify-between px-4 py-3 text-left hover:bg-subtle"
                      onClick={() => navigate(`/app/chats/${chat.id}`)}
                    >
                      <span>
                        <span className="block text-[14px] font-medium">{chat.title}</span>
                        <span className="text-[12px] text-muted">
                          {folder ? `${folder.name} · ` : ''}
                          {formatDate(chat.createdAt)}
                        </span>
                      </span>
                    </button>
                  )
                })}
              </div>
            ) : null,
          )}
        </div>
      )}
      <CreateChatModal open={open} onClose={() => setOpen(false)} />
    </div>
  )
}

export function ChatWorkspacePage() {
  const { chatId } = useParams<{ chatId: string }>()
  const navigate = useNavigate()
  const toast = useToast()
  const { canManageCases } = useAuth()
  const queryClient = useQueryClient()
  const [message, setMessage] = useState('')
  const [taskMode, setTaskMode] = useState(false)
  const [menu, setMenu] = useState(false)
  const [deleteOpen, setDeleteOpen] = useState(false)
  const [uploadOpen, setUploadOpen] = useState(false)
  const [contextOpen, setContextOpen] = useState(false)
  const [pendingUser, setPendingUser] = useState<string | null>(null)
  const [sendError, setSendError] = useState<string | null>(null)
  const bottomRef = useRef<HTMLDivElement>(null)

  const chatsQuery = useQuery({ queryKey: queryKeys.chats, queryFn: chatsApi.list })
  const chatQuery = useQuery({
    queryKey: queryKeys.chat(chatId ?? ''),
    queryFn: () => chatsApi.getById(chatId!),
    enabled: Boolean(chatId),
  })
  const docsQuery = useQuery({
    queryKey: queryKeys.chatDocuments(chatId ?? ''),
    queryFn: () => documentsApi.listByChat(chatId!),
    enabled: Boolean(chatId),
  })
  const folderDocsQuery = useQuery({
    queryKey: queryKeys.folderDocuments(chatQuery.data?.folderId ?? ''),
    queryFn: () => documentsApi.listByFolder(chatQuery.data!.folderId!),
    enabled: Boolean(chatQuery.data?.folderId),
  })
  const tasksQuery = useQuery({
    queryKey: queryKeys.chatTasks(chatId ?? ''),
    queryFn: () => tasksApi.listByChat(chatId!),
    enabled: Boolean(chatId),
    refetchInterval: (query) => {
      const list = query.state.data
      return list?.some((t) => t.status === 'InProgress') ? 2000 : false
    },
  })
  const foldersQuery = useQuery({
    queryKey: queryKeys.folders,
    queryFn: foldersApi.list,
    enabled: canManageCases,
    retry: false,
  })
  const skillsQuery = useQuery({
    queryKey: queryKeys.skills,
    queryFn: skillsApi.list,
    enabled: canManageCases,
    retry: false,
  })

  const chat = chatQuery.data
  const folder = foldersQuery.data?.find((f) => f.id === chat?.folderId)
  const chatDocuments = docsQuery.data ?? []
  const caseDocuments = folderDocsQuery.data ?? []
  const running = tasksQuery.data?.some((t) => t.status === 'InProgress')

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [chat?.messages.length, pendingUser])

  const sendMutation = useMutation({
    mutationFn: (content: string) => chatsApi.sendMessage(chatId!, { content }),
    onSuccess: () => {
      setPendingUser(null)
      queryClient.invalidateQueries({ queryKey: queryKeys.chat(chatId!) })
    },
    onError: (err) => {
      setPendingUser(null)
      setSendError(errorMessage(err, 'No pudimos enviar el mensaje.'))
    },
  })

  const taskMutation = useMutation({
    mutationFn: (description: string) => tasksApi.create({ chatId: chatId!, description }),
    onSuccess: () => {
      setMessage('')
      setTaskMode(false)
      queryClient.invalidateQueries({ queryKey: queryKeys.chatTasks(chatId!) })
      queryClient.invalidateQueries({ queryKey: queryKeys.chat(chatId!) })
    },
    onError: (err) => setSendError(errorMessage(err, 'No pudimos generar el plan.')),
  })

  const deleteMutation = useMutation({
    mutationFn: () => chatsApi.delete(chatId!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.chats })
      toast('Conversación eliminada.')
      navigate('/app/chats')
    },
  })

  const applySkill = useMutation({
    mutationFn: (customSkillId: string) => skillsApi.applyToChat({ chatId: chatId!, customSkillId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.chat(chatId!) }),
    onError: (err) => setSendError(errorMessage(err)),
  })
  const removeSkill = useMutation({
    mutationFn: (customSkillId: string) => skillsApi.removeFromChat({ chatId: chatId!, customSkillId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: queryKeys.chat(chatId!) }),
  })

  if (chatQuery.isError) {
    const forbidden = chatQuery.error instanceof ApiError && chatQuery.error.status === 403
    return (
      <div className="p-8">
        {forbidden ? (
          <p className="text-[14px]">No tenés acceso a esta conversación.</p>
        ) : (
          <QueryError message="No pudimos cargar el chat." onRetry={() => chatQuery.refetch()} />
        )}
      </div>
    )
  }

  const messages: MessageDto[] = chat?.messages ?? []

  const submit = () => {
    const content = message.trim()
    if (!content || !chatId) return
    setSendError(null)
    if (taskMode) {
      taskMutation.mutate(content)
      return
    }
    setPendingUser(content)
    setMessage('')
    sendMutation.mutate(content)
  }

  const contextPanel = (
    <div className="space-y-6 p-4 text-[14px]">
      <p className="text-[11px] font-medium uppercase tracking-wide text-faint">Contexto</p>
      <div>
        <p className="text-[11px] uppercase tracking-wide text-faint">Caso</p>
        {folder ? (
          <>
            <Link to={`/app/cases/${folder.id}`} className="mt-1 block font-medium text-blue-600 hover:underline">
              {folder.name}
            </Link>
            {folder.legalContext ? (
              <p className="mt-1 text-[12px] text-muted">{folder.legalContext}</p>
            ) : null}
          </>
        ) : (
          <p className="mt-1 text-muted">Sin caso asociado</p>
        )}
      </div>
      <div>
        <p className="text-[11px] uppercase tracking-wide text-faint">Documentos del caso</p>
        {caseDocuments.length === 0 ? (
          <p className="mt-1 text-muted">{folder ? 'Ninguno en este caso' : 'Este chat no está en un caso'}</p>
        ) : (
          <ul className="mt-1 space-y-1">
            {caseDocuments.map((doc) => (
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
        <p className="text-[11px] uppercase tracking-wide text-faint">Documentos de este chat</p>
        <ul className="mt-1 space-y-1">
          {chatDocuments.map((doc) => (
            <li key={doc.id}>
              <Link to={`/app/documents/${doc.id}`} className="hover:underline">
                {doc.title}
              </Link>
            </li>
          ))}
        </ul>
        {chatDocuments.length === 0 ? <p className="mt-1 text-muted">Ninguno</p> : null}
        <Button size="sm" variant="ghost" className="mt-2 px-0" onClick={() => setUploadOpen(true)}>
          + Adjuntar a este chat
        </Button>
      </div>
      <div>
        <p className="text-[11px] uppercase tracking-wide text-faint">Skill</p>
        {(chat?.appliedSkills ?? []).map((skill) => (
          <div key={skill.id} className="mt-1 flex items-center justify-between">
            <span>{skill.name}</span>
            <button
              type="button"
              className="text-[12px] text-muted hover:text-danger"
              onClick={() => removeSkill.mutate(skill.id)}
            >
              Quitar
            </button>
          </div>
        ))}
        {(chat?.appliedSkills ?? []).length === 0 ? <p className="mt-1 text-muted">Ninguna</p> : null}
      </div>
    </div>
  )

  return (
    <div className="flex h-[calc(100dvh-57px)] min-h-[560px] bg-canvas md:h-dvh">
      <aside className="hidden w-[240px] shrink-0 border-r border-border bg-surface lg:flex lg:flex-col">
        <div className="flex items-center justify-between border-b border-border px-3 py-3">
          <p className="text-[13px] font-semibold">Chats</p>
          <CreateChatButton />
        </div>
        <div className="flex-1 overflow-y-auto">
          {(chatsQuery.data ?? []).map((item) => (
            <Link
              key={item.id}
              to={`/app/chats/${item.id}`}
              className={cn(
                'block border-l-2 px-3 py-2.5 text-[13px]',
                item.id === chatId ? 'border-sky-500 bg-subtle font-medium' : 'border-transparent hover:bg-subtle',
              )}
            >
              {item.title}
            </Link>
          ))}
        </div>
      </aside>

      <section className="flex min-w-0 flex-1 flex-col">
        <header className="flex items-center gap-3 border-b border-border bg-surface px-4 py-3">
          <button type="button" className="lg:hidden" onClick={() => navigate('/app/chats')} aria-label="Volver">
            <ArrowLeft size={18} />
          </button>
          <div className="min-w-0 flex-1">
            {chatQuery.isLoading ? <Skeleton className="h-5 w-48" /> : (
              <>
                <h1 className="truncate text-[16px] font-semibold">{chat?.title}</h1>
                {folder ? <p className="truncate text-[12px] text-muted">Caso: {folder.name}</p> : null}
              </>
            )}
          </div>
          <Button size="sm" variant="ghost" className="lg:hidden" onClick={() => setContextOpen(true)}>
            <PanelRight size={16} /> Contexto
          </Button>
          <div className="relative">
            <button type="button" className="rounded-[8px] p-1.5 hover:bg-subtle" onClick={() => setMenu((v) => !v)} aria-label="Más acciones">
              <MoreHorizontal size={18} />
            </button>
            {menu ? (
              <div className="absolute right-0 z-10 mt-1 w-44 rounded-[8px] border border-border bg-surface py-1 shadow-lg">
                <button
                  type="button"
                  className="block w-full px-3 py-2 text-left text-[13px] text-danger hover:bg-subtle"
                  onClick={() => {
                    setMenu(false)
                    setDeleteOpen(true)
                  }}
                >
                  Eliminar conversación
                </button>
              </div>
            ) : null}
          </div>
        </header>

        <div className="min-h-0 flex-1 overflow-y-auto px-4 py-5 lg:px-8">
          {chatQuery.isLoading ? (
            <div className="space-y-3">
              <Skeleton className="h-16" />
              <Skeleton className="h-24" />
            </div>
          ) : (
            <div className="mx-auto max-w-3xl space-y-5">
              {messages.map((msg) => (
                <MessageBubble key={msg.id} message={msg} />
              ))}
              {pendingUser ? (
                <MessageBubble
                  message={{
                    id: 'pending',
                    chatId: chatId ?? '',
                    role: 'User',
                    content: pendingUser,
                    date: new Date().toISOString(),
                    skillsUsed: [],
                  }}
                />
              ) : null}
              {sendMutation.isPending ? <p className="text-[13px] text-muted">JurisApp está respondiendo…</p> : null}
              {taskMutation.isPending ? <p className="text-[13px] text-muted">Generando plan…</p> : null}

              {(tasksQuery.data ?? []).map((task) => (
                <TaskPlanCard key={task.id} task={task} />
              ))}
              <div ref={bottomRef} />
            </div>
          )}
        </div>

        <div className="border-t border-border bg-surface px-4 py-3 lg:px-8">
          <div className="mx-auto max-w-3xl">
            {sendError ? <Alert className="mb-2">{sendError}</Alert> : null}
            {folder && caseDocuments.length > 0 ? (
              <p className="mb-2 text-[12px] text-muted">
                JurisApp usa el contexto del caso y {caseDocuments.length === 1 ? 'el documento adjunto' : `los ${caseDocuments.length} documentos`} al responder.
              </p>
            ) : null}
            {taskMode ? (
              <p className="mb-2 text-[12px] font-medium text-blue-600">
                Modo tarea — describí el objetivo. JurisApp va a proponer un plan antes de ejecutarlo.
              </p>
            ) : null}
            <div className="rounded-[12px] border border-border-strong bg-canvas p-3">
              <Textarea
                className="min-h-20 border-0 bg-transparent p-0 focus:border-0"
                placeholder={
                  taskMode
                    ? 'Ej: Analizá estos documentos, identificá los principales riesgos contractuales y prepará recomendaciones.'
                    : 'Escribí tu consulta...'
                }
                value={message}
                onChange={(e) => setMessage(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault()
                    submit()
                  }
                }}
              />
              <div className="mt-2 flex flex-wrap items-center gap-2">
                <Button size="sm" variant="ghost" onClick={() => setUploadOpen(true)}>
                  <Paperclip size={14} /> Documento
                </Button>
                {canManageCases && (skillsQuery.data ?? []).filter((s) => s.isActive).length > 0 ? (
                  <select
                    className="h-8 rounded-[8px] border border-border bg-surface px-2 text-[12px]"
                    defaultValue=""
                    onChange={(e) => {
                      if (e.target.value) applySkill.mutate(e.target.value)
                      e.target.value = ''
                    }}
                  >
                    <option value="">Skill: aplicar…</option>
                    {(skillsQuery.data ?? [])
                      .filter((s) => s.isActive)
                      .map((skill) => (
                        <option key={skill.id} value={skill.id}>
                          {skill.name}
                        </option>
                      ))}
                  </select>
                ) : null}
                <button
                  type="button"
                  className={cn(
                    'h-8 rounded-[8px] px-2 text-[12px] font-medium',
                    taskMode ? 'bg-navy-900 text-white' : 'text-muted hover:bg-subtle',
                  )}
                  onClick={() => setTaskMode((v) => !v)}
                >
                  Modo tarea
                </button>
                <div className="ml-auto">
                  <Button
                    size="sm"
                    loading={sendMutation.isPending || taskMutation.isPending}
                    onClick={submit}
                    disabled={running && taskMode}
                  >
                    {taskMode ? 'Generar plan' : 'Enviar'}
                  </Button>
                </div>
              </div>
            </div>
            <AiDisclaimer className="mt-2" />
          </div>
        </div>
      </section>

      <aside className="hidden w-[280px] shrink-0 border-l border-border bg-surface xl:block">{contextPanel}</aside>

      {contextOpen ? (
        <div className="fixed inset-0 z-40 xl:hidden">
          <button type="button" className="absolute inset-0 bg-navy-950/40" aria-label="Cerrar" onClick={() => setContextOpen(false)} />
          <div className="absolute inset-y-0 right-0 w-[min(100%,320px)] bg-surface shadow-lg">{contextPanel}</div>
        </div>
      ) : null}

      <UploadDocumentModal open={uploadOpen} onClose={() => setUploadOpen(false)} lockChatId={chatId} />
      <ConfirmDialog
        open={deleteOpen}
        title="Eliminar conversación"
        description="Esta acción eliminará la conversación según el comportamiento definido por JurisApp."
        confirmLabel="Eliminar"
        danger
        loading={deleteMutation.isPending}
        onConfirm={() => deleteMutation.mutate()}
        onClose={() => setDeleteOpen(false)}
      />
    </div>
  )
}

function MessageBubble({ message }: { message: MessageDto }) {
  const isUser = message.role === 'User'
  return (
    <div className={cn('flex', isUser ? 'justify-end' : 'justify-start')}>
      <div
        className={cn(
          isUser
            ? 'max-w-[80%] rounded-[8px] bg-subtle px-3 py-2 text-[14px]'
            : 'max-w-[min(100%,720px)]',
        )}
      >
        {isUser ? (
          <p className="whitespace-pre-wrap">{message.content}</p>
        ) : (
          <>
            <MarkdownBody content={message.content} />
            {message.skillsUsed?.length ? (
              <div className="mt-2 flex flex-wrap gap-1">
                {message.skillsUsed.map((name) => (
                  <Badge key={name} tone="info">
                    {name}
                  </Badge>
                ))}
              </div>
            ) : null}
          </>
        )}
      </div>
    </div>
  )
}

function CreateChatButton() {
  const [open, setOpen] = useState(false)
  return (
    <>
      <Button size="sm" variant="ghost" onClick={() => setOpen(true)}>
        +
      </Button>
      <CreateChatModal open={open} onClose={() => setOpen(false)} />
    </>
  )
}
