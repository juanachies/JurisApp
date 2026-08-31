import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { tasksApi } from '@/api'
import { queryKeys } from '@/api/queryKeys'
import { errorMessage } from '@/api/client'
import { Alert } from '@/components/ui/Alert'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { ConfirmDialog } from '@/components/ui/Modal'
import { MarkdownBody } from '@/components/ui/MarkdownBody'
import { Textarea } from '@/components/ui/Textarea'
import { AiDisclaimer } from '@/components/ui/MarkdownBody'
import type { AITaskDto, AITaskStepStatus } from '@/types/api'
import { stepStatusLabel, taskStatusLabel } from '@/utils/format'
import { cn } from '@/utils/cn'

function stepMark(status: AITaskStepStatus) {
  switch (status) {
    case 'Completed':
      return '✓'
    case 'InProgress':
      return '●'
    case 'Failed':
      return '!'
    case 'Skipped':
      return '–'
    default:
      return '○'
  }
}

function statusTone(task: AITaskDto): 'info' | 'success' | 'warning' | 'danger' | 'neutral' {
  if (task.isPaused) return 'warning'
  switch (task.status) {
    case 'Completed':
      return 'success'
    case 'Failed':
      return 'danger'
    case 'Cancelled':
      return 'neutral'
    case 'InProgress':
      return 'info'
    case 'AwaitingApproval':
      return 'warning'
    default:
      return 'neutral'
  }
}

export function TaskPlanCard({ task }: { task: AITaskDto }) {
  const queryClient = useQueryClient()
  const [cancelOpen, setCancelOpen] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [editing, setEditing] = useState(false)
  const [drafts, setDrafts] = useState(
    task.steps.map((s) => ({ order: s.order, title: s.title, description: s.description })),
  )

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: queryKeys.task(task.id) })
    queryClient.invalidateQueries({ queryKey: queryKeys.chatTasks(task.chatId) })
    queryClient.invalidateQueries({ queryKey: queryKeys.chat(task.chatId) })
  }

  const approve = useMutation({
    mutationFn: () => tasksApi.approve(task.id),
    onSuccess: invalidate,
    onError: (err) => setError(errorMessage(err)),
  })
  const pause = useMutation({
    mutationFn: () => tasksApi.pause(task.id),
    onSuccess: invalidate,
    onError: (err) => setError(errorMessage(err)),
  })
  const resume = useMutation({
    mutationFn: () => tasksApi.resume(task.id),
    onSuccess: invalidate,
    onError: (err) => setError(errorMessage(err)),
  })
  const cancel = useMutation({
    mutationFn: () => tasksApi.cancel(task.id),
    onSuccess: () => {
      setCancelOpen(false)
      invalidate()
    },
    onError: (err) => setError(errorMessage(err)),
  })
  const savePlan = useMutation({
    mutationFn: () => tasksApi.updatePlan(task.id, { steps: drafts }),
    onSuccess: () => {
      setEditing(false)
      invalidate()
    },
    onError: (err) => setError(errorMessage(err)),
  })

  const awaiting = task.status === 'AwaitingApproval'
  const running = task.status === 'InProgress'
  const canCancel = task.status !== 'Completed' && task.status !== 'Cancelled'

  return (
    <div className="rounded-[12px] border border-border bg-surface p-4">
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-[12px] font-medium uppercase tracking-wide text-faint">Tarea con IA</p>
          <h3 className="mt-1 text-[15px] font-semibold">{task.description}</h3>
        </div>
        <Badge tone={statusTone(task)}>
          {running && !task.isPaused ? (
            <span className="mr-1.5 inline-block size-1.5 animate-pulse rounded-full bg-blue-600" />
          ) : null}
          {taskStatusLabel(task.status, task.isPaused)}
        </Badge>
      </div>

      {error ? <Alert className="mt-3">{error}</Alert> : null}

      {awaiting ? <p className="mt-3 text-[13px] text-muted">Plan propuesto. Revisalo antes de ejecutarlo.</p> : null}

      <ol className="mt-4 space-y-2">
        {task.steps.map((step, index) => (
          <li key={step.id || step.order} className="flex gap-3 text-[14px]">
            <span
              className={cn(
                'mt-0.5 w-4 shrink-0 text-center text-[13px]',
                step.status === 'Completed' && 'text-success',
                step.status === 'InProgress' && 'text-blue-600',
                step.status === 'Failed' && 'text-danger',
              )}
            >
              {stepMark(step.status)}
            </span>
            <div className="min-w-0 flex-1">
              {editing ? (
                <div className="space-y-2">
                  <input
                    className="h-9 w-full rounded-[8px] border border-border-strong px-2 text-[14px]"
                    value={drafts[index]?.title ?? ''}
                    onChange={(e) =>
                      setDrafts((prev) => prev.map((s, i) => (i === index ? { ...s, title: e.target.value } : s)))
                    }
                  />
                  <Textarea
                    className="min-h-16"
                    value={drafts[index]?.description ?? ''}
                    onChange={(e) =>
                      setDrafts((prev) =>
                        prev.map((s, i) => (i === index ? { ...s, description: e.target.value } : s)),
                      )
                    }
                  />
                </div>
              ) : (
                <>
                  <p className="font-medium">
                    {index + 1}. {step.title}
                  </p>
                  {step.description ? <p className="text-[13px] text-muted">{step.description}</p> : null}
                  {step.result ? (
                    <div className="mt-2 rounded-[8px] bg-subtle px-3 py-2">
                      <p className="text-[12px] text-faint">{stepStatusLabel(step.status)}</p>
                      <MarkdownBody content={step.result} className="text-[13px]" />
                    </div>
                  ) : null}
                </>
              )}
            </div>
          </li>
        ))}
      </ol>

      {task.result && task.status === 'Completed' ? (
        <div className="mt-4 border-t border-border pt-3">
          <p className="text-[12px] font-medium uppercase tracking-wide text-faint">Resultado</p>
          <MarkdownBody content={task.result} className="mt-2" />
          <AiDisclaimer className="mt-3" />
        </div>
      ) : null}

      <div className="mt-4 flex flex-wrap gap-2">
        {awaiting ? (
          <>
            <Button loading={approve.isPending} onClick={() => approve.mutate()}>
              Ejecutar plan
            </Button>
            {editing ? (
              <Button variant="secondary" loading={savePlan.isPending} onClick={() => savePlan.mutate()}>
                Guardar cambios
              </Button>
            ) : (
              <Button
                variant="secondary"
                onClick={() => {
                  setDrafts(task.steps.map((s) => ({ order: s.order, title: s.title, description: s.description })))
                  setEditing(true)
                }}
              >
                Editar plan
              </Button>
            )}
          </>
        ) : null}
        {running && !task.isPaused ? (
          <Button variant="secondary" loading={pause.isPending} onClick={() => pause.mutate()}>
            Pausar
          </Button>
        ) : null}
        {running && task.isPaused ? (
          <Button loading={resume.isPending} onClick={() => resume.mutate()}>
            Reanudar
          </Button>
        ) : null}
        {canCancel ? (
          <Button variant="ghost" onClick={() => setCancelOpen(true)}>
            Cancelar tarea
          </Button>
        ) : null}
      </div>

      <ConfirmDialog
        open={cancelOpen}
        title="Cancelar tarea"
        description="La ejecución se detendrá y los pasos pendientes no continuarán."
        confirmLabel="Cancelar tarea"
        cancelLabel="Volver"
        danger
        loading={cancel.isPending}
        onConfirm={() => cancel.mutate()}
        onClose={() => setCancelOpen(false)}
      />
    </div>
  )
}
