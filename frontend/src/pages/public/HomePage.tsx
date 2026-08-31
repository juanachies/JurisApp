import { PublicLayout, PlanCards } from '@/components/layout/PublicChrome'
import {
  AnalysisPreview,
  CasePreview,
  PlanModePreview,
  SkillPreviewCards,
  WorkspacePreview,
} from '@/components/marketing/ProductMockups'
import { ButtonLink, buttonClass } from '@/components/ui/Button'

export function HomePage() {
  return (
    <PublicLayout>
      <section className="border-b border-border bg-surface">
        <div className="mx-auto grid max-w-[1240px] items-center gap-12 px-5 py-16 lg:grid-cols-2 lg:py-24">
          <div>
            <p className="text-[13px] font-medium text-blue-600">IA aplicada al trabajo jurídico</p>
            <h1 className="mt-3 text-[48px] font-semibold leading-[1.12] tracking-tight text-ink">
              Tus casos, documentos y conversaciones. Una sola inteligencia jurídica.
            </h1>
            <p className="mt-5 max-w-xl text-[17px] leading-relaxed text-muted">
              Organizá casos, analizá documentos, mantené conversaciones y delegá tareas complejas a la IA
              desde un único espacio de trabajo.
            </p>
            <div className="mt-8 flex flex-wrap gap-3">
              <ButtonLink to="/register" size="lg">
                Comenzar
              </ButtonLink>
              <a href="#como-funciona" className={buttonClass('secondary', 'lg')}>
                Ver cómo funciona
              </a>
            </div>
          </div>
          <WorkspacePreview />
        </div>
      </section>

      <section className="mx-auto max-w-[1240px] px-5 py-20">
        <h2 className="max-w-2xl text-[32px] font-semibold leading-tight">
          La IA sola no alcanza. Necesita entender en qué estás trabajando.
        </h2>
        <p className="mt-4 max-w-2xl text-[16px] leading-relaxed text-muted">
          Hoy el trabajo jurídico se reparte entre carpetas, documentos, chatbots y notas sueltas. Cada
          conversación pierde el expediente. JurisApp conecta el caso, la prueba y la conversación para que la
          IA opere con contexto real — no como un chatbot aislado.
        </p>
      </section>

      <section id="producto" className="border-y border-border bg-surface py-20">
        <div className="mx-auto max-w-[1240px] px-5">
          <h2 className="text-[32px] font-semibold">El trabajo jurídico, en un mismo hilo</h2>
          <div className="mt-10 grid gap-8 md:grid-cols-5">
            {[
              ['Casos', 'Agrupá conversaciones y documentos por asunto.'],
              ['Documentos', 'Subí archivos y mantenelos asociados a su contexto.'],
              ['Análisis', 'Pedí resumen, riesgos o recomendaciones sin armar el prompt.'],
              ['Conversaciones', 'Continuá el criterio con historial y documentos a la vista.'],
              ['Tareas', 'Convertí un objetivo en un plan de pasos y ejecutalo.'],
            ].map(([title, body]) => (
              <div key={title}>
                <p className="text-[15px] font-semibold">{title}</p>
                <p className="mt-2 text-[14px] leading-relaxed text-muted">{body}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section id="funciones" className="mx-auto max-w-[1240px] px-5 py-20">
        <h2 className="text-[32px] font-semibold">Análisis de documentos, para leer de verdad</h2>
        <p className="mt-3 max-w-2xl text-[16px] text-muted">
          Pedí un resumen, una lectura de riesgos o recomendaciones a partir del archivo. El resultado queda
          para revisar — no reemplaza tu criterio.
        </p>
        <div className="mt-10">
          <AnalysisPreview />
        </div>
      </section>

      <section className="border-y border-border bg-surface py-20">
        <div className="mx-auto grid max-w-[1240px] items-center gap-12 px-5 lg:grid-cols-2">
          <div>
            <h2 className="text-[32px] font-semibold leading-tight">No le pidas una respuesta. Dale un objetivo.</h2>
            <p className="mt-4 text-[16px] leading-relaxed text-muted">
              JurisApp puede convertir una tarea compleja en un plan de trabajo, mostrarte los pasos antes de
              ejecutarlos y avanzar punto por punto una vez aprobado.
            </p>
          </div>
          <PlanModePreview />
        </div>
      </section>

      <section className="mx-auto max-w-[1240px] px-5 py-20">
        <h2 className="text-[32px] font-semibold">Tu forma de trabajar, convertida en una herramienta.</h2>
        <p className="mt-3 max-w-2xl text-[16px] text-muted">
          Creá instrucciones reutilizables para que JurisApp analice y responda siguiendo tus criterios
          habituales. Las skills las definís vos; no hay un catálogo prearmado.
        </p>
        <div className="mt-10">
          <SkillPreviewCards />
        </div>
      </section>

      <section id="como-funciona" className="border-y border-border bg-surface py-20">
        <div className="mx-auto grid max-w-[1240px] items-center gap-12 px-5 lg:grid-cols-2">
          <div>
            <h2 className="text-[32px] font-semibold">El caso como contenedor del trabajo</h2>
            <p className="mt-4 text-[16px] text-muted">
              Un caso agrupa documentos y conversaciones del mismo asunto. Nada más: sin plazos judiciales, sin
              agenda y sin CRM. Solo el contexto que la IA necesita para no perder el hilo.
            </p>
          </div>
          <CasePreview />
        </div>
      </section>

      <section id="planes" className="mx-auto max-w-[1240px] px-5 py-20">
        <h2 className="text-[32px] font-semibold">Planes</h2>
        <p className="mt-3 text-[16px] text-muted">
          El plan Pro es el que habilita la verificación profesional. Los límites salen de la cuenta, no de un
          listado de marketing.
        </p>
        <div className="mt-10">
          <PlanCards />
        </div>
      </section>

      <section className="border-t border-border bg-navy-900 py-20 text-white">
        <div className="mx-auto max-w-[1240px] px-5">
          <h2 className="max-w-xl text-[32px] font-semibold leading-tight">
            Una mejor forma de trabajar con información jurídica.
          </h2>
          <p className="mt-4 max-w-xl text-[16px] text-white/70">
            Organizá tus casos y llevá la IA al contexto real de tu trabajo.
          </p>
          <ButtonLink to="/register" size="lg" className="mt-8 bg-white text-navy-900 hover:bg-sky-100">
            Crear cuenta
          </ButtonLink>
        </div>
      </section>
    </PublicLayout>
  )
}
