import type { ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link, useLocation } from 'react-router-dom'
import { plansApi } from '@/api'
import { queryKeys } from '@/api/queryKeys'
import { Logo } from '@/components/brand/Logo'
import { Button, ButtonLink } from '@/components/ui/Button'
import { formatPrice, limitLabel, parseLimits } from '@/utils/format'
import { cn } from '@/utils/cn'

const nav = [
  { href: '/#producto', label: 'Producto' },
  { href: '/#funciones', label: 'Funciones' },
  { href: '/#como-funciona', label: 'Cómo funciona' },
  { href: '/pricing', label: 'Planes' },
]

export function PublicHeader() {
  const location = useLocation()
  return (
    <header className="sticky top-0 z-40 border-b border-border bg-surface/95 backdrop-blur">
      <div className="mx-auto flex h-16 w-full max-w-[1240px] items-center justify-between px-5">
        <Logo />
        <nav className="hidden items-center gap-6 md:flex">
          {nav.map((item) => (
            <a
              key={item.href}
              href={item.href}
              className={cn(
                'text-[14px] text-muted hover:text-ink',
                location.pathname === '/pricing' && item.href === '/pricing' && 'text-ink',
              )}
            >
              {item.label}
            </a>
          ))}
        </nav>
        <div className="flex items-center gap-2">
          <Link to="/login" className="hidden text-[14px] text-muted hover:text-ink sm:inline">
            Iniciar sesión
          </Link>
          <Link
            to="/register"
            className="inline-flex h-8 items-center rounded-[8px] bg-navy-900 px-3 text-[13px] font-medium text-white hover:bg-navy-800"
          >
            Comenzar
          </Link>
        </div>
      </div>
    </header>
  )
}

export function PublicFooter() {
  return (
    <footer className="border-t border-border bg-surface">
      <div className="mx-auto flex max-w-[1240px] flex-wrap items-center justify-between gap-3 px-5 py-8 text-[13px] text-muted">
        <Logo />
        <p>Herramienta de productividad jurídica. No reemplaza el criterio profesional.</p>
      </div>
    </footer>
  )
}

export function PublicLayout({ children }: { children: ReactNode }) {
  return (
    <div className="min-h-screen bg-canvas">
      <PublicHeader />
      {children}
      <PublicFooter />
    </div>
  )
}

export function AuthSplit({
  title,
  subtitle,
  children,
}: {
  title: string
  subtitle: string
  children: ReactNode
}) {
  return (
    <div className="grid min-h-screen lg:grid-cols-2">
      <div className="hidden flex-col justify-between bg-navy-900 px-12 py-12 text-white lg:flex">
        <Logo light to="/" />
        <div className="max-w-md">
          <p className="text-[13px] font-medium text-sky-400">IA aplicada al trabajo jurídico</p>
          <h1 className="mt-3 text-[36px] font-semibold leading-tight">
            Casos, documentos y conversaciones en un solo espacio.
          </h1>
          <p className="mt-4 text-[15px] text-white/70">
            JurisApp mantiene el contexto de tu trabajo para que la IA analice y responda sobre lo que realmente
            estás haciendo.
          </p>
        </div>
        <p className="text-[12px] text-white/40">JurisApp — software jurídico, Argentina</p>
      </div>
      <div className="flex items-center justify-center bg-canvas px-5 py-12">
        <div className="w-full max-w-md">
          <div className="mb-8 lg:hidden">
            <Logo />
          </div>
          <h2 className="text-[26px] font-semibold text-ink">{title}</h2>
          <p className="mt-1 text-[14px] text-muted">{subtitle}</p>
          <div className="mt-8">{children}</div>
        </div>
      </div>
    </div>
  )
}

export function PlanCards({ ctaTo = '/register' }: { ctaTo?: string }) {
  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: queryKeys.plans,
    queryFn: plansApi.list,
  })

  if (isLoading) {
    return (
      <div className="grid gap-4 md:grid-cols-3">
        {[0, 1, 2].map((i) => (
          <div key={i} className="h-72 animate-pulse rounded-[12px] bg-subtle" />
        ))}
      </div>
    )
  }

  if (isError || !data) {
    return (
      <div className="rounded-[12px] border border-border bg-surface p-6 text-center">
        <p className="text-[14px] text-muted">No pudimos cargar los planes.</p>
        <Button className="mt-3" variant="secondary" onClick={() => refetch()}>
          Reintentar
        </Button>
      </div>
    )
  }

  return (
    <div className="grid gap-4 md:grid-cols-3">
      {data.map((plan) => {
        const limits = parseLimits(plan.limitsJson)
        const highlight = plan.type === 'Pro'
        return (
          <div
            key={plan.id}
            className={cn(
              'flex flex-col rounded-[12px] border bg-surface p-6',
              highlight ? 'border-navy-900 shadow-sm' : 'border-border',
            )}
          >
            <p className="text-[13px] font-medium text-muted">{plan.name}</p>
            <p className="mt-2 text-[32px] font-semibold text-ink">{formatPrice(plan.price)}</p>
            {highlight ? (
              <p className="mt-1 text-[12px] text-blue-600">Requerido para verificación profesional</p>
            ) : (
              <p className="mt-1 text-[12px] text-faint">por mes</p>
            )}
            <ul className="mt-5 flex-1 space-y-2 text-[14px] text-ink">
              <li>Chats: {limitLabel(limits.chats)}</li>
              <li>Documentos: {limitLabel(limits.documents)}</li>
              <li>Tareas con IA: {limitLabel(limits.aiTasks)}</li>
            </ul>
            <ButtonLink
              to={ctaTo}
              className="mt-6 w-full"
              variant={highlight ? 'primary' : 'secondary'}
            >
              {plan.type === 'Free' ? 'Empezar gratis' : 'Elegir plan'}
            </ButtonLink>
          </div>
        )
      })}
    </div>
  )
}
