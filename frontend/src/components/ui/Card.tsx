import { type HTMLAttributes, type ReactNode } from 'react'
import { cn } from '@/lib/utils/cn'

interface CardProps extends HTMLAttributes<HTMLDivElement> {
  children: ReactNode
  hover?: boolean
  premium?: boolean
  highlight?: boolean
}

export function Card({
  children,
  className,
  hover = false,
  premium = false,
  highlight = false,
  ...props
}: CardProps) {
  return (
    <div
      className={cn(
        'rounded-[16px] border border-border bg-background-alt p-5 shadow-[var(--shadow-card)]',
        hover && 'transition-all duration-250 hover:border-accent/55 hover:shadow-[var(--shadow-card-hover)]',
        premium && 'premium-panel',
        highlight && 'legal-highlight',
        className,
      )}
      {...props}
    >
      {children}
    </div>
  )
}

export function PremiumPanel({ children, className, ...props }: CardProps) {
  return (
    <div className={cn('premium-panel p-6 md:p-8', className)} {...props}>
      {children}
    </div>
  )
}
