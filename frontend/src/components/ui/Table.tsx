import type { ReactNode } from 'react'
import { cn } from '@/utils/cn'

export function Table({
  children,
  className,
}: {
  children: ReactNode
  className?: string
}) {
  return (
    <div className={cn('overflow-x-auto rounded-[12px] border border-border bg-surface', className)}>
      <table className="w-full min-w-[640px] border-collapse text-left text-[14px]">{children}</table>
    </div>
  )
}

export function THead({ children }: { children: ReactNode }) {
  return (
    <thead className="border-b border-border bg-subtle text-[12px] font-medium uppercase tracking-wide text-muted">
      {children}
    </thead>
  )
}

export function Th({ children, className }: { children?: ReactNode; className?: string }) {
  return <th className={cn('px-4 py-2.5 font-medium', className)}>{children}</th>
}

export function TBody({ children }: { children: ReactNode }) {
  return <tbody className="divide-y divide-border">{children}</tbody>
}

export function Tr({
  children,
  onClick,
  className,
}: {
  children: ReactNode
  onClick?: () => void
  className?: string
}) {
  return (
    <tr
      onClick={onClick}
      className={cn(
        'h-[52px]',
        onClick && 'cursor-pointer hover:bg-subtle',
        className,
      )}
    >
      {children}
    </tr>
  )
}

export function Td({ children, className }: { children: ReactNode; className?: string }) {
  return <td className={cn('px-4 py-3 align-middle text-ink', className)}>{children}</td>
}
