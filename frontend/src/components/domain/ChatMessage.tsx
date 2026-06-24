import { type ReactNode } from 'react'
import { cn } from '@/lib/utils/cn'
import type { MessageDto } from '@/lib/api'
import { LegalDisclaimer } from '@/components/ui/LegalDisclaimer'
import { Badge } from '@/components/ui/Badge'

interface ChatMessageProps {
  message: MessageDto
}

export function ChatMessage({ message }: ChatMessageProps) {
  const role = message.role.toLowerCase()
  const isUser = role === 'user'
  const isAssistant = role === 'assistant'

  return (
    <div
      className={cn(
        'rounded-[10px] px-4 py-3',
        isUser && 'ml-8 bg-accent-secondary/8 border-l-[3px] border-accent-secondary',
        isAssistant && 'mr-8 bg-ai/5 border-l-[3px] border-ai',
        !isUser && !isAssistant && 'bg-muted text-muted-foreground text-sm',
      )}
    >
      <p
        className="mb-1 text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground"
        style={{ fontFamily: 'var(--font-display)' }}
      >
        {isUser ? 'Vos' : isAssistant ? 'JurisApp' : message.role}
      </p>
      <p className="whitespace-pre-wrap text-sm leading-relaxed text-foreground">
        {message.content}
      </p>
      {message.skillsUsed && message.skillsUsed.length > 0 && (
        <div className="mt-2 flex flex-wrap gap-1">
          {message.skillsUsed.map((skill) => (
            <Badge key={skill} variant="success">
              {skill}
            </Badge>
          ))}
        </div>
      )}
      {isAssistant && <LegalDisclaimer className="mt-3" />}
    </div>
  )
}
