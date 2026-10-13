import { PublicLayout, PlanCards } from '@/components/layout/PublicChrome'
import { HeroShot, HomeCursor, Reveal } from '@/components/marketing/HomeMotion'
import { ButtonLink } from '@/components/ui/Button'

const features = [
  { title: 'Casos', text: 'Un asunto, un hilo.', tone: 'bg-[#172033] text-white' },
  { title: 'Documentos', text: 'El archivo queda en el caso.', tone: 'bg-[#ff5a36] text-white' },
  { title: 'Análisis', text: 'Resumen, riesgos, recomendaciones.', tone: 'bg-[#ffc857] text-[#1a1408]' },
  { title: 'Tareas', text: 'Un objetivo, un plan de pasos.', tone: 'bg-[#1f8a70] text-white' },
]

const steps = [
  ['01', 'Creá el caso'],
  ['02', 'Subí lo que importa'],
  ['03', 'Preguntale a la IA'],
]

const marquee = ['Casos', 'Documentos', 'Chats', 'Análisis', 'Tareas', 'Contexto']

export function HomePage() {
  return (
    <PublicLayout tone="home">
      <HomeCursor />
      <section className="relative overflow-hidden bg-[#0c1220] text-white">
        <div className="home-blob left-[-8%] top-16 h-64 w-64 bg-[#ff5a36]/70" />
        <div className="home-blob right-[8%] top-24 h-72 w-72 bg-[#4d7cff]/60 [animation-delay:-4s]" />
        <div className="home-blob bottom-0 left-[30%] h-56 w-56 bg-[#1f8a70]/50 [animation-delay:-8s]" />
        <div className="relative mx-auto grid min-h-[100svh] max-w-[1240px] items-center gap-10 px-5 pb-16 pt-28 lg:grid-cols-[0.9fr_1.1fr] lg:pt-24">
          <Reveal>
            <p className="text-[13px] font-semibold uppercase tracking-[0.18em] text-[#ffc857]">JurisApp</p>
            <h1 className="mt-4 max-w-[12ch] text-[clamp(3rem,7vw,5.6rem)] font-semibold leading-[0.95] tracking-[-0.045em]">
              El expediente, con inteligencia.
            </h1>
            <p className="mt-5 max-w-md text-[18px] leading-relaxed text-white/75">
              Casos, documentos y conversaciones en un mismo lugar.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <ButtonLink to="/register" size="lg" className="bg-[#ff5a36] text-white hover:bg-[#ff734f]">
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

      <div className="overflow-hidden border-y border-black/10 bg-[#ffc857] py-4 text-[#1a1408]">
        <div className="home-marquee gap-10 text-[15px] font-semibold uppercase tracking-[0.16em]">
          {[...marquee, ...marquee].map((word, index) => (
            <span key={`${word}-${index}`} className="px-4">
              {word}
            </span>
          ))}
        </div>
      </div>

      <section id="producto" className="mx-auto max-w-[1240px] px-5 py-20">
        <Reveal>
          <h2 className="max-w-[14ch] text-[clamp(2.2rem,5vw,4rem)] font-semibold leading-[0.98] tracking-[-0.04em]">
            Todo el trabajo, en contexto.
          </h2>
        </Reveal>
        <div id="funciones" className="mt-10 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
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

      <section id="como-funciona" className="bg-[#172033] py-20 text-white">
        <div className="mx-auto grid max-w-[1240px] gap-12 px-5 lg:grid-cols-[0.8fr_1.2fr] lg:items-end">
          <Reveal>
            <p className="text-[13px] font-semibold uppercase tracking-[0.16em] text-[#7dffe0]">Cómo funciona</p>
            <h2 className="mt-3 text-[clamp(2.2rem,5vw,3.6rem)] font-semibold leading-[0.98] tracking-[-0.04em]">
              Tres pasos. Sin perder el hilo.
            </h2>
          </Reveal>
          <div className="grid gap-4 sm:grid-cols-3">
            {steps.map(([number, label], index) => (
              <Reveal key={number} delay={index * 90}>
                <div className="rounded-[24px] border border-white/10 bg-white/5 p-5">
                  <p className="text-[13px] text-[#ffc857]">{number}</p>
                  <p className="mt-8 text-[22px] font-semibold leading-tight">{label}</p>
                </div>
              </Reveal>
            ))}
          </div>
        </div>
      </section>

      <section id="planes" className="mx-auto max-w-[1240px] px-5 py-20">
        <Reveal>
          <h2 className="text-[clamp(2.2rem,5vw,3.6rem)] font-semibold tracking-[-0.04em]">Elegí un plan.</h2>
          <p className="mt-3 max-w-lg text-[17px] leading-relaxed text-[#3d4654]">
            El plan Pro habilita la verificación profesional.
          </p>
        </Reveal>
        <div className="mt-10">
          <PlanCards />
        </div>
      </section>

      <section className="px-5 pb-20">
        <Reveal>
          <div className="mx-auto flex max-w-[1240px] flex-col items-start justify-between gap-8 rounded-[32px] bg-[#ff5a36] px-8 py-12 text-white md:flex-row md:items-end md:px-12">
            <h2 className="max-w-[12ch] text-[clamp(2.4rem,5vw,4.2rem)] font-semibold leading-[0.95] tracking-[-0.045em]">
              Empezá con tu próximo caso.
            </h2>
            <ButtonLink to="/register" size="lg" className="bg-white text-[#16181d] hover:bg-[#fff4e8]">
              Crear cuenta
            </ButtonLink>
          </div>
        </Reveal>
      </section>
    </PublicLayout>
  )
}
