import { Link } from 'react-router-dom'
import { cn } from '@/utils/cn'
import logoImage from '@/assets/logo.png' 

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
      <img
        src={logoImage}
        alt="Logo de JurisApp"
        className={cn('size-7 object-contain', light && 'brightness-0 invert')} 
      />
      <span className={cn('text-[16px]', light ? 'text-white' : 'text-navy-900')}>
        JurisApp
      </span>
    </Link>
  )
}