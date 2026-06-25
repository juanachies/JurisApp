import type { AnalysisSeverity, SegmentedDocumentAnalysisDto } from '@/lib/api'

type SeverityBadgeVariant = 'default' | 'success' | 'warning' | 'danger' | 'info'

const severityRank: Record<AnalysisSeverity, number> = {
  neutral: 0,
  low: 1,
  medium: 2,
  high: 3,
  critical: 4,
}

export const severityLabels: Record<AnalysisSeverity, string> = {
  neutral: 'Neutral',
  low: 'Bajo',
  medium: 'Medio',
  high: 'Alto',
  critical: 'Crítico',
}

export function severityToBadgeVariant(severity: AnalysisSeverity): SeverityBadgeVariant {
  switch (severity) {
    case 'low':
      return 'success'
    case 'medium':
      return 'warning'
    case 'high':
    case 'critical':
      return 'danger'
    default:
      return 'info'
  }
}

export function severityToBorderClass(severity: AnalysisSeverity): string {
  switch (severity) {
    case 'low':
      return 'border-l-success'
    case 'medium':
      return 'border-l-warning'
    case 'high':
    case 'critical':
      return 'border-l-danger'
    default:
      return 'border-l-border'
  }
}

export function maxSeverity(...severities: AnalysisSeverity[]): AnalysisSeverity {
  return severities.reduce<AnalysisSeverity>(
    (max, current) => (severityRank[current] > severityRank[max] ? current : max),
    'neutral',
  )
}

export function getAnalysisMaxSeverity(
  analysis: SegmentedDocumentAnalysisDto,
): AnalysisSeverity {
  const segmentSeverities = analysis.segments.map((s) => s.severity)
  const itemSeverities = analysis.segments.flatMap((s) => s.items.map((i) => i.severity))
  return maxSeverity(...segmentSeverities, ...itemSeverities)
}

export function severityToDocumentRiskLevel(
  severity: AnalysisSeverity,
): 'low' | 'medium' | 'high' | 'critical' | undefined {
  if (severity === 'neutral') return undefined
  if (severity === 'high') return 'high'
  return severity
}

export function formatMainFieldValue(value: unknown): string {
  if (value === null || value === undefined) return '—'
  if (typeof value === 'string') return value
  if (typeof value === 'number' || typeof value === 'boolean') return String(value)
  if (Array.isArray(value)) return value.map((v) => String(v)).join(', ')
  return JSON.stringify(value)
}

export function formatConfidence(confidence: number): string {
  return `${Math.round(confidence * 100)}%`
}

export function formatFieldLabel(key: string): string {
  return key
    .replace(/([A-Z])/g, ' $1')
    .replace(/_/g, ' ')
    .trim()
    .replace(/^\w/, (c) => c.toUpperCase())
}
