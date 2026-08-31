import type { TextareaHTMLAttributes } from 'react'
import { cn } from '@/utils/cn'

type TextareaProps = TextareaHTMLAttributes<HTMLTextAreaElement> & {
  label?: string
  error?: string
  hint?: string
}

export function Textarea({ className, label, error, hint, ...props }: TextareaProps) {
  return (
    <label className="block space-y-1.5">
      {label ? (
        <span className="block text-[13px] font-medium text-ink">{label}</span>
      ) : null}
      <textarea
        className={cn(
          'min-h-28 w-full resize-y rounded-[8px] border bg-surface px-3 py-2 text-[14px] text-ink placeholder:text-faint',
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
