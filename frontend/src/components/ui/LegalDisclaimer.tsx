import { Scale } from 'lucide-react'

export function LegalDisclaimer({ className }: { className?: string }) {
  return (
    <p
      className={`text-xs text-muted-foreground leading-relaxed ${className ?? ''}`}
      role="note"
    >
      <Scale className="inline h-3 w-3 mr-1 text-accent-secondary" aria-hidden="true" />
      Los resultados generados por IA son de carácter asistivo y deben ser revisados por un
      profesional del derecho calificado antes de su uso en procedimientos legales.
    </p>
  )
}
