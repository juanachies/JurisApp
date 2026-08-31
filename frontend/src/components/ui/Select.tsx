import type { SelectHTMLAttributes } from 'react'
import { cn } from '@/utils/cn'

type SelectProps = SelectHTMLAttributes<HTMLSelectElement> & {
  label?: string
  error?: string
}

export function Select({ className, label, error, children, ...props }: SelectProps) {
  return (
    <label className="block space-y-1.5">
      {label ? (
        <span className="block text-[13px] font-medium text-ink">{label}</span>
      ) : null}
      <select
        className={cn(
          'h-10 w-full rounded-[8px] border bg-surface px-3 text-[14px] text-ink',
          error ? 'border-danger' : 'border-border-strong',
          className,
        )}
        {...props}
      >
        {children}
      </select>
      {error ? <span className="block text-[12px] text-danger">{error}</span> : null}
    </label>
  )
}
