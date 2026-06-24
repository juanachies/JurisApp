import { FileText, Shield } from 'lucide-react'
import type { DocumentDto } from '@/lib/api'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { IconContainer } from '@/components/ui/IconContainer'

interface DocumentCardProps {
  document: DocumentDto
  onAnalyze?: () => void
  isAnalyzing?: boolean
  riskLevel?: 'low' | 'medium' | 'critical'
}

const riskBorder = {
  low: 'border-l-success',
  medium: 'border-l-warning',
  critical: 'border-l-danger',
}

export function DocumentCard({
  document,
  onAnalyze,
  isAnalyzing,
  riskLevel,
}: DocumentCardProps) {
  return (
    <Card
      className={`document-surface flex items-start gap-4 ${riskLevel ? `border-l-[3px] ${riskBorder[riskLevel]}` : ''}`}
      hover
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
            onClick={onAnalyze}
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
