import { useMemo, useState, useEffect, type DragEvent } from 'react'
import { GripVertical, MessageCircle, Sparkles } from 'lucide-react'
import type {
  DocumentAnalysisSegmentDto,
  SegmentedDocumentAnalysisDto,
  SuggestedActionDto,
} from '@/lib/api'
import {
  formatConfidence,
  formatFieldLabel,
  formatMainFieldValue,
  getAnalysisMaxSeverity,
  severityLabels,
  severityToBadgeVariant,
  severityToBorderClass,
} from '@/lib/utils/severity'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { LegalDisclaimer } from '@/components/ui/LegalDisclaimer'

interface SegmentedAnalysisPanelProps {
  analysis: SegmentedDocumentAnalysisDto
  onAskAbout?: (prompt: string) => void
  onAskAboutSegment?: (segment: DocumentAnalysisSegmentDto) => void
}

function reorderSegments(
  segments: DocumentAnalysisSegmentDto[],
  order: string[],
): DocumentAnalysisSegmentDto[] {
  const byKey = new Map(segments.map((s) => [s.key, s]))
  const seen = new Set<string>()
  const ordered: DocumentAnalysisSegmentDto[] = []

  for (const key of order) {
    const segment = byKey.get(key)
    if (segment) {
      ordered.push(segment)
      seen.add(key)
    }
  }

  for (const segment of segments) {
    if (!seen.has(segment.key)) ordered.push(segment)
  }

  return ordered
}

export function SegmentedAnalysisPanel({
  analysis,
  onAskAbout,
  onAskAboutSegment,
}: SegmentedAnalysisPanelProps) {
  const [segmentOrder, setSegmentOrder] = useState(() =>
    analysis.segments.map((s) => s.key),
  )
  const [draggingKey, setDraggingKey] = useState<string | null>(null)

  useEffect(() => {
    setSegmentOrder(analysis.segments.map((s) => s.key))
  }, [analysis.documentId, analysis.categoryKey, analysis.segments])

  const orderedSegments = useMemo(
    () => reorderSegments(analysis.segments, segmentOrder),
    [analysis.segments, segmentOrder],
  )

  const globalSeverity = getAnalysisMaxSeverity(analysis)
  const mainFieldEntries = Object.entries(analysis.mainFields ?? {})

  const handleDragStart = (key: string) => (e: DragEvent<HTMLDivElement>) => {
    setDraggingKey(key)
    e.dataTransfer.effectAllowed = 'move'
  }

  const handleDragOver = (key: string) => (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault()
    if (!draggingKey || draggingKey === key) return

    setSegmentOrder((prev) => {
      const next = [...prev]
      const from = next.indexOf(draggingKey)
      const to = next.indexOf(key)
      if (from < 0 || to < 0) return prev
      next.splice(from, 1)
      next.splice(to, 0, draggingKey)
      return next
    })
  }

  const handleDragEnd = () => setDraggingKey(null)

  return (
    <div className="segments-dashboard space-y-5">
      <Card highlight className="space-y-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <div className="flex items-center gap-2">
              <Sparkles className="h-4 w-4 text-ai" aria-hidden="true" />
              <h3 className="font-heading text-base text-foreground">{analysis.displayName}</h3>
            </div>
            <p className="mt-1 text-xs text-muted-foreground">
              Categoría: {analysis.categoryKey.replace(/_/g, ' ')}
            </p>
          </div>
          <div className="flex flex-wrap items-center gap-2">
            <Badge variant="info">{formatConfidence(analysis.confidence)} confianza</Badge>
            <Badge variant={severityToBadgeVariant(globalSeverity)}>
              {severityLabels[globalSeverity]}
            </Badge>
          </div>
        </div>

        {mainFieldEntries.length > 0 && (
          <section>
            <h4
              className="mb-3 text-xs font-semibold uppercase tracking-[0.14em] text-accent-secondary"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              Ficha del caso
            </h4>
            <dl className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              {mainFieldEntries.map(([key, value]) => (
                <div key={key} className="rounded-[10px] border border-border bg-background px-3 py-2">
                  <dt className="text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">
                    {formatFieldLabel(key)}
                  </dt>
                  <dd className="mt-1 text-sm text-foreground">{formatMainFieldValue(value)}</dd>
                </div>
              ))}
            </dl>
          </section>
        )}

        {onAskAbout && (
          <Button
            variant="secondary"
            size="sm"
            onClick={() =>
              onAskAbout(
                `Explicame el análisis completo del ${analysis.displayName.toLowerCase()} y qué debería priorizar.`,
              )
            }
          >
            <MessageCircle className="h-3.5 w-3.5" aria-hidden="true" />
            Preguntar sobre el análisis
          </Button>
        )}
      </Card>

      <div className="segments-dashboard-grid">
        {orderedSegments.map((segment) => (
          <AnalysisSegmentCard
            key={segment.key}
            segment={segment}
            draggable
            isDragging={draggingKey === segment.key}
            onDragStart={handleDragStart(segment.key)}
            onDragOver={handleDragOver(segment.key)}
            onDragEnd={handleDragEnd}
            onAsk={
              onAskAboutSegment
                ? () => onAskAboutSegment(segment)
                : onAskAbout
                  ? () =>
                      onAskAbout(
                        `Explicame el segmento "${segment.title}" del análisis (${segment.key}).`,
                      )
                  : undefined
            }
          />
        ))}
      </div>

      {analysis.suggestedActions.length > 0 && (
        <section className="mt-2">
          <h4
            className="mb-3 text-xs font-semibold uppercase tracking-[0.14em] text-accent-secondary"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            Acciones sugeridas
          </h4>
          <div className="flex flex-wrap gap-2">
            {analysis.suggestedActions.map((action) => (
              <SuggestedActionChip
                key={action.key}
                action={action}
                onSelect={
                  onAskAbout
                    ? () =>
                        onAskAbout(
                          `Ayudame con la acción sugerida: "${action.title}".`,
                        )
                    : undefined
                }
              />
            ))}
          </div>
        </section>
      )}

      <LegalDisclaimer />
    </div>
  )
}

