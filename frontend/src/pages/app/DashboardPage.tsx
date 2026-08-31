import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useQueries, useQuery } from '@tanstack/react-query'
import { chatsApi, documentsApi, foldersApi, plansApi, tasksApi } from '@/api'
import { queryKeys } from '@/api/queryKeys'
import { useAuth } from '@/app/AuthContext'
import { AppPage, QueryError } from '@/components/layout/AppShell'
import { CreateChatModal } from '@/components/chat/CreateChatModal'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { EmptyState } from '@/components/ui/EmptyState'
import { Skeleton } from '@/components/ui/Loading'
import { formatDate, greetingForNow, taskStatusLabel } from '@/utils/format'
import type { AITaskDto, ChatSummaryDto, DocumentDto, FolderDto } from '@/types/api'

export function DashboardPage() {
  const { user, canManageCases } = useAuth()
  const navigate = useNavigate()
  const [newChat, setNewChat] = useState(false)

  const chatsQuery = useQuery({ queryKey: queryKeys.chats, queryFn: chatsApi.list })
  const foldersQuery = useQuery({
    queryKey: queryKeys.folders,
    queryFn: foldersApi.list,
    enabled: canManageCases,
  })
  const planQuery = useQuery({ queryKey: queryKeys.currentPlan, queryFn: plansApi.current })

  const chats = chatsQuery.data ?? []
  const recentChats = [...chats].sort((a, b) => b.createdAt.localeCompare(a.createdAt)).slice(0, 8)
  const folders = foldersQuery.data ?? []

  const docsQueries = useQueries({
    queries: recentChats.slice(0, 6).map((chat) => ({
      queryKey: queryKeys.chatDocuments(chat.id),
      queryFn: () => documentsApi.listByChat(chat.id),
    })),
  })
  const folderDocsQueries = useQueries({
    queries: folders.slice(0, 6).map((folder) => ({
      queryKey: queryKeys.folderDocuments(folder.id),
      queryFn: () => documentsApi.listByFolder(folder.id),
      enabled: canManageCases,
    })),
  })
  const taskQueries = useQueries({
    queries: recentChats.slice(0, 5).map((chat) => ({
      queryKey: queryKeys.chatTasks(chat.id),
      queryFn: () => tasksApi.listByChat(chat.id),
    })),
  })

  const documents: { doc: DocumentDto; location: string }[] = []
  recentChats.slice(0, 6).forEach((chat, i) => {
    for (const doc of docsQueries[i]?.data ?? []) {
      documents.push({ doc, location: chat.title })
    }
  })
  folders.slice(0, 6).forEach((folder, i) => {
    for (const doc of folderDocsQueries[i]?.data ?? []) {
      if (!documents.some((row) => row.doc.id === doc.id)) {
        documents.push({ doc, location: folder.name })
      }
    }
  })

  const tasks: { task: AITaskDto; chat: ChatSummaryDto }[] = []
  recentChats.slice(0, 5).forEach((chat, i) => {
    for (const task of taskQueries[i]?.data ?? []) {
      tasks.push({ task, chat })
    }
  })
  const activeTasks = tasks
    .filter((row) => row.task.status === 'InProgress' || row.task.status === 'AwaitingApproval')
    .slice(0, 4)

  if (chatsQuery.isError) {
    return (
      <AppPage>
        <QueryError message="No pudimos cargar tu espacio de trabajo." onRetry={() => chatsQuery.refetch()} />
      </AppPage>
    )
  }

  return (
    <AppPage>
      <div className="mb-8 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-[28px] font-semibold text-ink">
            {greetingForNow()}, {user ? user.firstName : ''}
          </h1>
          <p className="mt-1 text-[14px] text-muted">
            {canManageCases ? 'Tu espacio de trabajo jurídico.' : 'Tus conversaciones, documentos e IA.'}
          </p>
        </div>
        <div className="flex gap-2">
          {canManageCases ? (
            <Button variant="secondary" onClick={() => navigate('/app/cases')}>
              + Nuevo caso
            </Button>
          ) : null}
          <Button onClick={() => setNewChat(true)}>+ Nuevo chat</Button>
        </div>
      </div>

      {!user?.isEmailVerified ? (
        <div className="mb-6 rounded-[8px] border border-warning/20 bg-warning-bg px-4 py-3 text-[13px] text-warning">
          Tu email todavía no está verificado.{' '}
          <Link to="/verify-email" className="underline">
            Verificar ahora
          </Link>
        </div>
      ) : null}

      <div className="mb-6 flex items-center justify-between rounded-[12px] border border-border bg-surface px-4 py-3">
        <p className="text-[14px]">
          Plan <span className="font-semibold">{planQuery.data?.planName ?? '—'}</span>
        </p>
        <Link to="/app/subscription" className="text-[13px] text-blue-600 hover:underline">
          Gestionar plan →
        </Link>
      </div>

      {canManageCases ? (
      <section className="mb-10">
        <h2 className="mb-3 text-[20px] font-semibold">Trabajo reciente</h2>
        {foldersQuery.isLoading ? (
          <Skeleton className="h-40" />
        ) : folders.length === 0 ? (
          <EmptyState
            title="Todavía no creaste ningún caso."
            description="Agrupá documentos y conversaciones relacionadas en un mismo espacio."
            action={{ label: 'Nuevo caso', onClick: () => navigate('/app/cases') }}
          />
        ) : (
          <div className="divide-y divide-border rounded-[12px] border border-border bg-surface">
            {folders.slice(0, 5).map((folder) => (
              <CaseRow key={folder.id} folder={folder} chats={chats} />
            ))}
          </div>
        )}
      </section>
      ) : null}

      <div className="grid gap-8 lg:grid-cols-2">
        <section>
          <h2 className="mb-3 text-[20px] font-semibold">Conversaciones recientes</h2>
          {chats.length === 0 ? (
            <EmptyState
              title="Todavía no tenés conversaciones."
              description="Creá un chat para empezar a trabajar con JurisApp."
              action={{ label: 'Nuevo chat', onClick: () => setNewChat(true) }}
            />
          ) : (
            <Card className="divide-y divide-border">
              {recentChats.map((chat) => {
                const folder = folders.find((f) => f.id === chat.folderId)
                return (
                  <Link key={chat.id} to={`/app/chats/${chat.id}`} className="block px-4 py-3 hover:bg-subtle">
                    <p className="text-[14px] font-medium">{chat.title}</p>
                    <p className="text-[12px] text-muted">
                      {folder ? `${folder.name} · ` : ''}
                      {formatDate(chat.createdAt)}
                    </p>
                  </Link>
                )
              })}
            </Card>
          )}
        </section>

        <section>
          <h2 className="mb-3 text-[20px] font-semibold">Documentos recientes</h2>
          {documents.length === 0 ? (
            <EmptyState
              title="No hay documentos todavía."
              description="Subí un archivo desde un chat o caso para mantenerlo asociado a su contexto."
            />
          ) : (
            <Card className="divide-y divide-border">
              {documents.slice(0, 8).map(({ doc, location }) => (
                <Link key={doc.id} to={`/app/documents/${doc.id}`} className="block px-4 py-3 hover:bg-subtle">
                  <p className="text-[14px] font-medium">{doc.title}</p>
                  <p className="text-[12px] text-muted">{location}</p>
                </Link>
              ))}
            </Card>
          )}
        </section>
      </div>

      {activeTasks.length > 0 ? (
        <section className="mt-10">
          <h2 className="mb-3 text-[20px] font-semibold">Tareas con IA</h2>
          <Card className="divide-y divide-border">
            {activeTasks.map(({ task, chat }) => {
              const done = task.steps.filter((s) => s.status === 'Completed').length
              return (
                <Link key={task.id} to={`/app/chats/${chat.id}`} className="block px-4 py-3 hover:bg-subtle">
                  <p className="text-[14px] font-medium">{task.description}</p>
                  <p className="text-[12px] text-muted">
                    {chat.title}
                    {task.steps.length ? ` · ${done} de ${task.steps.length} pasos` : ''}
                    {' · '}
                    {taskStatusLabel(task.status, task.isPaused)}
                  </p>
                </Link>
              )
            })}
          </Card>
        </section>
      ) : null}

      <CreateChatModal open={newChat} onClose={() => setNewChat(false)} />
    </AppPage>
  )
}

function CaseRow({ folder, chats }: { folder: FolderDto; chats: ChatSummaryDto[] }) {
  const chatCount = chats.filter((c) => c.folderId === folder.id).length
  const docs = useQuery({
    queryKey: queryKeys.folderDocuments(folder.id),
    queryFn: () => documentsApi.listByFolder(folder.id),
  })
  return (
    <Link to={`/app/cases/${folder.id}`} className="flex items-center justify-between px-4 py-3 hover:bg-subtle">
      <div>
        <p className="text-[14px] font-medium">{folder.name}</p>
        <p className="text-[12px] text-muted">
          Caso
          {docs.data ? ` · ${docs.data.length} documentos` : ''}
          {` · ${chatCount} chats`}
        </p>
      </div>
      <span className="text-[13px] text-blue-600">Abrir →</span>
    </Link>
  )
}
