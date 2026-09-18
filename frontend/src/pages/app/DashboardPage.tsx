import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { ArrowUpRight, FolderOpen, FolderPlus, MessageSquarePlus, Plus, Clock3 } from 'lucide-react'
import { chatsApi, foldersApi } from '@/api'
import { queryKeys } from '@/api/queryKeys'
import { useAuth } from '@/app/AuthContext'
import { AppPage, QueryError } from '@/components/layout/AppShell'
import { CreateChatModal } from '@/components/chat/CreateChatModal'
import { Button } from '@/components/ui/Button'
import { Skeleton } from '@/components/ui/Loading'
import { formatDate, greetingForNow } from '@/utils/format'
import type { ChatSummaryDto, FolderDto } from '@/types/api'

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

  const chats = [...(chatsQuery.data ?? [])].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
  const folders = canManageCases ? (foldersQuery.data ?? []) : []

  if (chatsQuery.isError) {
    return (
      <AppPage wide>
        <QueryError message="No pudimos cargar tu espacio de trabajo." onRetry={() => chatsQuery.refetch()} />
      </AppPage>
    )
  }

  return (
    <AppPage wide>
      <div className="mx-auto max-w-7xl">
        <div className="mb-8 flex flex-wrap items-center justify-between gap-4">
          <div>
            <p className="text-[11px] font-medium uppercase tracking-[0.14em] text-muted">Workspace</p>
            <h1 className="mt-2 text-[30px] font-semibold tracking-[-0.04em] text-ink">
              {greetingForNow()}, {user ? user.firstName : ''}
            </h1>
          </div>
        </div>

        {!user?.isEmailVerified ? (
          <div className="mb-6 rounded-[12px] border border-warning/20 bg-warning-bg px-4 py-3 text-[13px] text-warning">
            Tu email todavía no está verificado.{' '}
            <Link to="/verify-email" className="underline">
              Verificar ahora
            </Link>
          </div>
        ) : null}

        {canManageCases ? (
          <section className="mb-8">
            <div className="mb-4 flex items-center justify-between gap-3">
              <div>
                <p className="text-[11px] font-medium uppercase tracking-[0.14em] text-muted">Carpetas</p>
                <h2 className="mt-1 text-[20px] font-semibold text-ink">Espacios de trabajo</h2>
              </div>

              <Button onClick={() => navigate('/app/cases')} className="bg-navy-900 text-white hover:bg-navy-800">
                <FolderPlus size={16} />
                Crear caso
              </Button>
            </div>

            {foldersQuery.isLoading ? (
              <Skeleton className="h-36" />
            ) : folders.length === 0 ? (
              <div className="rounded-[20px] border border-border/60 bg-surface p-8 shadow-[0_16px_30px_rgba(15,23,42,0.02)]">
                <div className="mx-auto flex max-w-md flex-col items-center text-center">
                  <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-subtle text-muted">
                    <FolderOpen size={22} />
                  </div>
                  <p className="text-base font-medium text-ink">Todavía no creaste ningún caso.</p>
                  <p className="mt-2 text-sm text-muted">
                    Agrupá tus asuntos por carpeta para mantener todo ordenado y conectado.
                  </p>
                </div>
              </div>
            ) : (
              <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-5">
                <button
                  type="button"
                  onClick={() => navigate('/app/cases')}
                  className="group flex min-h-[170px] flex-col justify-between rounded-[18px] border border-dashed border-border-strong bg-surface p-4 text-left shadow-[0_16px_30px_rgba(15,23,42,0.02)] transition duration-200 hover:-translate-y-0.5 hover:border-blue-500 hover:bg-blue-50/40"
                >
                  <div className="flex items-center justify-between">
                    <div className="flex h-12 w-12 items-center justify-center rounded-[12px] border border-border bg-subtle text-blue-700">
                      <Plus size={22} strokeWidth={2.5} />
                    </div>
                  </div>
                  <div>
                    <p className="text-[15px] font-semibold text-ink">Nueva carpeta</p>
                    <p className="mt-1 text-[12px] text-muted">Crear caso nuevo</p>
                  </div>
                </button>

                {folders.map((folder) => (
                  <FolderCard key={folder.id} folder={folder} chats={chats} />
                ))}
              </div>
            )}
          </section>
        ) : null}

        <section>
          <div className="mb-4 flex items-center justify-between gap-3">
            <div>
              <p className="text-[11px] font-medium uppercase tracking-[0.14em] text-muted">Conversaciones</p>
              <h2 className="mt-1 text-[20px] font-semibold text-ink">Actividad reciente</h2>
            </div>

            <Button onClick={() => setNewChat(true)} className="bg-navy-900 text-white hover:bg-navy-800">
              <MessageSquarePlus size={16} />
              Nuevo chat
            </Button>
          </div>

          {chats.length === 0 ? (
            <div className="rounded-[20px] border border-border/60 bg-surface p-8 shadow-[0_16px_30px_rgba(15,23,42,0.02)]">
              <div className="mx-auto flex max-w-md flex-col items-center text-center">
                <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-subtle text-muted">
                  <FolderPlus size={22} strokeWidth={1.5} />
                </div>
                <p className="text-base font-medium text-ink">Todavía no tenés conversaciones.</p>
                <p className="mt-2 text-sm text-muted">
                  Comenzá un chat para registrar la estrategia, el análisis o el seguimiento del caso.
                </p>
              </div>
            </div>
          ) : (
            <div className="overflow-hidden rounded-[18px] border border-border bg-surface shadow-[0_16px_30px_rgba(15,23,42,0.02)]">
              {chats.map((chat) => {
                const folder = folders.find((item) => item.id === chat.folderId)

                return (
                  <Link
                    key={chat.id}
                    to={`/app/chats/${chat.id}`}
                    className="group flex items-center justify-between gap-4 border-b border-border px-4 py-4 last:border-b-0 transition-colors hover:bg-subtle"
                  >
                    <div className="flex min-w-0 items-center gap-3">
                      <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-[12px] bg-subtle text-blue-700">
                        <MessageSquarePlus size={18} />
                      </div>
                      <div className="min-w-0">
                        <p className="truncate text-[15px] font-medium text-ink">{chat.title}</p>
                        <div className="mt-1 flex flex-wrap items-center gap-2 text-[12px] text-muted">
                          {folder ? <span>{folder.name}</span> : <span>Sin carpeta</span>}
                          <span className="text-faint">•</span>
                          <span className="inline-flex items-center gap-1">
                            <Clock3 size={12} />
                            Última visita {formatDate(chat.createdAt)}
                          </span>
                        </div>
                      </div>
                    </div>

                    <span className="inline-flex items-center gap-1 text-[12px] font-medium text-blue-600">
                      Abrir
                      <ArrowUpRight size={14} />
                    </span>
                  </Link>
                )
              })}
            </div>
          )}
        </section>
      </div>

      <CreateChatModal open={newChat} onClose={() => setNewChat(false)} />
    </AppPage>
  )
}

