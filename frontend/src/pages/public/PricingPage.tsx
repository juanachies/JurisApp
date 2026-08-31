import { PublicLayout, PlanCards } from '@/components/layout/PublicChrome'

export function PricingPage() {
  return (
    <PublicLayout>
      <div className="mx-auto max-w-[1240px] px-5 py-16">
        <h1 className="text-[36px] font-semibold">Planes</h1>
        <p className="mt-2 max-w-xl text-[16px] text-muted">
          Elegí el plan según el volumen de chats, documentos y tareas con IA. El plan Pro o Max es necesario
          para solicitar la verificación como abogado.
        </p>
        <div className="mt-10">
          <PlanCards ctaTo="/register" />
        </div>
      </div>
    </PublicLayout>
  )
}
