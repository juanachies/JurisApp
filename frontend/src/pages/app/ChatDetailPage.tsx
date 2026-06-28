import { useState, useRef, useEffect, useMemo, type KeyboardEvent } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useMutation, useQueries, useQuery, useQueryClient } from '@tanstack/react-query'
import { ScanSearch } from 'lucide-react'
import {
  chatsApi,
  documentsApi,
  analysisApi,
  skillsApi,
  aiTasksApi,
  type DocumentAnalysisSegmentDto,
  type SegmentedDocumentAnalysisDto,
  type ApiError,
} from '@/lib/api'
import { DocumentCard } from '@/components/domain/DocumentCard'
import { AITaskPanel } from '@/components/domain/AITaskPanel'
import { SegmentedAnalysisPanel } from '@/components/domain/SegmentedAnalysisPanel'
import { FloatingChat } from '@/components/domain/FloatingChat'
import { SkillChip } from '@/components/domain/SkillChip'
import { Button } from '@/components/ui/Button'
import { Textarea } from '@/components/ui/Textarea'
import { Select } from '@/components/ui/Select'
import { Alert } from '@/components/ui/Alert'
import { ConfirmDialog } from '@/components/ui/Modal'
import { Spinner } from '@/components/ui/Loading'
import { useAuth } from '@/lib/auth/AuthContext'
import {
  getAnalysisMaxSeverity,
  severityToDocumentRiskLevel,
} from '@/lib/utils/severity'

type ChatMode = 'normal' | 'task'

function buildMessageWithSegmentContext(
  userMessage: string,
  segment: DocumentAnalysisSegmentDto,
): string {
  const itemsSummary =
    segment.items.length > 0
      ? `\n\nÍtems detectados en el segmento:\n${segment.items
          .map(
            (item) =>
              `- ${item.title}: ${item.description}${item.recommendation ? ` (Recomendación: ${item.recommendation})` : ''}`,
          )
          .join('\n')}`
      : ''

  return `[Contexto del segmento "${segment.title}"]\n${segment.content}${itemsSummary}\n\n[Pregunta]\n${userMessage}`
}

