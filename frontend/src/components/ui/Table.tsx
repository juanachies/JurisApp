import { type ReactNode } from 'react'
import { cn } from '@/lib/utils/cn'

interface TableProps {
  headers: string[]
  children: ReactNode
  className?: string
}

export function Table({ headers, children, className }: TableProps) {
  return (
    <div className="hidden overflow-x-auto md:block">
      <table className={cn('w-full border-collapse text-sm', className)}>
        <thead>
          <tr className="border-b border-border">
            {headers.map((h) => (
              <th
                key={h}
                className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wider text-muted-foreground"
                style={{ fontFamily: 'var(--font-display)' }}
              >
                {h}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>{children}</tbody>
      </table>
    </div>
  )
}

interface TableRowProps {
  children: ReactNode
  onClick?: () => void
}

export function TableRow({ children, onClick }: TableRowProps) {
  return (
    <tr
      className={cn(
        'border-b border-border transition-colors hover:bg-muted/30',
        onClick && 'cursor-pointer',
      )}
      onClick={onClick}
    >
      {children}
    </tr>
  )
}

export function TableCell({ children, className }: { children: ReactNode; className?: string }) {
  return <td className={cn('px-4 py-3 text-foreground', className)}>{children}</td>
}

interface DataListProps {
  children: ReactNode
}

export function DataList({ children }: DataListProps) {
  return <div className="flex flex-col gap-3 md:hidden">{children}</div>
}

interface DataListItemProps {
  children: ReactNode
  onClick?: () => void
}

export function DataListItem({ children, onClick }: DataListItemProps) {
  return (
    <div
      className={cn(
        'rounded-[10px] border border-border bg-background-alt p-4',
        onClick && 'cursor-pointer hover:border-accent-secondary/45',
      )}
      onClick={onClick}
      role={onClick ? 'button' : undefined}
      tabIndex={onClick ? 0 : undefined}
      onKeyDown={onClick ? (e) => e.key === 'Enter' && onClick() : undefined}
    >
      {children}
    </div>
  )
}
