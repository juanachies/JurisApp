import { Check } from 'lucide-react'
import type { PlanDto } from '@/lib/api'
import { cn } from '@/lib/utils/cn'
import { formatCurrency } from '@/lib/utils/format'
import { Button } from '@/components/ui/Button'
import { Badge } from '@/components/ui/Badge'
import { parseLimitsJson } from '@/lib/utils/format'

interface PricingCardProps {
  plan: PlanDto
  isCurrent?: boolean
  isFeatured?: boolean
  onSelect?: () => void
  isLoading?: boolean
}

export function PricingCard({
  plan,
  isCurrent,
  isFeatured,
  onSelect,
  isLoading,
}: PricingCardProps) {
  const limits = parseLimitsJson(plan.limitsJson)
  const limitEntries = Object.entries(limits)

  return (
    <div
      className={cn(
        'relative flex flex-col rounded-[16px] border bg-background-alt p-6 shadow-[var(--shadow-card)]',
        isFeatured ? 'border-accent border-t-[3px]' : 'border-border',
        isCurrent && 'ring-2 ring-accent-secondary/30',
      )}
    >
      {isFeatured && (
        <Badge variant="premium" className="absolute -top-3 left-1/2 -translate-x-1/2">
          Recomendado
        </Badge>
      )}

      <h3 className="font-heading text-xl text-foreground">{plan.name}</h3>
      <p className="mt-2">
        <span className="text-3xl font-semibold text-foreground">
          {plan.price === 0 ? 'Gratis' : formatCurrency(plan.price)}
        </span>
        {plan.price > 0 && (
          <span className="text-sm text-muted-foreground"> /mes</span>
        )}
      </p>

      <ul className="mt-6 flex-1 space-y-2">
        {limitEntries.length > 0 ? (
          limitEntries.map(([key, value]) => (
            <li key={key} className="flex items-start gap-2 text-sm text-muted-foreground">
              <Check className="h-4 w-4 shrink-0 text-success mt-0.5" aria-hidden="true" />
              <span>
                {key}: {String(value)}
              </span>
            </li>
          ))
        ) : (
          <li className="flex items-start gap-2 text-sm text-muted-foreground">
            <Check className="h-4 w-4 shrink-0 text-success" aria-hidden="true" />
            Acceso a funciones básicas
          </li>
        )}
      </ul>

      {onSelect && (
        <Button
          className="mt-6 w-full"
          variant={isFeatured ? 'premium' : 'primary'}
          onClick={onSelect}
          isLoading={isLoading}
          disabled={isCurrent}
        >
          {isCurrent ? 'Plan actual' : plan.price === 0 ? 'Comenzar gratis' : 'Elegir plan'}
        </Button>
      )}
    </div>
  )
}
