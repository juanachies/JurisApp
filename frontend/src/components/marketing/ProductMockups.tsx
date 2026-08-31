import type { ReactNode } from 'react'
import { Check, Circle } from 'lucide-react'

export function WorkspacePreview() {
  return (
    <div className="overflow-hidden rounded-[12px] border border-border bg-surface shadow-lg">
      <div className="flex min-h-[380px]">
        <aside className="hidden w-[200px] shrink-0 bg-navy-900 p-4 text-white sm:block">
          <p className="text-[13px] font-semibold">JurisApp</p>
          <div className="mt-6 space-y-1 text-[13px]">
            <div className="rounded-[6px] bg-white/10 px-2 py-1.5">Inicio</div>
            <p className="px-2 pt-3 text-[10px] uppercase tracking-wide text-white/40">Trabajo</p>
            <div className="border-l-2 border-sky-400 bg-white/8 px-2 py-1.5">Casos</div>
            <div className="px-2 py-1.5 text-white/70">Chats</div>
            <div className="px-2 py-1.5 text-white/70">Documentos</div>
          </div>
        </aside>
        <div className="grid min-w-0 flex-1 lg:grid-cols-[1fr_240px]">
          <div className="border-r border-border p-5">
            <p className="text-[12px] text-muted">Caso · Gómez c/ Inmobiliaria Norte</p>
            <h3 className="mt-1 text-[16px] font-semibold">Análisis contrato de alquiler</h3>
            <div className="mt-4 space-y-3 text-[13px]">
              <div className="ml-auto max-w-[80%] rounded-[8px] bg-subtle px-3 py-2">
                ¿Qué cláusulas de rescisión son más riesgosas para el locatario?
              </div>
              <div className="max-w-[90%] text-[13px] leading-relaxed text-ink">
                El contrato concentra el riesgo en la cláusula 12 (rescisión anticipada) y en la 18
                (actualización). Conviene negociar un preaviso recíproco y un tope de ajuste.
              </div>
            </div>
            <div className="mt-6 rounded-[8px] border border-border p-3">
              <p className="text-[12px] font-medium text-muted">Plan propuesto</p>
              <ol className="mt-2 space-y-1.5 text-[13px]">
                <li className="flex items-center gap-2">
                  <Check size={14} className="text-success" /> Analizar estructura contractual
                </li>
                <li className="flex items-center gap-2">
                  <Circle size={10} className="fill-sky-500 text-sky-500" /> Detectar cláusulas de riesgo
                </li>
                <li className="flex items-center gap-2 text-muted">
                  <Circle size={10} /> Preparar estrategia de negociación
                </li>
              </ol>
            </div>
          </div>
          <aside className="hidden p-4 text-[13px] lg:block">
            <p className="text-[11px] font-medium uppercase tracking-wide text-faint">Contexto</p>
            <p className="mt-3 font-medium">Gómez c/ Inmobiliaria Norte</p>
            <p className="mt-4 text-[11px] uppercase tracking-wide text-faint">Documentos</p>
            <p className="mt-1">Contrato_Locacion.pdf</p>
            <p>Carta documento.pdf</p>
            <p className="mt-4 text-[11px] uppercase tracking-wide text-faint">Skill</p>
            <p className="mt-1">Análisis contractual</p>
          </aside>
        </div>
      </div>
    </div>
  )
}

export function AnalysisPreview() {
  return (
    <div className="rounded-[12px] border border-border bg-surface p-6 shadow-sm">
      <p className="text-[13px] text-muted">Contrato de prestación de servicios.pdf</p>
      <h3 className="mt-1 text-[18px] font-semibold">Análisis</h3>
      <div className="mt-5 grid gap-5 md:grid-cols-3">
        <PreviewBlock title="Resumen">
          Relación de locación de servicios por 12 meses, con renovación tácita y honorarios mensuales
          ajustables.
        </PreviewBlock>
        <PreviewBlock title="Riesgos">
          Cláusula de indemnidad amplia, jurisdicción exclusiva y falta de tope de responsabilidad.
        </PreviewBlock>
        <PreviewBlock title="Recomendaciones">
          Acotar la indemnidad, incorporar un límite de daños y pactar un mecanismo de revisión de honorarios.
        </PreviewBlock>
      </div>
    </div>
  )
}

export function PlanModePreview() {
  return (
    <div className="rounded-[12px] border border-border bg-surface p-6">
      <p className="text-[12px] font-medium text-blue-600">Modo tarea</p>
      <h3 className="mt-1 text-[18px] font-semibold">Preparar análisis contractual</h3>
      <ol className="mt-5 space-y-3 text-[14px]">
        {[
          ['1', 'Leer documentación'],
          ['2', 'Identificar obligaciones'],
          ['3', 'Detectar riesgos'],
          ['4', 'Elaborar recomendaciones'],
        ].map(([n, label]) => (
          <li key={n} className="flex items-center gap-3">
            <span className="flex size-6 items-center justify-center rounded-full border border-border text-[12px] text-muted">
              {n}
            </span>
            {label}
          </li>
        ))}
      </ol>
      <div className="mt-6 inline-flex h-9 items-center rounded-[8px] bg-navy-900 px-3 text-[13px] font-medium text-white">
        Ejecutar plan
      </div>
    </div>
  )
}

export function SkillPreviewCards() {
  const skills = [
    {
      name: 'Análisis contractual estricto',
      body: 'Prioriza obligaciones, penalidades, jurisdicción y riesgos económicos.',
    },
    {
      name: 'Revisión laboral',
      body: 'Enfoca la lectura en jornada, categorías, despido y cláusulas de confidencialidad.',
    },
    {
      name: 'Estilo ejecutivo',
      body: 'Responde en formato breve, con hallazgos y siguiente paso recomendado.',
    },
  ]
  return (
    <div className="grid gap-4 md:grid-cols-3">
      {skills.map((skill) => (
        <div key={skill.name} className="rounded-[12px] border border-border bg-surface p-5">
          <p className="font-medium">{skill.name}</p>
          <p className="mt-2 text-[13px] text-muted">{skill.body}</p>
          <p className="mt-4 text-[12px] text-success">Estado: ejemplo ilustrativo</p>
        </div>
      ))}
    </div>
  )
}

export function CasePreview() {
  return (
    <div className="rounded-[12px] border border-border bg-surface p-6">
      <h3 className="text-[18px] font-semibold">Gómez c/ Inmobiliaria Norte</h3>
      <p className="mt-2 text-[13px] text-muted">Documentos 2 · Chats 2</p>
      <ul className="mt-4 space-y-2 text-[14px]">
        <li>Contrato.pdf</li>
        <li>Carta documento.pdf</li>
        <li className="text-muted">Análisis inicial</li>
        <li className="text-muted">Estrategia de respuesta</li>
      </ul>
    </div>
  )
}

function PreviewBlock({ title, children }: { title: string; children: ReactNode }) {
  return (
    <div>
      <p className="text-[12px] font-medium uppercase tracking-wide text-faint">{title}</p>
      <p className="mt-2 text-[14px] leading-relaxed text-ink">{children}</p>
    </div>
  )
}
