import { Briefcase } from 'lucide-react'
import type { ReactNode } from 'react'
import type { FolderDto } from '@/lib/api'
import { Card } from '@/components/ui/Card'
import { IconContainer } from '@/components/ui/IconContainer'

interface FolderCardProps {
  folder: FolderDto
  onClick?: () => void
  actions?: ReactNode
}

export function FolderCard({ folder, onClick, actions }: FolderCardProps) {
  return (
    <Card hover className="cursor-pointer" onClick={onClick}>
      <div className="flex items-start gap-4">
        <IconContainer>
          <Briefcase className="h-5 w-5" aria-hidden="true" />
        </IconContainer>
        <div className="flex-1 min-w-0">
          <h3 className="font-medium text-foreground">{folder.name}</h3>
          {folder.legalContext && (
            <p className="mt-1 line-clamp-2 text-sm text-muted-foreground">
              {folder.legalContext}
            </p>
          )}
        </div>
        {actions}
      </div>
    </Card>
  )
}
