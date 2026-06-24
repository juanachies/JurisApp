import { type ReactNode } from 'react'
import { cn } from '@/lib/utils/cn'

interface TabsProps {
  tabs: { id: string; label: string }[]
  activeTab: string
  onChange: (id: string) => void
  className?: string
}

export function Tabs({ tabs, activeTab, onChange, className }: TabsProps) {
  return (
    <div className={cn('flex gap-1 border-b border-border', className)} role="tablist">
      {tabs.map((tab) => (
        <button
          key={tab.id}
          type="button"
          role="tab"
          aria-selected={activeTab === tab.id}
          className={cn(
            'px-4 py-2.5 text-sm font-medium transition-colors focus-ring rounded-t-[10px]',
            activeTab === tab.id
              ? 'border-b-2 border-accent-secondary text-accent-secondary'
              : 'text-muted-foreground hover:text-foreground',
          )}
          onClick={() => onChange(tab.id)}
        >
          {tab.label}
        </button>
      ))}
    </div>
  )
}

interface TabPanelProps {
  id: string
  activeTab: string
  children: ReactNode
}

export function TabPanel({ id, activeTab, children }: TabPanelProps) {
  if (id !== activeTab) return null
  return (
    <div role="tabpanel" className="pt-4">
      {children}
    </div>
  )
}
