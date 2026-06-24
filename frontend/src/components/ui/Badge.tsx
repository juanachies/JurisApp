import { type HTMLAttributes } from 'react'
import { cn } from '@/lib/utils/cn'

type BadgeVariant = 'default' | 'success' | 'warning' | 'danger' | 'info' | 'ai' | 'premium'

interface BadgeProps extends HTMLAttributes<HTMLSpanElement> {
  variant?: BadgeVariant
}

const variantClasses: Record<BadgeVariant, string> = {
  default: 'bg-muted text-foreground',
  success: 'bg-success/10 text-success border-success/20',
  warning: 'bg-warning/10 text-warning border-warning/20',
  danger: 'bg-danger/10 text-danger border-danger/20',
  info: 'bg-accent-secondary/10 text-accent-secondary border-accent-secondary/20',
  ai: 'bg-ai/10 text-ai border-ai/20',
  premium: 'bg-accent/10 text-accent border-accent/30',
}

export function Badge({ className, variant = 'default', children, ...props }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1 rounded-[6px] border px-2 py-0.5 text-xs font-semibold',
        variantClasses[variant],
        className,
      )}
      {...props}
    >
      {children}
    </span>
  )
}

interface StatusBadgeProps {
  status: string
  label?: string
}

const statusMap: Record<string, BadgeVariant> = {
  Completed: 'success',
  Active: 'success',
  Verified: 'success',
  Pending: 'warning',
  AwaitingApproval: 'warning',
  InProgress: 'ai',
  Failed: 'danger',
  Cancelled: 'danger',
  Rejected: 'danger',
  Inactive: 'default',
}

export function StatusBadge({ status, label }: StatusBadgeProps) {
  const variant = statusMap[status] ?? 'default'
  return <Badge variant={variant}>{label ?? status}</Badge>
}
