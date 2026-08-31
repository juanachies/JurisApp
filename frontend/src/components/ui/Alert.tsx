import type { ReactNode } from 'react'
import { cn } from '@/utils/cn'

export function Alert({
  children,
  variant = 'error',
  className,
}: {
  children: ReactNode
  variant?: 'error' | 'warning' | 'info' | 'success'
  className?: string
}) {
  const styles = {
    error: 'bg-danger-bg text-danger border-danger/20',
    warning: 'bg-warning-bg text-warning border-warning/20',
    info: 'bg-sky-100 text-blue-700 border-sky-400/30',
    success: 'bg-success-bg text-success border-success/20',
  } as const

  return (
    <div className={cn('rounded-[8px] border px-3 py-2.5 text-[13px]', styles[variant], className)}>
      {children}
    </div>
  )
}
