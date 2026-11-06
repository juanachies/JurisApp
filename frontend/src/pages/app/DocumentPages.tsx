import { useEffect, useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useQueries, useQuery, useQueryClient } from '@tanstack/react-query'
import { chatsApi, documentsApi, foldersApi } from '@/api'
import { ApiError, fileUrl } from '@/api/client'
import { queryKeys } from '@/api/queryKeys'
import { useAuth } from '@/app/AuthContext'
import { AppPage, QueryError } from '@/components/layout/AppShell'
import { ensureCompleteAnalysis, readAnalysisRun, retryCompleteAnalysis, useAnalysisRun } from '@/components/documents/analysisRun'
import { UploadDocumentModal } from '@/components/documents/UploadDocumentModal'
import { Alert } from '@/components/ui/Alert'
import { Button, ButtonLink, buttonClass } from '@/components/ui/Button'
import { EmptyState } from '@/components/ui/EmptyState'
import { MarkdownBody, AiDisclaimer } from '@/components/ui/MarkdownBody'
import { PageHeader } from '@/components/ui/PageHeader'
import { Table, TBody, Td, Th, THead, Tr } from '@/components/ui/Table'
import { TableSkeleton } from '@/components/ui/Loading'
import type { DocumentAnalysisDto, DocumentDto } from '@/types/api'
import { fileExtension } from '@/utils/format'

type Filter = 'all' | 'cases' | 'chats'

export function DocumentsPage() {
  const { canManageCases } = useAuth()
  const [filter, setFilter] = useState<Filter>('all')
  const [upload, setUpload] = useState(false)
  const [q, setQ] = useState('')

  const chatsQuery = useQuery({ queryKey: queryKeys.chats, queryFn: chatsApi.list })
  const foldersQuery = useQuery({
    queryKey: queryKeys.folders,
    queryFn: foldersApi.list,
    enabled: canManageCases,
    retry: false,
  })

  const chatDocs = useQueries({
    queries: (chatsQuery.data ?? []).map((chat) => ({
      queryKey: queryKeys.chatDocuments(chat.id),
      queryFn: () => documentsApi.listByChat(chat.id),
      enabled: Boolean(chatsQuery.data),
    })),
  })
  const folderDocs = useQueries({
    queries: (foldersQuery.data ?? []).map((folder) => ({
      queryKey: queryKeys.folderDocuments(folder.id),
      queryFn: () => documentsApi.listByFolder(folder.id),
      enabled: canManageCases && Boolean(foldersQuery.data),
    })),
  })

  const rows = useMemo(() => {
    const list: { doc: DocumentDto; location: string; kind: Filter }[] = []
    ;(chatsQuery.data ?? []).forEach((chat, i) => {
      for (const doc of chatDocs[i]?.data ?? []) {
        list.push({ doc, location: chat.title, kind: 'chats' })
      }
    })
    ;(foldersQuery.data ?? []).forEach((folder, i) => {
      for (const doc of folderDocs[i]?.data ?? []) {
        if (!list.some((row) => row.doc.id === doc.id)) {
          list.push({ doc, location: folder.name, kind: 'cases' })
        }
      }
    })
    return list.filter((row) => {
      if (filter !== 'all' && row.kind !== filter) return false
      return row.doc.title.toLowerCase().includes(q.toLowerCase())
    })
  }, [chatsQuery.data, foldersQuery.data, chatDocs, folderDocs, filter, q])

  const loading = chatsQuery.isLoading || (canManageCases && foldersQuery.isLoading)

  return (
    <AppPage>
      <PageHeader
        title="Documentos"
        description="Tus documentos jurídicos en un solo lugar, siempre asociados a un chat o a un caso."
        actions={<Button onClick={() => setUpload(true)}>Subir documento</Button>}
      />
      <div className="mb-4 flex flex-wrap items-center gap-2">
        {(['all', 'cases', 'chats'] as const).map((id) => (
          <button
            key={id}
            type="button"
            className={`rounded-full border px-3 py-1.5 text-[13px] ${filter === id ? 'border-navy-900 bg-navy-900 text-white' : 'border-border bg-surface text-muted'}`}
            onClick={() => setFilter(id)}
          >
            {id === 'all' ? 'Todos' : id === 'cases' ? 'En casos' : 'En chats'}
          </button>
        ))}
        <input
          value={q}
          onChange={(e) => setQ(e.target.value)}
          placeholder="Filtrar por nombre"
          className="h-10 rounded-[12px] border border-border-strong bg-surface px-3 text-[13px]"
        />
      </div>
      {loading ? <TableSkeleton /> : null}
      {chatsQuery.isError ? (
        <QueryError message="No pudimos cargar tus documentos." onRetry={() => chatsQuery.refetch()} />
      ) : null}
      {!loading && rows.length === 0 ? (
        <EmptyState
          title="No hay documentos todavía."
          description="Subí un archivo desde un chat o caso para mantenerlo asociado a su contexto."
          action={{ label: 'Subir documento', onClick: () => setUpload(true) }}
        />
      ) : !loading ? (
        <Table>
          <THead>
            <tr>
              <Th>Documento</Th>
              <Th>Ubicación</Th>
              <Th>Tipo</Th>
              <Th></Th>
            </tr>
          </THead>
          <TBody>
            {rows.map(({ doc, location }) => (
              <Tr key={doc.id}>
                <Td className="font-medium">
                  <Link to={`/app/documents/${doc.id}`} className="hover:underline">
                    {doc.title}
                  </Link>
                </Td>
                <Td className="text-muted">{location}</Td>
                <Td>{fileExtension(doc.title) || '—'}</Td>
                <Td>
                  <Link to={`/app/documents/${doc.id}`} className="text-[13px] text-blue-600 hover:underline">
                    Ver análisis
                  </Link>
                </Td>
              </Tr>
            ))}
          </TBody>
        </Table>
      ) : null}
      <UploadDocumentModal open={upload} onClose={() => setUpload(false)} />
    </AppPage>
  )
}

