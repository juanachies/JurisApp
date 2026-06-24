import { type ReactNode } from 'react'
import { cn } from '@/lib/utils/cn'

interface IconContainerProps {
  children: ReactNode
  variant?: 'default' | 'premium'
  size?: 'sm' | 'md' | 'lg'
  className?: string
}

const sizeClasses = {
  sm: 'h-8 w-8',
  md: 'h-10 w-10',
  lg: 'h-12 w-12',
}

export function IconContainer({
  children,
  variant = 'default',
  size = 'md',
  className,
}: IconContainerProps) {
  return (
    <div
      className={cn(
        'flex items-center justify-center',
        variant === 'premium' ? 'premium-icon-container' : 'icon-container',
        sizeClasses[size],
        className,
      )}
    >
      {children}
    </div>
  )
}
