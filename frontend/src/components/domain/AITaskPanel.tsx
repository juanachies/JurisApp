import { useState } from 'react'
import { Bot, CheckCircle, PlayCircle, PauseCircle } from 'lucide-react'
import type { AITaskDetailDto, UpdateAITaskPlanStep } from '@/lib/api'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { StatusBadge } from '@/components/ui/Badge'
import { Input } from '@/components/ui/Input'
import { Textarea } from '@/components/ui/Textarea'
import { LegalDisclaimer } from '@/components/ui/LegalDisclaimer'
import { cn } from '@/lib/utils/cn'

interface AITaskPanelProps {
  task: AITaskDetailDto
  onSavePlan: (steps: UpdateAITaskPlanStep[]) => void
  onApprove: () => void
  onPause: () => void
  onResume: () => void
  onCancel: () => void
  isLoading?: boolean
}

export function AITaskPanel({
  task,
  onSavePlan,
  onApprove,
  onPause,
  onResume,
  onCancel,
  isLoading,
}: AITaskPanelProps) {
  const [editedSteps, setEditedSteps] = useState<UpdateAITaskPlanStep[]>(
    task.steps.map((s) => ({
      order: s.order,
      title: s.title,
      description: s.description,
    })),
  )

  const canEdit = task.status === 'AwaitingApproval'
  const isRunning = task.status === 'InProgress' && !task.isPaused
  const showPause = task.status === 'InProgress' && !task.isPaused
  const showResume = task.status === 'InProgress' && task.isPaused

  const updateStep = (order: number, field: 'title' | 'description', value: string) => {
    setEditedSteps((prev) =>
      prev.map((s) => (s.order === order ? { ...s, [field]: value } : s)),
    )
  }

  return (
    <Card
      className={cn('space-y-4', isRunning && 'ai-processing ai-pulse')}
    >
      <div className="flex items-center gap-2">
        <Bot className="h-5 w-5 text-ai" aria-hidden="true" />
        <h3 className="text-sm font-semibold text-foreground">Tarea IA</h3>
        <StatusBadge status={task.status} />
        {task.isPaused && <StatusBadge status="Pending" label="Pausada" />}
      </div>

      {task.plan && (
        <p className="text-sm text-muted-foreground whitespace-pre-wrap">{task.plan}</p>
      )}

      <div className="space-y-3">
        {task.steps.map((step) => {
          const isCurrent =
            task.status === 'InProgress' && step.order === task.currentStepIndex
          const edited = editedSteps.find((s) => s.order === step.order)

          return (
            <div
              key={step.id}
              className={cn(
                'rounded-[10px] border border-border bg-background-alt p-3',
                step.status === 'Completed' && 'border-success/30 bg-success/5',
                isCurrent && 'border-ai/40 shadow-sm',
              )}
            >
              {canEdit && edited ? (
                <div className="space-y-2">
                  <Input
                    label={`Paso ${step.order} — título`}
                    value={edited.title}
                    onChange={(e) => updateStep(step.order, 'title', e.target.value)}
                  />
                  <Textarea
                    label="Descripción"
                    value={edited.description}
                    onChange={(e) => updateStep(step.order, 'description', e.target.value)}
                    rows={2}
                  />
                </div>
              ) : (
                <>
                  <div className="flex items-center gap-2">
                    {step.status === 'Completed' && (
                      <CheckCircle className="h-4 w-4 text-success" aria-hidden="true" />
                    )}
                    <span className="text-sm font-semibold text-foreground">
                      Paso {step.order}: {step.title}
                    </span>
                    <StatusBadge status={step.status} />
                  </div>
                  <p className="mt-1 text-xs text-muted-foreground">{step.description}</p>
                  {step.result && (
                    <p className="mt-2 text-xs text-foreground whitespace-pre-wrap border-t border-border pt-2">
                      {step.result}
                    </p>
                  )}
                </>
              )}
            </div>
          )
        })}
      </div>

      <div className="flex flex-wrap gap-2">
        {canEdit && (
          <>
            <Button
              variant="secondary"
              size="sm"
              onClick={() => onSavePlan(editedSteps)}
              isLoading={isLoading}
            >
              Guardar plan
            </Button>
            <Button size="sm" onClick={onApprove} isLoading={isLoading}>
              <PlayCircle className="h-4 w-4" aria-hidden="true" />
              Aprobar y ejecutar
            </Button>
          </>
        )}
        {showPause && (
          <Button variant="secondary" size="sm" onClick={onPause} isLoading={isLoading}>
            <PauseCircle className="h-4 w-4" aria-hidden="true" />
            Pausar
          </Button>
        )}
        {showResume && (
          <Button size="sm" onClick={onResume} isLoading={isLoading}>
            <PlayCircle className="h-4 w-4" aria-hidden="true" />
            Reanudar
          </Button>
        )}
        {task.status !== 'Completed' && task.status !== 'Cancelled' && (
          <Button variant="ghost" size="sm" onClick={onCancel} disabled={isLoading}>
            Cancelar tarea
          </Button>
        )}
      </div>

      <LegalDisclaimer />
    </Card>
  )
}