export function DocumentDetailPage() {
  const { documentId } = useParams<{ documentId: string }>()
  const { canManageCases } = useAuth()
  const queryClient = useQueryClient()
  const run = useAnalysisRun(documentId ?? '')

  const docQuery = useQuery({
    queryKey: queryKeys.document(documentId ?? ''),
    queryFn: () => documentsApi.getById(documentId!),
    enabled: Boolean(documentId),
  })
  const chatsQuery = useQuery({ queryKey: queryKeys.chats, queryFn: chatsApi.list })
  const foldersQuery = useQuery({
    queryKey: queryKeys.folders,
    queryFn: foldersApi.list,
    enabled: canManageCases,
    retry: false,
  })
  const analysisQuery = useQuery({
    queryKey: queryKeys.documentAnalysis(documentId ?? ''),
    queryFn: async () => {
      try {
        return await documentsApi.getAnalysis(documentId!)
      } catch (err) {
        if (err instanceof ApiError && err.status === 404) {
          const saved = readAnalysisRun(documentId!)
          if (saved?.status === 'done' && saved.analysis) return saved.analysis
          return null
        }
        throw err
      }
    },
    enabled: Boolean(documentId),
    staleTime: Infinity,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
  })

  useEffect(() => {
    if (!documentId || !analysisQuery.isSuccess || analysisQuery.data) return
    ensureCompleteAnalysis(documentId, queryClient)
  }, [documentId, analysisQuery.isSuccess, analysisQuery.data, queryClient])

  const doc = docQuery.data
  const chat = chatsQuery.data?.find((c) => c.id === doc?.chatId)
  const folder = foldersQuery.data?.find((f) => f.id === doc?.folderId)
  const href = fileUrl(doc?.url)
  const analysis = analysisQuery.data ?? (run?.status === 'done' ? run.analysis ?? null : null)
  const failed = !analysis && run?.status === 'error'
  const analyzing = !analysis && !failed && (analysisQuery.isSuccess || run?.status === 'queued' || run?.status === 'running')

  return (
    <AppPage>
      <Link to="/app/documents" className="text-[13px] text-blue-600 hover:underline">
        ← Documentos
      </Link>
      {docQuery.isError ? (
        <div className="mt-4">
          <QueryError message="No pudimos cargar el documento." onRetry={() => docQuery.refetch()} />
        </div>
      ) : null}
      <div className="mt-3 mb-6 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-[clamp(1.7rem,2.4vw,2.1rem)] font-semibold tracking-[-0.04em]">{doc?.title ?? 'Documento'}</h1>
          <p className="mt-1 text-[14px] text-muted">
            {folder ? `Caso: ${folder.name}` : chat ? `Chat: ${chat.title}` : 'Sin ubicación'}
          </p>
        </div>
        <div className="flex gap-2">
          {href ? (
            <a href={href} target="_blank" rel="noreferrer" className={buttonClass('secondary')}>
              Abrir archivo
            </a>
          ) : null}
          {doc?.chatId ? (
            <ButtonLink to={`/app/chats/${doc.chatId}`} variant="secondary">
              Abrir conversación
            </ButtonLink>
          ) : null}
        </div>
      </div>

      <section className="rounded-[20px] border border-border bg-surface p-6 shadow-card">
        <h2 className="text-[18px] font-semibold">Análisis con IA</h2>
        <p className="mt-1 text-[13px] text-muted">
          El análisis completo queda guardado y no cambia. Para hacer preguntas sobre el documento, usá el chat donde está vinculado.
        </p>
        {analysisQuery.isLoading && !run ? (
          <p className="mt-6 text-[14px] text-muted">Cargando el análisis guardado…</p>
        ) : null}
        {analyzing ? (
          <p className="mt-6 text-[14px] font-medium text-blue-600">Analizando con IA</p>
        ) : null}
        {failed ? (
          <div className="mt-4">
            <Alert>{run?.error ?? 'No pudimos analizar el documento.'}</Alert>
            <Button className="mt-3" variant="secondary" onClick={() => retryCompleteAnalysis(documentId!, queryClient)}>
              Reintentar análisis
            </Button>
          </div>
        ) : null}
        {analysisQuery.isError ? (
          <div className="mt-4">
            <QueryError message="No pudimos cargar el análisis." onRetry={() => analysisQuery.refetch()} />
          </div>
        ) : null}
        {analysis ? <AnalysisResult analysis={analysis} /> : null}
      </section>
    </AppPage>
  )
}

function AnalysisResult({ analysis }: { analysis: DocumentAnalysisDto }) {
  return (
    <div className="mt-6 space-y-6">
      <p className="text-[12px] font-medium text-success">Análisis completo</p>
      {analysis.summary ? (
        <article>
          <h3 className="text-[16px] font-semibold">Resumen</h3>
          <MarkdownBody className="mt-2" content={analysis.summary} />
        </article>
      ) : null}
      {analysis.risks ? (
        <article>
          <h3 className="text-[16px] font-semibold">Riesgos</h3>
          <MarkdownBody className="mt-2" content={analysis.risks} />
        </article>
      ) : null}
      {analysis.recommendations ? (
        <article>
          <h3 className="text-[16px] font-semibold">Recomendaciones</h3>
          <MarkdownBody className="mt-2" content={analysis.recommendations} />
        </article>
      ) : null}
      {analysis.references ? (
        <article>
          <h3 className="text-[16px] font-semibold">Referencias</h3>
          <MarkdownBody className="mt-2" content={analysis.references} />
        </article>
      ) : null}
      <AiDisclaimer />
    </div>
  )
}
