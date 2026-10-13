import { PublicLayout, PlanCards } from '@/components/layout/PublicChrome'

export function PricingPage() {
  return (
    <PublicLayout>
      <div className="mx-auto max-w-[1240px] px-5 py-20">
        <p className="text-[13px] font-semibold uppercase tracking-[0.16em] text-blue-600">JurisApp</p>
        <h1 className="mt-3 text-[clamp(2.2rem,5vw,3.6rem)] font-semibold tracking-[-0.04em]">Planes</h1>
        <p className="mt-3 max-w-xl text-[17px] leading-relaxed text-muted">
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
