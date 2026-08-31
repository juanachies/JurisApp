import { Link } from 'react-router-dom'
import { cn } from '@/utils/cn'

export function Logo({
  to = '/',
  light,
  className,
}: {
  to?: string
  light?: boolean
  className?: string
}) {
  return (
    <Link to={to} className={cn('inline-flex items-center gap-2 font-semibold tracking-tight', className)}>
      <span
        className={cn(
          'flex size-7 items-center justify-center rounded-[7px] text-[11px] font-semibold',
          light ? 'bg-white/12 text-white' : 'bg-navy-900 text-white',
        )}
        aria-hidden
      >
        J
      </span>
      <span className={cn('text-[16px]', light ? 'text-white' : 'text-navy-900')}>JurisApp</span>
    </Link>
  )
}
