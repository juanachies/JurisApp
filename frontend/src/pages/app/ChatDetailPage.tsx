import { useState, useRef, useEffect, type KeyboardEvent } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Paperclip, Send, Trash2, Sparkles } from 'lucide-react'
import {
  chatsApi,
  documentsApi,
  skillsApi,
  aiTasksApi,
  type DocumentAnalysisDto,
} from '@/lib/api'
import { ChatMessage } from '@/components/domain/ChatMessage'
import { DocumentCard } from '@/components/domain/DocumentCard'
import { AITaskPanel } from '@/components/domain/AITaskPanel'
import { RiskSummaryCard } from '@/components/domain/RiskSummaryCard'
import { SkillChip } from '@/components/domain/SkillChip'
import { Button } from '@/components/ui/Button'
import { Textarea } from '@/components/ui/Textarea'
import { Select } from '@/components/ui/Select'
import { Alert } from '@/components/ui/Alert'
import { Tabs, TabPanel } from '@/components/ui/Tabs'
import { ConfirmDialog } from '@/components/ui/Modal'
import { Spinner } from '@/components/ui/Loading'
import { LegalDisclaimer } from '@/components/ui/LegalDisclaimer'
import { useAuth } from '@/lib/auth/AuthContext'

type ChatMode = 'normal' | 'task'

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
  const [analysis, setAnalysis] = useState<DocumentAnalysisDto | null>(null)
  const [analyzingDocId, setAnalyzingDocId] = useState<string | null>(null)
  const [showDelete, setShowDelete] = useState(false)
  const [mobileTab, setMobileTab] = useState('chat')

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

  const analyzeMutation = useMutation({
    mutationFn: (documentId: string) => {
      setAnalyzingDocId(documentId)
      return documentsApi.analyze({ documentId, type: 'Summary' })
    },
    onSuccess: (data) => {
      setAnalysis(data)
      setMobileTab('insights')
    },
    onSettled: () => setAnalyzingDocId(null),
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
    } else {
      sendMutation.mutate(content)
    }
  }

  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && (e.ctrlKey || e.metaKey)) {
      e.preventDefault()
      handleSend()
    }
  }

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

  const contextPanel = (
    <div className="space-y-6">
      <section>
        <h3
          className="mb-3 text-xs font-semibold uppercase tracking-[0.14em] text-accent-secondary"
          style={{ fontFamily: 'var(--font-display)' }}
        >
          Skills aplicadas
        </h3>
        {chat.appliedSkills?.length ? (
          <div className="flex flex-wrap gap-2">
            {chat.appliedSkills.map((s) => (
              <SkillChip
                key={s.id}
                name={s.name}
                onRemove={() => removeSkillMutation.mutate(s.id)}
              />
            ))}
          </div>
        ) : (
          <p className="text-xs text-muted-foreground">Sin skills aplicadas</p>
        )}
        {skills && skills.length > 0 && (
          <div className="mt-3 flex gap-2">
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

      <section>
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
                onAnalyze={() => analyzeMutation.mutate(doc.id)}
                isAnalyzing={analyzingDocId === doc.id}
              />
            ))}
          </div>
        ) : (
          <p className="text-xs text-muted-foreground">Sin documentos adjuntos</p>
        )}
      </section>

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

      {analysis && <RiskSummaryCard analysis={analysis} />}
    </div>
  )

  const chatThread = (
    <div className="flex flex-col h-full">
      <div className="flex-1 overflow-y-auto chat-scrollbar space-y-4 min-h-[300px] max-h-[calc(100vh-320px)] pr-2">
        {sortedMessages.length === 0 ? (
          <p className="text-center text-sm text-muted-foreground py-8">
            Sin mensajes. Escribí tu consulta legal.
          </p>
        ) : (
          sortedMessages.map((msg) => <ChatMessage key={msg.id} message={msg} />)
        )}
        <div ref={messagesEndRef} />
      </div>

      <div className="mt-4 border-t border-border pt-4">
        <div className="mb-3 flex flex-wrap items-center gap-4">
          <label className="flex items-center gap-2 text-sm">
            <input
              type="radio"
              name="chatMode"
              checked={mode === 'normal'}
              onChange={() => setMode('normal')}
            />
            Modo normal
          </label>
          <label className="flex items-center gap-2 text-sm">
            <input
              type="radio"
              name="chatMode"
              checked={mode === 'task'}
              onChange={() => setMode('task')}
            />
            <Sparkles className="h-3.5 w-3.5 text-ai" aria-hidden="true" />
            Modo Tarea IA
          </label>
        </div>

        <Textarea
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder={
            mode === 'task'
              ? 'Describí el encargo legal completo...'
              : 'Escribí tu consulta legal aquí...'
          }
          rows={4}
        />

        <p className="mt-1 text-xs text-muted-foreground">
          {mode === 'task'
            ? 'Genera un plan de trabajo paso a paso para tu encargo.'
            : 'Ctrl+Enter para enviar. '}
          <LegalDisclaimer />
        </p>

        <div className="mt-3 flex flex-wrap items-center gap-2">
          <input
            ref={fileRef}
            type="file"
            accept=".pdf,.doc,.docx,.txt,.rtf,.odt"
            className="sr-only"
            onChange={(e) => {
              const file = e.target.files?.[0]
              if (file) uploadMutation.mutate(file)
              e.target.value = ''
            }}
          />
          <Button
            variant="secondary"
            size="sm"
            onClick={() => fileRef.current?.click()}
            isLoading={uploadMutation.isPending}
          >
            <Paperclip className="h-4 w-4" aria-hidden="true" />
            Adjuntar
          </Button>
          <Button
            size="sm"
            onClick={handleSend}
            isLoading={sendMutation.isPending || createTaskMutation.isPending}
          >
            <Send className="h-4 w-4" aria-hidden="true" />
            {mode === 'task' ? 'Generar plan' : 'Enviar'}
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setShowDelete(true)}
          >
            <Trash2 className="h-4 w-4" aria-hidden="true" />
            Eliminar
          </Button>
        </div>
      </div>
    </div>
  )

  return (
    <div className="mx-auto max-w-7xl">
      <div className="mb-4">
        <h2 className="font-heading text-xl text-foreground">{chat.title}</h2>
      </div>

      {/* Desktop layout */}
      <div className="hidden lg:grid lg:grid-cols-[1fr_340px] gap-6">
        {chatThread}
        <aside className="space-y-4">{contextPanel}</aside>
      </div>

      {/* Mobile layout */}
      <div className="lg:hidden">
        <Tabs
          tabs={[
            { id: 'chat', label: 'Chat' },
            { id: 'docs', label: 'Documentos' },
            { id: 'insights', label: 'Análisis' },
            { id: 'task', label: 'Tarea IA' },
          ]}
          activeTab={mobileTab}
          onChange={setMobileTab}
        />
        <TabPanel id="chat" activeTab={mobileTab}>
          {chatThread}
        </TabPanel>
        <TabPanel id="docs" activeTab={mobileTab}>
          {contextPanel}
        </TabPanel>
        <TabPanel id="insights" activeTab={mobileTab}>
          {analysis ? <RiskSummaryCard analysis={analysis} /> : (
            <p className="text-sm text-muted-foreground">Sin análisis todavía.</p>
          )}
        </TabPanel>
        <TabPanel id="task" activeTab={mobileTab}>
          {taskDetail ? (
            <AITaskPanel
              task={taskDetail}
              onSavePlan={(steps) => savePlanMutation.mutate(steps)}
              onApprove={() => approveMutation.mutate()}
              onPause={() => pauseMutation.mutate()}
              onResume={() => resumeMutation.mutate()}
              onCancel={() => cancelTaskMutation.mutate()}
              isLoading={approveMutation.isPending}
            />
          ) : (
            <p className="text-sm text-muted-foreground">Sin tarea IA activa.</p>
          )}
        </TabPanel>
      </div>

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
