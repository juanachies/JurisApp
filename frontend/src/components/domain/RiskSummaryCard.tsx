import type { DocumentAnalysisDto } from '@/lib/api'
import { Card } from '@/components/ui/Card'
import { LegalDisclaimer } from '@/components/ui/LegalDisclaimer'
import { AlertTriangle, CheckCircle, ShieldAlert } from 'lucide-react'

interface RiskSummaryCardProps {
  analysis: DocumentAnalysisDto
}

function getRiskLevel(risks: string): 'low' | 'medium' | 'critical' {
  const lower = risks.toLowerCase()
  if (lower.includes('crític') || lower.includes('alto') || lower.includes('grave')) return 'critical'
  if (lower.includes('medi') || lower.includes('moderad') || lower.includes('atención')) return 'medium'
  return 'low'
}

const riskConfig = {
  low: { icon: CheckCircle, color: 'text-success', label: 'Riesgo bajo' },
  medium: { icon: AlertTriangle, color: 'text-warning', label: 'Riesgo medio' },
  critical: { icon: ShieldAlert, color: 'text-danger', label: 'Riesgo crítico' },
}

export function RiskSummaryCard({ analysis }: RiskSummaryCardProps) {
  const level = getRiskLevel(analysis.risks)
  const config = riskConfig[level]
  const Icon = config.icon

  return (
    <Card highlight className="space-y-4">
      <div className="flex items-center gap-2">
        <Icon className={`h-5 w-5 ${config.color}`} aria-hidden="true" />
        <span className={`text-sm font-semibold ${config.color}`}>{config.label}</span>
      </div>

      <section>
        <h4
          className="mb-2 text-xs font-semibold uppercase tracking-[0.14em] text-accent-secondary"
          style={{ fontFamily: 'var(--font-display)' }}
        >
          Resumen
        </h4>
        <p className="text-sm leading-relaxed text-foreground whitespace-pre-wrap">
          {analysis.summary}
        </p>
      </section>

      {analysis.risks && (
        <section>
          <h4
            className="mb-2 text-xs font-semibold uppercase tracking-[0.14em] text-warning"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            Riesgos detectados
          </h4>
          <p className="text-sm leading-relaxed text-foreground whitespace-pre-wrap">
            {analysis.risks}
          </p>
        </section>
      )}

      {analysis.recommendations && (
        <section>
          <h4
            className="mb-2 text-xs font-semibold uppercase tracking-[0.14em] text-accent-secondary"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            Recomendaciones
          </h4>
          <p className="text-sm leading-relaxed text-foreground whitespace-pre-wrap">
            {analysis.recommendations}
          </p>
        </section>
      )}

      <LegalDisclaimer />
    </Card>
  )
}
