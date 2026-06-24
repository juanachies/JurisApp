import { Link, Outlet } from 'react-router-dom'
import { Scale } from 'lucide-react'

export function AuthLayout() {
  return (
    <div className="flex min-h-screen app-background marble-texture">
      <div className="hidden w-1/2 flex-col justify-between bg-sidebar p-12 lg:flex">
        <Link to="/" className="flex items-center gap-2 no-underline">
          <Scale className="h-8 w-8 text-accent" aria-hidden="true" />
          <span className="font-heading text-2xl text-white">JurisApp</span>
        </Link>
        <div>
          <h2 className="font-heading text-3xl leading-tight text-white">
            Tu espacio de trabajo legal, con inteligencia integrada
          </h2>
          <p className="mt-4 max-w-md text-sm leading-relaxed text-white/70">
            Analizá documentos, gestioná expedientes, ejecutá tareas con IA y consultá con
            confianza. Diseñado para abogados independientes y estudios jurídicos en Argentina.
          </p>
        </div>
        <p className="text-xs text-white/40">
          Plataforma segura · Datos protegidos · IA asistiva con revisión profesional
        </p>
      </div>

      <div className="flex flex-1 items-center justify-center p-6 md:p-12">
        <div className="w-full max-w-md">
          <div className="mb-8 flex items-center gap-2 lg:hidden">
            <Scale className="h-6 w-6 text-accent-secondary" aria-hidden="true" />
            <span className="font-heading text-xl text-foreground">JurisApp</span>
          </div>
          <Outlet />
        </div>
      </div>
    </div>
  )
}
