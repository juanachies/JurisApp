import { Link, Outlet } from 'react-router-dom'
import { Scale } from 'lucide-react'
import { Button } from '@/components/ui/Button'

export function MarketingLayout() {
  return (
    <div className="min-h-screen app-background marble-texture">
      <header className="sticky top-0 z-30 border-b border-border/60 bg-background-alt/90 backdrop-blur-sm">
        <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 md:px-8">
          <Link to="/" className="flex items-center gap-2 no-underline">
            <Scale className="h-7 w-7 text-accent-secondary" aria-hidden="true" />
            <span className="font-heading text-xl text-foreground">JurisApp</span>
          </Link>
          <nav className="flex items-center gap-2 md:gap-4" aria-label="Navegación pública">
            <Link
              to="/pricing"
              className="hidden text-sm font-medium text-muted-foreground no-underline hover:text-accent-secondary sm:inline"
            >
              Planes
            </Link>
            <Link to="/login">
              <Button variant="ghost" size="sm">
                Iniciar sesión
              </Button>
            </Link>
            <Link to="/register">
              <Button size="sm">Comenzar</Button>
            </Link>
          </nav>
        </div>
      </header>

      <Outlet />

      <footer className="border-t border-border bg-background-alt py-12">
        <div className="mx-auto max-w-7xl px-4 md:px-8">
          <div className="flex flex-col items-center justify-between gap-6 md:flex-row">
            <div className="flex items-center gap-2">
              <Scale className="h-5 w-5 text-accent" aria-hidden="true" />
              <span className="font-heading text-foreground">JurisApp</span>
            </div>
            <p className="text-sm text-muted-foreground text-center">
              Plataforma legal con IA para abogados argentinos. Todos los derechos reservados.
            </p>
            <nav className="flex gap-4 text-sm">
              <Link to="/pricing" className="text-muted-foreground no-underline hover:text-accent-secondary">
                Planes
              </Link>
              <Link to="/login" className="text-muted-foreground no-underline hover:text-accent-secondary">
                Acceso
              </Link>
            </nav>
          </div>
        </div>
      </footer>
    </div>
  )
}
