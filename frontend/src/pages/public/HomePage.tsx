import { PublicLayout, PlanCards } from '@/components/layout/PublicChrome'
import { HeroShot, HomeCursor, Reveal } from '@/components/marketing/HomeMotion'
import {
  AnalysisPreview,
  CasePreview,
  PlanModePreview,
  SkillPreviewCards,
} from '@/components/marketing/ProductMockups'
import { ButtonLink } from '@/components/ui/Button'

const features = [
  {
    title: 'Casos',
    text: 'Agrupá conversaciones y documentos por asunto.',
    tone: 'bg-navy-950 text-white',
  },
  {
    title: 'Documentos',
    text: 'Subí archivos y mantenelos en su contexto.',
    tone: 'bg-navy-800 text-white',
  },
  {
    title: 'Análisis',
    text: 'Pedí resumen, riesgos o recomendaciones sin armar el prompt.',
    tone: 'bg-sky-100 text-navy-900',
  },
  {
    title: 'Conversaciones',
    text: 'Seguí el criterio con el historial y los documentos a la vista.',
    tone: 'bg-ember text-white',
  },
  {
    title: 'Tareas',
    text: 'Convertí un objetivo en un plan de pasos y ejecutalo.',
    tone: 'bg-pine text-white',
  },
]

const marquee = ['Casos', 'Documentos', 'Chats', 'Análisis', 'Tareas', 'Contexto']

