import { useQuery } from '@tanstack/react-query'
import { plansApi } from '@/lib/api'
import { PricingCard } from '@/components/domain/PricingCard'
import { PremiumPanel } from '@/components/ui/Card'
import { Skeleton } from '@/components/ui/Loading'
import { LegalDisclaimer } from '@/components/ui/LegalDisclaimer'

export function PricingPage() {
  const { data: plans, isLoading } = useQuery({
    queryKey: ['plans'],
    queryFn: plansApi.list,
  })

  return (
    <section className="px-4 py-16 md:py-24">
      <div className="mx-auto max-w-5xl text-center">
        <p
          className="text-xs font-semibold uppercase tracking-[0.18em] text-accent"
          style={{ fontFamily: 'var(--font-display)' }}
        >
          Planes
        </p>
        <h1 className="mt-3 font-heading text-4xl text-foreground md:text-5xl">
          Elegí el plan para tu práctica
        </h1>
        <p className="mx-auto mt-4 max-w-2xl text-muted-foreground">
          Desde consultas básicas hasta análisis avanzado y tareas IA ilimitadas.
          Todos los planes incluyen revisión profesional de outputs.
        </p>
      </div>

      <div className="mx-auto mt-12 grid max-w-5xl gap-6 md:grid-cols-3">
        {isLoading
          ? Array.from({ length: 3 }).map((_, i) => (
              <Skeleton key={i} className="h-80" />
            ))
          : plans?.map((plan) => (
              <PricingCard
                key={plan.id}
                plan={plan}
                isFeatured={plan.type === 'Pro'}
              />
            ))}
      </div>

      <div className="mx-auto mt-16 max-w-3xl">
        <PremiumPanel>
          <h2 className="font-heading text-xl text-foreground">Preguntas frecuentes</h2>
          <div className="mt-6 space-y-4 text-sm text-muted-foreground">
            <div>
              <h3 className="font-medium text-foreground">¿La IA reemplaza al abogado?</h3>
              <p className="mt-1">
                No. JurisApp es una herramienta asistiva. Todos los resultados deben ser
                revisados por un profesional calificado.
              </p>
            </div>
            <div>
              <h3 className="font-medium text-foreground">¿Puedo cambiar de plan?</h3>
              <p className="mt-1">
                Sí, podés actualizar o cambiar tu suscripción en cualquier momento desde
                la sección de planes.
              </p>
            </div>
          </div>
          <LegalDisclaimer className="mt-6" />
        </PremiumPanel>
      </div>
    </section>
  )
}