function FolderCard({ folder, chats }: { folder: FolderDto; chats: ChatSummaryDto[] }) {
  const folderChats = chats.filter((chat) => chat.folderId === folder.id).length

  return (
    <Link
      to={`/app/cases/${folder.id}`}
      className="group flex min-h-[170px] flex-col justify-between rounded-[18px] border border-border bg-surface p-4 shadow-[0_16px_30px_rgba(15,23,42,0.02)] transition duration-200 hover:-translate-y-0.5 hover:border-blue-500 hover:shadow-[0_18px_32px_rgba(37,99,235,0.08)]"
    >
      <div className="flex items-start justify-between gap-2">
        <div className="flex h-12 w-12 items-center justify-center rounded-[12px] bg-[#edf5ff] text-blue-700">
          <FolderOpen size={22} strokeWidth={2} />
        </div>
        <span className="rounded-full border border-border bg-subtle px-2 py-1 text-[11px] font-medium uppercase tracking-[0.08em] text-muted">
          {folderChats} chat{folderChats === 1 ? '' : 's'}
        </span>
      </div>

      <div>
        <p className="line-clamp-2 text-[15px] font-semibold text-ink">{folder.name}</p>
        <p className="mt-2 text-[12px] text-muted">{folder.legalContext?.trim() || 'Sin descripción'}</p>
      </div>
    </Link>
  )
}