export function ChatDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { user } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const fileRef = useRef<HTMLInputElement>(null)
  const messagesEndRef = useRef<HTMLDivElement>(null)

  const [message, setMessage] = useState('')
  const [mode, setMode] = useState<ChatMode>('normal')
  const [selectedSkillId, setSelectedSkillId] = useState('')
  const [analysesOverride, setAnalysesOverride] = useState<
    Record<string, SegmentedDocumentAnalysisDto>
  >({})
  const [freeTextAnalysis, setFreeTextAnalysis] =
    useState<SegmentedDocumentAnalysisDto | null>(null)
  const [consultaInput, setConsultaInput] = useState('')
  const [activeAnalysisDocId, setActiveAnalysisDocId] = useState<string | null>(null)
  const [analyzingDocId, setAnalyzingDocId] = useState<string | null>(null)
  const [showDelete, setShowDelete] = useState(false)
  const [chatExpanded, setChatExpanded] = useState(false)
  const [selectedSegment, setSelectedSegment] =
    useState<DocumentAnalysisSegmentDto | null>(null)

  const { data: chat, isLoading } = useQuery({
    queryKey: ['chats', id],
    queryFn: () => chatsApi.getById(id!),
    enabled: !!id,
  })

  const { data: documents } = useQuery({
    queryKey: ['documents', 'chat', id],
    queryFn: () => documentsApi.listByChat(id!),
    enabled: !!id,
  })

  const savedAnalysisQueries = useQueries({
    queries: (documents ?? []).map((doc) => ({
      queryKey: ['analysis', 'document', doc.id],
      queryFn: () => analysisApi.getByDocument(doc.id),
      enabled: !!doc.id,
      staleTime: 60_000,
    })),
  })

  const analysesByDocId = useMemo(() => {
    const map: Record<string, SegmentedDocumentAnalysisDto> = { ...analysesOverride }
    documents?.forEach((doc, index) => {
      const data = savedAnalysisQueries[index]?.data
      if (data) map[doc.id] = data
    })
    return map
  }, [analysesOverride, documents, savedAnalysisQueries])

  const { data: skills } = useQuery({
    queryKey: ['skills', 'me'],
    queryFn: skillsApi.listMine,
    enabled: !!id && (user?.role === 'Lawyer' || user?.role === 'Admin'),
    retry: false,
  })

  const { data: tasks, refetch: refetchTasks } = useQuery({
    queryKey: ['ai-tasks', 'chat', id],
    queryFn: () => aiTasksApi.listByChat(id!),
    enabled: !!id,
    refetchInterval: (query) => {
      const active = query.state.data?.find(
        (t) => t.status === 'InProgress' && !t.isPaused,
      )
      return active ? 3000 : false
    },
  })

  const activeTask = tasks?.find(
    (t) => !['Completed', 'Cancelled', 'Failed'].includes(t.status),
  )

  const { data: taskDetail, refetch: refetchTaskDetail } = useQuery({
    queryKey: ['ai-tasks', activeTask?.id],
    queryFn: () => aiTasksApi.getById(activeTask!.id),
    enabled: !!activeTask?.id,
  })

  const activeAnalysis = useMemo(() => {
    if (activeAnalysisDocId && analysesByDocId[activeAnalysisDocId]) {
      return analysesByDocId[activeAnalysisDocId]
    }
    return freeTextAnalysis
  }, [activeAnalysisDocId, analysesByDocId, freeTextAnalysis])

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [chat?.messages])

  const sendMutation = useMutation({
    mutationFn: (content: string) => chatsApi.sendMessage(id!, { content }),
    onSuccess: () => {
      setMessage('')
      queryClient.invalidateQueries({ queryKey: ['chats', id] })
    },
  })

  const createTaskMutation = useMutation({
    mutationFn: (description: string) => aiTasksApi.create({ chatId: id!, description }),
    onSuccess: () => {
      setMessage('')
      refetchTasks()
      refetchTaskDetail()
      queryClient.invalidateQueries({ queryKey: ['chats', id] })
    },
  })

  const uploadMutation = useMutation({
    mutationFn: (file: File) => documentsApi.upload(file, id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['documents', 'chat', id] })
    },
  })

  const analyzeDocumentMutation = useMutation({
    mutationFn: (documentId: string) => {
      setAnalyzingDocId(documentId)
      return analysisApi.analyzeSegmented({ chatId: id!, documentId })
    },
    onSuccess: (data, documentId) => {
      setAnalysesOverride((prev) => ({ ...prev, [documentId]: data }))
      setActiveAnalysisDocId(documentId)
      setFreeTextAnalysis(null)
      queryClient.setQueryData(['analysis', 'document', documentId], data)
    },
    onSettled: () => setAnalyzingDocId(null),
  })

  const analyzeConsultaMutation = useMutation({
    mutationFn: (input: string) =>
      analysisApi.analyzeSegmented({ chatId: id!, input }),
    onSuccess: (data) => {
      setFreeTextAnalysis(data)
      setActiveAnalysisDocId(null)
    },
  })

  const applySkillMutation = useMutation({
    mutationFn: () =>
      skillsApi.applyToChat({ chatId: id!, customSkillId: selectedSkillId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chats', id] })
      setSelectedSkillId('')
    },
  })

  const removeSkillMutation = useMutation({
    mutationFn: (skillId: string) =>
      skillsApi.removeFromChat({ chatId: id!, customSkillId: skillId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['chats', id] }),
  })

  const deleteMutation = useMutation({
    mutationFn: () => chatsApi.delete(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['chats'] })
      navigate('/app/chats')
    },
  })

  const savePlanMutation = useMutation({
    mutationFn: (steps: { order: number; title: string; description: string }[]) =>
      aiTasksApi.updatePlan(activeTask!.id, { steps }),
    onSuccess: () => refetchTaskDetail(),
  })

  const approveMutation = useMutation({
    mutationFn: () => aiTasksApi.approve(activeTask!.id),
    onSuccess: () => {
      refetchTasks()
      refetchTaskDetail()
      queryClient.invalidateQueries({ queryKey: ['chats', id] })
    },
  })

  const pauseMutation = useMutation({
    mutationFn: () => aiTasksApi.pause(activeTask!.id),
    onSuccess: () => refetchTaskDetail(),
  })

  const resumeMutation = useMutation({
    mutationFn: () => aiTasksApi.resume(activeTask!.id),
    onSuccess: () => {
      refetchTasks()
      refetchTaskDetail()
      queryClient.invalidateQueries({ queryKey: ['chats', id] })
    },
  })

  const cancelTaskMutation = useMutation({
    mutationFn: () => aiTasksApi.cancel(activeTask!.id),
    onSuccess: () => refetchTasks(),
  })

  const handleSend = () => {
    const content = message.trim()
    if (!content) return

    if (mode === 'task') {
      createTaskMutation.mutate(content)
      return
    }

    const payload =
      selectedSegment != null
        ? buildMessageWithSegmentContext(content, selectedSegment)
        : content

    sendMutation.mutate(payload)
  }

  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
      e.preventDefault()
      handleSend()
    }
  }

  const handleAskFromAnalysis = (prompt: string) => {
    setSelectedSegment(null)
    setMessage(prompt)
    setMode('normal')
    setChatExpanded(true)
  }

  const handleAskAboutSegment = (segment: DocumentAnalysisSegmentDto) => {
    setSelectedSegment(segment)
    setMessage('')
    setMode('normal')
    setChatExpanded(true)
  }

  const getDocumentRiskLevel = (docId: string) => {
    const analysis = analysesByDocId[docId]
    if (!analysis) return undefined
    return severityToDocumentRiskLevel(getAnalysisMaxSeverity(analysis))
  }

  const analysisPanel = activeAnalysis ? (
    <SegmentedAnalysisPanel
      analysis={activeAnalysis}
      onAskAbout={handleAskFromAnalysis}
      onAskAboutSegment={handleAskAboutSegment}
    />
  ) : (
    <div className="space-y-4">
      <p className="text-sm text-muted-foreground">
        Analizá un documento adjunto o pegá una consulta/caso para obtener un análisis
        segmentado por categoría jurídica.
      </p>
      <CardConsultaAnalyzer
        value={consultaInput}
        onChange={setConsultaInput}
        onAnalyze={() => {
          const text = consultaInput.trim()
          if (text) analyzeConsultaMutation.mutate(text)
        }}
        isLoading={analyzeConsultaMutation.isPending}
      />
    </div>
  )

  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <Spinner size="lg" />
      </div>
    )
  }

  if (!chat) {
    return <Alert variant="error">Consulta no encontrada.</Alert>
  }

  const sortedMessages = [...(chat.messages ?? [])].sort(
    (a, b) => new Date(a.date).getTime() - new Date(b.date).getTime(),
  )

  return (
    <div className="analysis-workspace mx-auto max-w-[88rem]">
      <header className="analysis-header space-y-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="font-heading text-xl text-foreground">{chat.title}</h2>
            <p className="mt-1 text-sm text-muted-foreground">
              Tablero de análisis documental con copiloto legal integrado.
            </p>
          </div>
        </div>

        {(analyzeDocumentMutation.isError || analyzeConsultaMutation.isError) && (
          <Alert variant="error">
            {(analyzeDocumentMutation.error as ApiError | undefined)?.message ??
              (analyzeConsultaMutation.error as ApiError | undefined)?.message ??
              'No se pudo completar el análisis.'}
          </Alert>
        )}

        <div className="grid gap-4 lg:grid-cols-2">
          <section className="rounded-[16px] border border-border bg-background-alt p-4">
            <h3
              className="mb-3 text-xs font-semibold uppercase tracking-[0.14em] text-accent-secondary"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              Documentos
            </h3>
            {documents?.length ? (
              <div className="space-y-3">
                {documents.map((doc) => (
                  <DocumentCard
                    key={doc.id}
                    document={doc}
                    onAnalyze={() => analyzeDocumentMutation.mutate(doc.id)}
                    isAnalyzing={analyzingDocId === doc.id}
                    riskLevel={getDocumentRiskLevel(doc.id)}
                    isActive={activeAnalysisDocId === doc.id}
                    onSelect={
                      analysesByDocId[doc.id]
                        ? () => {
                            setActiveAnalysisDocId(doc.id)
                            setFreeTextAnalysis(null)
                          }
                        : undefined
                    }
                  />
                ))}
              </div>
            ) : (
              <p className="text-xs text-muted-foreground">Sin documentos adjuntos</p>
            )}
          </section>

          <section className="rounded-[16px] border border-border bg-background-alt p-4">
            <h3
              className="mb-3 text-xs font-semibold uppercase tracking-[0.14em] text-accent-secondary"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              Skills aplicadas
            </h3>
            {chat.appliedSkills?.length ? (
              <div className="mb-3 flex flex-wrap gap-2">
                {chat.appliedSkills.map((s) => (
                  <SkillChip
                    key={s.id}
                    name={s.name}
                    onRemove={() => removeSkillMutation.mutate(s.id)}
                  />
                ))}
              </div>
            ) : (
              <p className="mb-3 text-xs text-muted-foreground">Sin skills aplicadas</p>
            )}
            {skills && skills.length > 0 && (
              <div className="flex gap-2">
                <Select
                  value={selectedSkillId}
                  onChange={(e) => setSelectedSkillId(e.target.value)}
                  className="flex-1"
                >
                  <option value="">Seleccionar skill</option>
                  {skills.map((s) => (
                    <option key={s.id} value={s.id}>
                      {s.name}
                    </option>
                  ))}
                </Select>
                <Button
                  size="sm"
                  variant="secondary"
                  onClick={() => applySkillMutation.mutate()}
                  disabled={!selectedSkillId}
                  isLoading={applySkillMutation.isPending}
                >
                  Aplicar
                </Button>
              </div>
            )}
          </section>
        </div>

        {taskDetail && (
          <AITaskPanel
            task={taskDetail}
            onSavePlan={(steps) => savePlanMutation.mutate(steps)}
            onApprove={() => approveMutation.mutate()}
            onPause={() => pauseMutation.mutate()}
            onResume={() => resumeMutation.mutate()}
            onCancel={() => cancelTaskMutation.mutate()}
            isLoading={
              savePlanMutation.isPending ||
              approveMutation.isPending ||
              pauseMutation.isPending ||
              resumeMutation.isPending ||
              cancelTaskMutation.isPending
            }
          />
        )}
      </header>

      <section className="segments-dashboard">{analysisPanel}</section>

      <FloatingChat
        messages={sortedMessages}
        message={message}
        onMessageChange={setMessage}
        onSend={handleSend}
        onKeyDown={handleKeyDown}
        mode={mode}
        onModeChange={setMode}
        selectedSegment={selectedSegment}
        onClearSelectedSegment={() => setSelectedSegment(null)}
        isSending={sendMutation.isPending || createTaskMutation.isPending}
        isExpanded={chatExpanded}
        onExpandedChange={setChatExpanded}
        fileInputRef={fileRef}
        onAttachClick={() => fileRef.current?.click()}
        onFileChange={(e) => {
          const file = e.target.files?.[0]
          if (file) uploadMutation.mutate(file)
          e.target.value = ''
        }}
        isUploading={uploadMutation.isPending}
        onDeleteClick={() => setShowDelete(true)}
        messagesEndRef={messagesEndRef}
      />

      <ConfirmDialog
        isOpen={showDelete}
        onClose={() => setShowDelete(false)}
        onConfirm={() => deleteMutation.mutate()}
        title="Eliminar consulta"
        message="¿Estás seguro de que querés eliminar esta consulta? Esta acción no se puede deshacer."
        confirmLabel="Eliminar"
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  )
}

function CardConsultaAnalyzer({
  value,
  onChange,
  onAnalyze,
  isLoading,
}: {
  value: string
  onChange: (value: string) => void
  onAnalyze: () => void
  isLoading?: boolean
}) {
  return (
    <div className="rounded-[16px] border border-border bg-background-alt p-4 space-y-3">
      <h4
        className="text-xs font-semibold uppercase tracking-[0.14em] text-accent-secondary"
        style={{ fontFamily: 'var(--font-display)' }}
      >
        Consulta o caso sin documento
      </h4>
      <Textarea
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder="Pegá el texto del caso, una consulta jurídica o una pregunta..."
        rows={5}
      />
      <Button
        size="sm"
        onClick={onAnalyze}
        disabled={!value.trim()}
        isLoading={isLoading}
      >
        <ScanSearch className="h-3.5 w-3.5" aria-hidden="true" />
        Analizar consulta
      </Button>
    </div>
  )
}
