import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { plansApi, billingApi } from '@/lib/api'
import { PricingCard } from '@/components/domain/PricingCard'
import { Alert } from '@/components/ui/Alert'
import { Skeleton } from '@/components/ui/Loading'
import { LegalDisclaimer } from '@/components/ui/LegalDisclaimer'
import { PremiumPanel } from '@/components/ui/Card'

export function PlansPage() {
  const queryClient = useQueryClient()

  const { data: plans, isLoading: plansLoading } = useQuery({
    queryKey: ['plans'],
    queryFn: plansApi.list,
  })

  const { data: current, isLoading: currentLoading } = useQuery({
    queryKey: ['plans', 'current'],
    queryFn: plansApi.getCurrent,
  })

  const subscribeMutation = useMutation({
    mutationFn: (planId: string) => plansApi.subscribeFree(planId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['plans'] }),
  })

  const checkoutMutation = useMutation({
    mutationFn: (planId: string) => billingApi.createCheckoutSession({ planId }),
    onSuccess: (res) => {
      if (res.url) window.open(res.url, '_blank')
    },
  })

  const simulateMutation = useMutation({
    mutationFn: (planId: string) => billingApi.simulatePurchase({ planId }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['plans'] })
      alert('Suscripción activada (simulación).')
    },
  })

  const handleSelect = (planId: string, price: number) => {
    if (price === 0) {
      subscribeMutation.mutate(planId)
    } else if (import.meta.env.DEV) {
      simulateMutation.mutate(planId)
    } else {
      checkoutMutation.mutate(planId)
    }
  }

  const isLoading = plansLoading || currentLoading

  return (
    <div className="mx-auto max-w-5xl space-y-8">
      <div>
        <h2 className="font-heading text-2xl text-foreground">Planes y suscripción</h2>
        {current && (
          <p className="mt-1 text-sm text-muted-foreground">
            Plan actual: <strong>{current.planName}</strong>
            {current.hasActiveSubscription && ' — Suscripción activa'}
          </p>
        )}
      </div>

      {import.meta.env.DEV && (
        <Alert variant="info">
          Modo desarrollo: podés usar &quot;Simular compra&quot; para activar planes Pro/Max sin Stripe.
        </Alert>
      )}

      <div className="grid gap-6 md:grid-cols-3">
        {isLoading
          ? Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-80" />)
          : plans?.map((plan) => (
              <PricingCard
                key={plan.id}
                plan={plan}
                isCurrent={current?.planId === plan.id}
                isFeatured={plan.type === 'Pro'}
                onSelect={() => handleSelect(plan.id, plan.price)}
                isLoading={
                  subscribeMutation.isPending ||
                  checkoutMutation.isPending ||
                  simulateMutation.isPending
                }
              />
            ))}
      </div>

      <PremiumPanel>
        <LegalDisclaimer />
      </PremiumPanel>
    </div>
  )
}
