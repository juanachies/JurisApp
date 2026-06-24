import { Badge } from '@/components/ui/Badge'

interface SkillChipProps {
  name: string
  onRemove?: () => void
}

export function SkillChip({ name, onRemove }: SkillChipProps) {
  return (
    <Badge variant="success" className="gap-1">
      {name}
      {onRemove && (
        <button
          type="button"
          onClick={onRemove}
          className="ml-0.5 rounded-full hover:bg-success/20 focus-ring"
          aria-label={`Quitar skill ${name}`}
        >
          ×
        </button>
      )}
    </Badge>
  )
}
