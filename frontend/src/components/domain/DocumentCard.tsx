import { FileText, Shield } from 'lucide-react'
import type { DocumentDto } from '@/lib/api'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { IconContainer } from '@/components/ui/IconContainer'

interface DocumentCardProps {
  document: DocumentDto
  onAnalyze?: () => void
  onSelect?: () => void
  isAnalyzing?: boolean
  isActive?: boolean
  riskLevel?: 'low' | 'medium' | 'high' | 'critical'
}

const riskBorder = {
  low: 'border-l-success',
  medium: 'border-l-warning',
  high: 'border-l-danger',
  critical: 'border-l-danger',
}

export function DocumentCard({
  document,
  onAnalyze,
  onSelect,
  isAnalyzing,
  isActive,
  riskLevel,
}: DocumentCardProps) {
  return (
    <Card
      className={`document-surface flex items-start gap-4 ${
        riskLevel ? `border-l-[3px] ${riskBorder[riskLevel]}` : ''
      } ${isActive ? 'ring-2 ring-accent/40' : ''}`}
      hover
      onClick={onSelect}
      role={onSelect ? 'button' : undefined}
      tabIndex={onSelect ? 0 : undefined}
      onKeyDown={
        onSelect
          ? (e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault()
                onSelect()
              }
            }
          : undefined
      }
    >
      <IconContainer size="md">
        <FileText className="h-5 w-5" aria-hidden="true" />
      </IconContainer>
      <div className="flex-1 min-w-0">
        <h4 className="truncate text-sm font-semibold text-foreground">{document.title}</h4>
        <p className="mt-0.5 text-xs text-muted-foreground">Documento adjunto</p>
        {onAnalyze && (
          <Button
            variant="secondary"
            size="sm"
            className="mt-3"
            onClick={(e) => {
              e.stopPropagation()
              onAnalyze()
            }}
            isLoading={isAnalyzing}
          >
            <Shield className="h-3.5 w-3.5" aria-hidden="true" />
            Analizar
          </Button>
        )}
      </div>
    </Card>
  )
}
