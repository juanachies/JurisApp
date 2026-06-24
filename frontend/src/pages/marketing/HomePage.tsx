import { Link } from 'react-router-dom'
import {
  FileText,
  MessageSquare,
  FolderOpen,
  Sparkles,
  Shield,
  Wand2,
} from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { PremiumPanel } from '@/components/ui/Card'
import { IconContainer } from '@/components/ui/IconContainer'

const features = [
  {
    icon: MessageSquare,
    label: 'Consultas legales',
    title: 'Chats con contexto jurídico',
    description:
      'Consultá con IA entrenada para el derecho argentino. Aplicá skills personalizadas y adjuntá documentos directamente en la conversación.',
  },
  {
    icon: FileText,
    label: 'Análisis documental',
    title: 'Revisión y detección de riesgos',
    description:
      'Subí contratos, demandas y escritos. Obtené resúmenes, alertas de cláusulas críticas y recomendaciones accionables.',
  },
  {
    icon: Sparkles,
    label: 'Tareas IA',
    title: 'Planes de trabajo paso a paso',
    description:
      'Describí un encargo legal complejo y obtené un plan estructurado que podés revisar, editar y ejecutar con control total.',
  },
  {
    icon: FolderOpen,
    label: 'Expedientes',
    title: 'Organización por caso',
    description:
      'Agrupá consultas, documentos y tareas por expediente. Mantené el contexto legal de cada asunto siempre accesible.',
  },
  {
    icon: Wand2,
    label: 'Custom Skills',
    title: 'Conocimiento de tu estudio',
    description:
      'Definí instrucciones especializadas para contratos, laboral, civil o cualquier área. La IA las aplica automáticamente.',
  },
  {
    icon: Shield,
    label: 'Seguridad',
    title: 'Confianza profesional',
    description:
      'Plataforma diseñada para el ejercicio serio del derecho. IA asistiva con disclaimers y revisión profesional obligatoria.',
  },
]

export function HomePage() {
  return (
    <>
      <section className="relative px-4 py-20 md:py-28 lg:py-32">
        <div className="mx-auto max-w-5xl text-center">
          <p
            className="text-xs font-semibold uppercase tracking-[0.18em] text-accent-secondary"
            style={{ fontFamily: 'var(--font-display)' }}
          >
            Plataforma legal con IA · Argentina
          </p>
          <h1 className="mt-4 font-heading text-4xl leading-[1.08] tracking-tight text-foreground md:text-6xl lg:text-7xl">
            El espacio de trabajo legal que tu estudio necesita
          </h1>
          <p className="mx-auto mt-6 max-w-2xl text-lg leading-relaxed text-muted-foreground">
            JurisApp integra inteligencia artificial en flujos reales de trabajo jurídico:
            análisis documental, consultas, tareas automatizadas y organización de expedientes.
            Serio, confiable y diseñado para abogados.
          </p>
          <div className="mt-10 flex flex-col items-center justify-center gap-4 sm:flex-row">
            <Link to="/register">
              <Button size="lg">Comenzar gratis</Button>
            </Link>
            <Link to="/pricing">
              <Button variant="secondary" size="lg">
                Ver planes
              </Button>
            </Link>
          </div>
        </div>
      </section>

      <div className="premium-divider mx-auto max-w-3xl" />

      <section className="px-4 py-16 md:py-24">
        <div className="mx-auto max-w-7xl">
          <div className="grid grid-cols-2 gap-6 md:grid-cols-4">
            {[
              { value: 'IA', label: 'Integrada en flujos' },
              { value: '24/7', label: 'Disponibilidad' },
              { value: '100%', label: 'Control profesional' },
              { value: 'AR', label: 'Derecho argentino' },
            ].map((stat) => (
              <div key={stat.label} className="text-center">
                <p className="font-heading text-3xl text-accent-secondary md:text-4xl">
                  {stat.value}
                </p>
                <p className="mt-1 text-sm text-muted-foreground">{stat.label}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="px-4 py-16 md:py-24">
        <div className="mx-auto max-w-5xl">
          <PremiumPanel>
            <p
              className="text-center text-xs font-semibold uppercase tracking-[0.14em] text-accent"
              style={{ fontFamily: 'var(--font-display)' }}
            >
              Producto
            </p>
            <h2 className="mt-3 text-center font-heading text-3xl text-foreground md:text-4xl">
              Más que un chatbot legal
            </h2>
            <div className="premium-divider mx-auto mt-6 max-w-xs" />
            <p className="mx-auto mt-6 max-w-3xl text-center text-base leading-relaxed text-muted-foreground md:text-lg">
              JurisApp es un sistema operativo legal: un lugar donde analizás documentos con
              detección de riesgos, ejecutás tareas complejas con planes revisables, organizás
              expedientes y acelerás tu práctica con IA controlada. No es un juguete futurista —
              es una herramienta profesional para el ejercicio del derecho.
            </p>
          </PremiumPanel>
        </div>
      </section>

      <section className="px-4 py-16 md:py-24 bg-background-alt/50">
        <div className="mx-auto max-w-7xl">
          <h2 className="text-center font-heading text-3xl text-foreground md:text-4xl">
            Todo lo que necesitás en un solo lugar
          </h2>
          <div className="mt-12 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {features.map((f) => (
              <div
                key={f.title}
                className="rounded-[16px] border border-border bg-background-alt p-6 shadow-[var(--shadow-card)] transition-all duration-250 hover:border-accent-secondary/45 hover:shadow-[var(--shadow-card-hover)]"
              >
                <p
                  className="text-xs font-semibold uppercase tracking-[0.14em] text-accent-secondary"
                  style={{ fontFamily: 'var(--font-display)' }}
                >
                  {f.label}
                </p>
                <div className="mt-3 flex items-center gap-3">
                  <IconContainer size="sm">
                    <f.icon className="h-4 w-4" aria-hidden="true" />
                  </IconContainer>
                  <h3 className="font-medium text-foreground">{f.title}</h3>
                </div>
                <p className="mt-3 text-sm leading-relaxed text-muted-foreground">
                  {f.description}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="px-4 py-20 md:py-28">
        <div className="mx-auto max-w-3xl text-center">
          <h2 className="font-heading text-3xl text-foreground md:text-4xl">
            Empezá a trabajar con más claridad y menos fricción
          </h2>
          <p className="mt-4 text-muted-foreground">
            Unite a los estudios jurídicos que ya confían en una plataforma seria.
          </p>
          <Link to="/register" className="mt-8 inline-block">
            <Button variant="premium" size="lg">
              Crear cuenta gratuita
            </Button>
          </Link>
        </div>
      </section>
    </>
  )
}
