import type { ReactNode } from 'react'
import { cn } from '@/utils/cn'

type BadgeTone = 'neutral' | 'info' | 'success' | 'warning' | 'danger' | 'navy'

const tones: Record<BadgeTone, string> = {
  neutral: 'bg-subtle text-muted',
  info: 'bg-sky-100 text-blue-700',
  success: 'bg-success-bg text-success',
  warning: 'bg-warning-bg text-warning',
  danger: 'bg-danger-bg text-danger',
  navy: 'bg-navy-900 text-white',
}

export function Badge({
  children,
  tone = 'neutral',
  className,
}: {
  children: ReactNode
  tone?: BadgeTone
  className?: string
}) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2 py-0.5 text-[12px] font-medium',
        tones[tone],
        className,
      )}
    >
      {children}
    </span>
  )
}
