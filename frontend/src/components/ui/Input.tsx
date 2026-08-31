import type { InputHTMLAttributes } from 'react'
import { cn } from '@/utils/cn'

type InputProps = InputHTMLAttributes<HTMLInputElement> & {
  label?: string
  error?: string
  hint?: string
}

export function Input({ className, label, error, hint, id, ...props }: InputProps) {
  const inputId = id ?? props.name
  return (
    <label className="block space-y-1.5">
      {label ? (
        <span className="block text-[13px] font-medium text-ink">{label}</span>
      ) : null}
      <input
        id={inputId}
        className={cn(
          'h-10 w-full rounded-[8px] border bg-surface px-3 text-[14px] text-ink placeholder:text-faint',
          'transition-colors duration-150 focus:border-blue-600',
          error ? 'border-danger' : 'border-border-strong',
          className,
        )}
        {...props}
      />
      {error ? <span className="block text-[12px] text-danger">{error}</span> : null}
      {!error && hint ? <span className="block text-[12px] text-muted">{hint}</span> : null}
    </label>
  )
}
