import { useEffect, useState, type ReactNode } from 'react'
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

export function PublicHeader({ tone = 'default' }: { tone?: 'default' | 'home' }) {
  const location = useLocation()
  const home = tone === 'home'
  const [scrolled, setScrolled] = useState(false)

  useEffect(() => {
    if (!home) return
    const onScroll = () => setScrolled(window.scrollY > 12)
    onScroll()
    window.addEventListener('scroll', onScroll, { passive: true })
    return () => window.removeEventListener('scroll', onScroll)
  }, [home])

  return (
    <header
      className={cn(
        'z-40',
        home
          ? cn(
              'fixed inset-x-0 top-0 transition-colors duration-300',
              scrolled ? 'border-b border-white/10 bg-[#0c1220]/90 backdrop-blur' : 'bg-transparent',
            )
          : 'sticky top-0 border-b border-border bg-surface/95 backdrop-blur',
      )}
    >
      <div className="mx-auto flex h-16 w-full max-w-[1240px] items-center justify-between px-5">
        <Logo light={home} />
        <nav className="hidden items-center gap-6 md:flex">
          {nav.map((item) => (
            <a
              key={item.href}
              href={item.href}
              className={cn(
                'text-[14px]',
                home ? 'text-white/75 hover:text-white' : 'text-muted hover:text-ink',
                !home && location.pathname === '/pricing' && item.href === '/pricing' && 'text-ink',
              )}
            >
              {item.label}
            </a>
          ))}
        </nav>
        <div className="flex items-center gap-2">
          <Link
            to="/login"
            className={cn('hidden text-[14px] sm:inline', home ? 'text-white/80 hover:text-white' : 'text-muted hover:text-ink')}
          >
            Iniciar sesión
          </Link>
          <Link
            to="/register"
            className={cn(
              'inline-flex h-8 items-center rounded-[8px] px-3 text-[13px] font-medium text-white',
              home ? 'bg-[#ff5a36] hover:bg-[#ff734f]' : 'bg-navy-900 hover:bg-navy-800',
            )}
          >
            Comenzar
          </Link>
        </div>
      </div>
    </header>
  )
}

export function PublicFooter({ tone = 'default' }: { tone?: 'default' | 'home' }) {
  const home = tone === 'home'
  return (
    <footer className={cn('border-t', home ? 'border-white/10 bg-[#0c1220] text-white/70' : 'border-border bg-surface text-muted')}>
      <div className="mx-auto flex max-w-[1240px] flex-wrap items-center justify-between gap-3 px-5 py-8 text-[13px]">
        <Logo light={home} />
        <p>Herramienta de productividad jurídica. No reemplaza el criterio profesional.</p>
      </div>
    </footer>
  )
}

export function PublicLayout({ children, tone = 'default' }: { children: ReactNode; tone?: 'default' | 'home' }) {
  return (
    <div className={tone === 'home' ? 'home-root min-h-screen' : 'min-h-screen bg-canvas'}>
      <PublicHeader tone={tone} />
      {children}
      <PublicFooter tone={tone} />
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
