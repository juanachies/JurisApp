import { Link } from 'react-router-dom'
import { ChevronRight, FileText, Loader2 } from 'lucide-react'
import { useAnalysisRun } from '@/components/documents/analysisRun'
import type { DocumentDto } from '@/types/api'
import { cn } from '@/utils/cn'

export function DocumentAnalysisCard({ doc }: { doc: DocumentDto }) {
  const run = useAnalysisRun(doc.id)
  const complete = doc.hasAnalysis || run?.status === 'done'
  const failed = !complete && run?.status === 'error'
  const analyzing = !complete && !failed && (run?.status === 'queued' || run?.status === 'running')

  const status = complete ? 'Análisis completo' : failed ? 'No se pudo analizar' : analyzing ? 'Analizando con IA' : 'Análisis con IA'
  const hint = complete ? 'Tocá para verlo' : failed ? 'Tocá para reintentar' : analyzing ? 'Tocá para ver el avance' : 'Tocá para abrirlo'

  return (
    <Link
      to={`/app/documents/${doc.id}`}
      aria-busy={analyzing}
      className={cn(
        'flex items-center gap-3 rounded-[14px] border px-3 py-2.5 shadow-sm transition-colors',
        complete && 'border-success/30 bg-success-bg hover:border-success',
        failed && 'border-danger/30 bg-danger-bg hover:border-danger',
        analyzing && 'border-blue-500/40 bg-subtle hover:border-blue-600',
      )}
    >
      <span
        className={cn(
          'flex h-9 w-9 shrink-0 items-center justify-center rounded-[10px] text-white',
          complete ? 'bg-navy-900' : failed ? 'bg-danger' : 'bg-blue-600',
        )}
      >
        {analyzing ? <Loader2 size={16} className="animate-spin" aria-hidden /> : <FileText size={16} aria-hidden />}
      </span>
      <span className="min-w-0 flex-1">
        <span className="block truncate text-[13px] font-medium text-ink">{doc.title}</span>
        <span
          className={cn(
            'mt-0.5 block text-[12px] font-medium',
            complete && 'text-success',
            failed && 'text-danger',
            analyzing && 'text-blue-600',
          )}
        >
          {status}
        </span>
        <span className="block text-[12px] text-muted">{hint}</span>
      </span>
      <ChevronRight size={16} className="shrink-0 text-muted" aria-hidden />
    </Link>
  )
}