interface AnalysisSegmentCardProps {
  segment: DocumentAnalysisSegmentDto
  draggable?: boolean
  isDragging?: boolean
  onDragStart?: (e: DragEvent<HTMLDivElement>) => void
  onDragOver?: (e: DragEvent<HTMLDivElement>) => void
  onDragEnd?: () => void
  onAsk?: () => void
}

function AnalysisSegmentCard({
  segment,
  draggable,
  isDragging,
  onDragStart,
  onDragOver,
  onDragEnd,
  onAsk,
}: AnalysisSegmentCardProps) {
  const count = segment.countable
    ? (segment.itemsCount ?? segment.items.length)
    : null

  return (
    <Card
      className={`segment-card flex h-full flex-col border-l-[3px] transition-shadow ${severityToBorderClass(segment.severity)} ${
        isDragging ? 'opacity-60 shadow-lg ring-2 ring-accent/20' : ''
      }`}
      draggable={draggable}
      onDragStart={onDragStart}
      onDragOver={onDragOver}
      onDragEnd={onDragEnd}
    >
      <div className="flex min-h-0 flex-1 items-start gap-3">
        {draggable && (
          <button
            type="button"
            className="segment-drag-handle mt-0.5 shrink-0 cursor-grab rounded-md p-0.5 text-muted-foreground transition-colors hover:bg-background hover:text-foreground active:cursor-grabbing"
            aria-label={`Reordenar ${segment.title}`}
            tabIndex={-1}
          >
            <GripVertical className="h-4 w-4" aria-hidden="true" />
          </button>
        )}

        <div className="flex min-w-0 flex-1 flex-col gap-3">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <h5 className="text-sm font-semibold text-foreground">{segment.title}</h5>
            <div className="flex items-center gap-2">
              {count !== null && count > 0 && (
                <Badge variant="default">{count}</Badge>
              )}
              <Badge variant={severityToBadgeVariant(segment.severity)}>
                {severityLabels[segment.severity]}
              </Badge>
            </div>
          </div>

          {segment.content && (
            <p className="text-sm leading-relaxed text-foreground whitespace-pre-wrap">
              {segment.content}
            </p>
          )}

          {segment.items.length > 0 && (
            <ul className="space-y-3">
              {segment.items.map((item, index) => {
                const hasContent =
                  Boolean(item.title || item.description || item.recommendation)
                if (!hasContent) return null

                return (
                  <li
                    key={`${segment.key}-${index}`}
                    className={`rounded-[10px] border border-border bg-background px-3 py-3 border-l-[3px] ${severityToBorderClass(item.severity)}`}
                  >
                    {(item.title || item.severity !== 'neutral') && (
                      <div className="flex flex-wrap items-center justify-between gap-2">
                        {item.title && (
                          <p className="text-sm font-medium text-foreground">{item.title}</p>
                        )}
                        {item.severity !== 'neutral' && (
                          <Badge
                            variant={severityToBadgeVariant(item.severity)}
                            className="text-[10px]"
                          >
                            {severityLabels[item.severity]}
                          </Badge>
                        )}
                      </div>
                    )}
                    {item.description && (
                      <p
                        className={`text-sm text-muted-foreground ${item.title ? 'mt-1' : ''}`}
                      >
                        {item.description}
                      </p>
                    )}
                    {item.recommendation && (
                      <p className="mt-2 text-xs text-foreground">
                        <span className="font-semibold text-accent-secondary">
                          Recomendación:{' '}
                        </span>
                        {item.recommendation}
                      </p>
                    )}
                  </li>
                )
              })}
            </ul>
          )}

          {onAsk && (
            <Button variant="secondary" size="sm" onClick={onAsk} className="mt-auto w-full sm:w-auto">
              <MessageCircle className="h-3.5 w-3.5" aria-hidden="true" />
              Preguntar sobre este segmento
            </Button>
          )}
        </div>
      </div>
    </Card>
  )
}

function SuggestedActionChip({
  action,
  onSelect,
}: {
  action: SuggestedActionDto
  onSelect?: () => void
}) {
  if (!onSelect) {
    return <Badge variant="ai">{action.title}</Badge>
  }

  return (
    <button
      type="button"
      onClick={onSelect}
      className="inline-flex items-center rounded-[6px] border border-ai/20 bg-ai/10 px-2.5 py-1 text-xs font-semibold text-ai transition-colors hover:bg-ai/15"
    >
      {action.title}
    </button>
  )
}
