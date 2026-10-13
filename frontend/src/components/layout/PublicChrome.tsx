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
              scrolled ? 'border-b border-white/10 bg-navy-950/90 backdrop-blur' : 'bg-transparent',
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
              'inline-flex h-9 items-center rounded-[10px] px-3.5 text-[13px] font-medium text-white',
              'bg-ember hover:bg-ember-hover',
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
    <footer className={cn('border-t', home ? 'border-white/10 bg-navy-950 text-white/70' : 'border-border bg-surface text-muted')}>
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
      <div className="relative hidden flex-col justify-between overflow-hidden bg-navy-950 px-12 py-12 text-white lg:flex">
        <div className="home-blob left-[-10%] top-10 h-56 w-56 bg-blue-500/30" />
        <div className="home-blob bottom-0 right-[-8%] h-64 w-64 bg-ember/25" />
        <Logo light to="/" className="relative" />
        <div className="relative max-w-md">
          <p className="text-[13px] font-medium uppercase tracking-[0.16em] text-sky-400">IA aplicada al trabajo jurídico</p>
          <h1 className="mt-4 text-[clamp(2.2rem,4vw,3.4rem)] font-semibold leading-[1.02]">
            Casos, documentos y conversaciones en un solo espacio.
          </h1>
          <p className="mt-4 text-[16px] leading-relaxed text-white/70">
            JurisApp mantiene el contexto de tu trabajo para que la IA analice y responda sobre lo que realmente
            estás haciendo.
          </p>
          <span className="mt-8 block h-1 w-12 rounded-full bg-ember" />
        </div>
        <p className="relative text-[12px] text-white/40">JurisApp — software jurídico, Argentina</p>
      </div>
      <div className="flex items-center justify-center bg-canvas px-5 py-12">
        <div className="w-full max-w-md rounded-[24px] border border-border bg-surface p-8 shadow-card">
          <div className="mb-8 lg:hidden">
            <Logo />
          </div>
          <h2 className="text-[28px] font-semibold tracking-[-0.04em] text-ink">{title}</h2>
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
          <div key={i} className="h-72 animate-pulse rounded-[20px] bg-subtle" />
        ))}
      </div>
    )
  }

  if (isError || !data) {
    return (
      <div className="rounded-[20px] border border-border bg-surface p-6 text-center shadow-card">
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
              'flex flex-col rounded-[20px] border bg-surface p-6 shadow-card',
              highlight ? 'border-ember' : 'border-border',
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
