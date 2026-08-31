import { cn } from '@/utils/cn'
import { Button } from '@/components/ui/Button'

export function EmptyState({
  title,
  description,
  action,
  className,
}: {
  title: string
  description?: string
  action?: { label: string; onClick: () => void }
  className?: string
}) {
  return (
    <div className={cn('rounded-[12px] border border-dashed border-border-strong bg-surface px-6 py-12 text-center', className)}>
      <h3 className="text-[16px] font-semibold text-ink">{title}</h3>
      {description ? <p className="mx-auto mt-2 max-w-md text-[14px] text-muted">{description}</p> : null}
      {action ? (
        <Button className="mt-5" onClick={action.onClick}>
          {action.label}
        </Button>
      ) : null}
    </div>
  )
}
