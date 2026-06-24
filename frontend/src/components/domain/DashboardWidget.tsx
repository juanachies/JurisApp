import { type ReactNode } from 'react'
import { Card } from '@/components/ui/Card'
import { IconContainer } from '@/components/ui/IconContainer'

interface DashboardWidgetProps {
  title: string
  value: string | number
  description?: string
  icon: ReactNode
  action?: ReactNode
}

export function DashboardWidget({
  title,
  value,
  description,
  icon,
  action,
}: DashboardWidgetProps) {
  return (
    <Card hover className="flex flex-col">
      <div className="flex items-start justify-between">
        <IconContainer size="md">{icon}</IconContainer>
        {action}
      </div>
      <p
        className="mt-4 text-xs font-semibold uppercase tracking-[0.14em] text-muted-foreground"
        style={{ fontFamily: 'var(--font-display)' }}
      >
        {title}
      </p>
      <p className="mt-1 text-2xl font-semibold text-foreground">{value}</p>
      {description && (
        <p className="mt-1 text-xs text-muted-foreground">{description}</p>
      )}
    </Card>
  )
}