export function HomePage() {
  return (
    <PublicLayout tone="home">
      <HomeCursor />
      <section className="relative overflow-hidden bg-navy-950 text-white">
        <div className="home-blob left-[-8%] top-16 h-64 w-64 bg-blue-500/35" />
        <div className="home-blob right-[8%] top-24 h-72 w-72 bg-ember/40 [animation-delay:-4s]" />
        <div className="home-blob bottom-0 left-[30%] h-56 w-56 bg-pine/35 [animation-delay:-8s]" />
        <div className="relative mx-auto grid min-h-[100svh] max-w-[1240px] items-center gap-10 px-5 pb-16 pt-28 lg:grid-cols-[0.9fr_1.1fr] lg:pt-24">
          <Reveal>
            <p className="text-[13px] font-semibold uppercase tracking-[0.18em] text-sky-400">JurisApp</p>
            <h1 className="mt-4 max-w-[12ch] text-[clamp(3rem,7vw,5.6rem)] font-semibold leading-[0.95] tracking-[-0.045em]">
              El expediente, con inteligencia.
            </h1>
            <p className="mt-5 max-w-md text-[18px] leading-relaxed text-white/75">
              Casos, documentos y conversaciones en un mismo lugar.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <ButtonLink to="/register" size="lg" className="bg-ember text-white hover:bg-ember-hover">
                Empezar gratis
              </ButtonLink>
              <a
                href="#producto"
                className="inline-flex h-11 items-center rounded-[8px] border border-white/25 px-4 text-[15px] font-medium text-white hover:bg-white/10"
              >
                Ver el producto
              </a>
            </div>
          </Reveal>
          <Reveal delay={120}>
            <HeroShot />
          </Reveal>
        </div>
      </section>

      <div className="overflow-hidden border-y border-white/10 bg-navy-900 py-4 text-white">
        <div className="home-marquee gap-10 text-[15px] font-semibold uppercase tracking-[0.16em]">
          {[...marquee, ...marquee].map((word, index) => (
            <span key={`${word}-${index}`} className="inline-flex items-center gap-10 px-4">
              {word}
              <span className="size-1.5 rounded-full bg-ember" />
            </span>
          ))}
        </div>
      </div>

      <section id="producto" className="mx-auto max-w-[1240px] px-5 py-20">
        <Reveal>
          <h2 className="max-w-[16ch] text-[clamp(2.2rem,5vw,4rem)] font-semibold leading-[0.98] tracking-[-0.04em]">
            La IA sola no alcanza.
          </h2>
          <p className="mt-4 max-w-2xl text-[18px] leading-relaxed text-muted">
            Cada chat suelto pierde el expediente. JurisApp une el caso, la prueba y la conversación para que la
            IA trabaje con contexto real.
          </p>
        </Reveal>
        <div id="funciones" className="mt-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {features.map((feature, index) => (
            <Reveal key={feature.title} delay={index * 80}>
              <article className={`flex min-h-56 flex-col justify-between rounded-[24px] p-6 ${feature.tone}`}>
                <p className="text-[13px] font-semibold uppercase tracking-[0.14em] opacity-70">0{index + 1}</p>
                <div>
                  <h3 className="text-[28px] font-semibold tracking-[-0.03em]">{feature.title}</h3>
                  <p className="mt-2 text-[16px] leading-snug opacity-90">{feature.text}</p>
                </div>
              </article>
            </Reveal>
          ))}
        </div>
      </section>

      <section id="como-funciona" className="bg-navy-900 py-20 text-white">
        <div className="mx-auto grid max-w-[1240px] items-center gap-10 px-5 lg:grid-cols-2">
          <Reveal>
            <p className="text-[13px] font-semibold uppercase tracking-[0.16em] text-sky-400">Análisis</p>
            <h2 className="mt-3 text-[clamp(2rem,4vw,3.2rem)] font-semibold leading-[1.02] tracking-[-0.04em]">
              Para leer el documento de verdad.
            </h2>
            <p className="mt-4 max-w-lg text-[17px] leading-relaxed text-white/75">
              Resumen, riesgos o recomendaciones a partir del archivo. Queda para revisar: no reemplaza tu criterio.
            </p>
          </Reveal>
          <Reveal delay={80}>
            <AnalysisPreview />
          </Reveal>
        </div>
      </section>

      <section className="mx-auto grid max-w-[1240px] items-center gap-10 px-5 py-20 lg:grid-cols-2">
        <Reveal>
          <h2 className="text-[clamp(2rem,4vw,3.2rem)] font-semibold leading-[1.02] tracking-[-0.04em]">
            No le pidas una respuesta. Dale un objetivo.
          </h2>
          <p className="mt-4 max-w-lg text-[17px] leading-relaxed text-muted">
            Una tarea compleja se vuelve un plan. Ves los pasos antes de ejecutarlos y avanzás cuando lo aprobás.
          </p>
        </Reveal>
        <Reveal delay={80}>
          <PlanModePreview />
        </Reveal>
      </section>

      <section className="bg-sky-100/70 py-20">
        <div className="mx-auto max-w-[1240px] px-5">
          <Reveal>
            <h2 className="max-w-[16ch] text-[clamp(2rem,4vw,3.2rem)] font-semibold leading-[1.02] tracking-[-0.04em]">
              Tu forma de trabajar, hecha herramienta.
            </h2>
            <p className="mt-4 max-w-2xl text-[17px] leading-relaxed text-muted">
              Creá instrucciones reutilizables. Las skills las definís vos; no hay un catálogo prearmado.
            </p>
          </Reveal>
          <div className="mt-10">
            <SkillPreviewCards />
          </div>
        </div>
      </section>

      <section className="mx-auto grid max-w-[1240px] items-center gap-10 px-5 py-20 lg:grid-cols-2">
        <Reveal>
          <h2 className="text-[clamp(2rem,4vw,3.2rem)] font-semibold leading-[1.02] tracking-[-0.04em]">
            El caso guarda el trabajo.
          </h2>
          <p className="mt-4 max-w-lg text-[17px] leading-relaxed text-muted">
            Agrupa documentos y conversaciones del mismo asunto. Sin agenda ni CRM: solo el contexto para no perder
            el hilo.
          </p>
        </Reveal>
        <Reveal delay={80}>
          <CasePreview />
        </Reveal>
      </section>

      <section id="planes" className="mx-auto max-w-[1240px] px-5 py-20">
        <Reveal>
          <h2 className="text-[clamp(2.2rem,5vw,3.6rem)] font-semibold tracking-[-0.04em]">Planes</h2>
          <p className="mt-3 max-w-xl text-[17px] leading-relaxed text-muted">
            El plan Pro habilita la verificación profesional. Los límites salen de tu cuenta.
          </p>
        </Reveal>
        <div className="mt-10">
          <PlanCards />
        </div>
      </section>

      <section className="px-5 pb-20">
        <Reveal>
          <div className="mx-auto flex max-w-[1240px] flex-col items-start justify-between gap-8 rounded-[32px] bg-navy-950 px-8 py-12 text-white md:flex-row md:items-end md:px-12">
            <h2 className="max-w-[12ch] text-[clamp(2.4rem,5vw,4.2rem)] font-semibold leading-[0.95] tracking-[-0.045em]">
              Empezá con tu próximo caso.
            </h2>
            <ButtonLink to="/register" size="lg" className="bg-ember text-white hover:bg-ember-hover">
              Crear cuenta
            </ButtonLink>
          </div>
        </Reveal>
      </section>
    </PublicLayout>
  )
}
