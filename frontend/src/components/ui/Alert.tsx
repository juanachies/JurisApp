import { type ReactNode } from 'react'
import { cn } from '@/lib/utils/cn'
import { AlertCircle, CheckCircle, Info } from 'lucide-react'

type AlertVariant = 'info' | 'success' | 'warning' | 'error'

interface AlertProps {
  variant?: AlertVariant
  children: ReactNode
  className?: string
}

const variantConfig: Record<AlertVariant, { icon: typeof Info; classes: string }> = {
  info: { icon: Info, classes: 'bg-accent-secondary/8 border-accent-secondary/20 text-accent-secondary' },
  success: { icon: CheckCircle, classes: 'bg-success/8 border-success/20 text-success' },
  warning: { icon: AlertCircle, classes: 'bg-warning/8 border-warning/20 text-warning' },
  error: { icon: AlertCircle, classes: 'bg-danger/8 border-danger/20 text-danger' },
}

export function Alert({ variant = 'info', children, className }: AlertProps) {
  const { icon: Icon, classes } = variantConfig[variant]
  return (
    <div
      className={cn('flex gap-3 rounded-[10px] border px-4 py-3 text-sm', classes, className)}
      role={variant === 'error' ? 'alert' : 'status'}
    >
      <Icon className="h-4 w-4 shrink-0 mt-0.5" aria-hidden="true" />
      <div>{children}</div>
    </div>
  )
}
