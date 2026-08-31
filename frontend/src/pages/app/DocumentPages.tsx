import { useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useMutation, useQueries, useQuery, useQueryClient } from '@tanstack/react-query'
import { chatsApi, documentsApi, foldersApi, skillsApi } from '@/api'
import { fileUrl } from '@/api/client'
import { queryKeys } from '@/api/queryKeys'
import { errorMessage } from '@/api/client'
import { useAuth } from '@/app/AuthContext'
import { AppPage, QueryError } from '@/components/layout/AppShell'
import { UploadDocumentModal } from '@/components/documents/UploadDocumentModal'
import { Alert } from '@/components/ui/Alert'
import { Button, ButtonLink, buttonClass } from '@/components/ui/Button'
import { EmptyState } from '@/components/ui/EmptyState'
import { MarkdownBody, AiDisclaimer } from '@/components/ui/MarkdownBody'
import { PageHeader } from '@/components/ui/PageHeader'
import { Table, TBody, Td, Th, THead, Tr } from '@/components/ui/Table'
import { TableSkeleton } from '@/components/ui/Loading'
import { useToast } from '@/components/ui/Toast'
import type { DocumentAnalysisDto, DocumentAnalysisType, DocumentDto } from '@/types/api'
import { analysisTypeLabel, fileExtension } from '@/utils/format'

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
            className={`rounded-[8px] px-3 py-1.5 text-[13px] ${filter === id ? 'bg-navy-900 text-white' : 'bg-surface text-muted'}`}
            onClick={() => setFilter(id)}
          >
            {id === 'all' ? 'Todos' : id === 'cases' ? 'En casos' : 'En chats'}
          </button>
        ))}
        <input
          value={q}
          onChange={(e) => setQ(e.target.value)}
          placeholder="Filtrar por nombre"
          className="h-9 rounded-[8px] border border-border-strong bg-surface px-3 text-[13px]"
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
                    Analizar
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

const ANALYSIS_ACTIONS: { label: string; types?: DocumentAnalysisType[] }[] = [
  { label: 'Resumir', types: ['Summary'] },
  { label: 'Detectar riesgos', types: ['RiskAnalysis'] },
  { label: 'Generar recomendaciones', types: ['Recommendations'] },
  { label: 'Revisión contractual', types: ['ContractReview'] },
  { label: 'Análisis completo' },
]

export function DocumentDetailPage() {
  const { documentId } = useParams<{ documentId: string }>()
  const { canManageCases } = useAuth()
  const queryClient = useQueryClient()
  const toast = useToast()
  const [error, setError] = useState<string | null>(null)
  const [skillId, setSkillId] = useState('')

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
  const skillsQuery = useQuery({
    queryKey: queryKeys.skills,
    queryFn: skillsApi.list,
    enabled: canManageCases,
    retry: false,
  })
  const analysisQuery = useQuery({
    queryKey: queryKeys.documentAnalysis(documentId ?? ''),
    queryFn: async () => null as DocumentAnalysisDto | null,
    enabled: false,
    staleTime: Infinity,
  })

  const analyze = useMutation({
    mutationFn: (types?: DocumentAnalysisType[]) =>
      documentsApi.analyze({
        documentId: documentId!,
        types,
        customSkillIds: skillId ? [skillId] : undefined,
      }),
    onSuccess: (result) => {
      queryClient.setQueryData(queryKeys.documentAnalysis(documentId!), result)
      toast('Análisis listo.')
      setError(null)
    },
    onError: (err) => setError(errorMessage(err, 'No pudimos analizar el documento.')),
  })

  const doc = docQuery.data
  const chat = chatsQuery.data?.find((c) => c.id === doc?.chatId)
  const folder = foldersQuery.data?.find((f) => f.id === doc?.folderId)
  const href = fileUrl(doc?.url)

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
          <h1 className="text-[28px] font-semibold">{doc?.title ?? 'Documento'}</h1>
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

      <section className="rounded-[12px] border border-border bg-surface p-5">
        <h2 className="text-[18px] font-semibold">Análisis con IA</h2>
        <p className="mt-1 text-[13px] text-muted">
          El análisis no se vuelve a listar al recargar: queda visible en esta sesión después de pedirlo.
        </p>
        {canManageCases && (skillsQuery.data ?? []).some((s) => s.isActive) ? (
          <select
            className="mt-4 h-10 rounded-[8px] border border-border-strong bg-canvas px-3 text-[14px]"
            value={skillId}
            onChange={(e) => setSkillId(e.target.value)}
          >
            <option value="">Skill: ninguna</option>
            {(skillsQuery.data ?? [])
              .filter((s) => s.isActive)
              .map((skill) => (
                <option key={skill.id} value={skill.id}>
                  {skill.name}
                </option>
              ))}
          </select>
        ) : null}
        <div className="mt-4 flex flex-wrap gap-2">
          {ANALYSIS_ACTIONS.map((action) => (
            <Button
              key={action.label}
              variant="secondary"
              loading={analyze.isPending}
              onClick={() => analyze.mutate(action.types)}
            >
              {action.label}
            </Button>
          ))}
        </div>
        {analyze.isPending ? (
          <p className="mt-6 text-[14px] text-muted">
            Analizando documento… JurisApp está procesando el contenido para generar el análisis solicitado.
          </p>
        ) : null}
        {error ? <Alert className="mt-4">{error}</Alert> : null}
        {analysisQuery.data ? <AnalysisResult analysis={analysisQuery.data} /> : null}
      </section>
    </AppPage>
  )
}

function AnalysisResult({ analysis }: { analysis: DocumentAnalysisDto }) {
  return (
    <div className="mt-6 space-y-6">
      <p className="text-[12px] text-faint">Tipo: {analysisTypeLabel(analysis.type)}</p>
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
