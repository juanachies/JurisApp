import type { HTMLAttributes } from 'react'
import { cn } from '@/utils/cn'

export function Card({
  className,
  children,
  ...props
}: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      className={cn('rounded-[12px] border border-border bg-surface shadow-sm', className)}
      {...props}
    >
      {children}
    </div>
  )
}
