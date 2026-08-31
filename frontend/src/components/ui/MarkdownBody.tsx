import Markdown from 'react-markdown'
import rehypeSanitize from 'rehype-sanitize'
import { cn } from '@/utils/cn'

export function MarkdownBody({ content, className }: { content: string; className?: string }) {
  return (
    <div className={cn('prose-legal', className)}>
      <Markdown rehypePlugins={[rehypeSanitize]}>{content}</Markdown>
    </div>
  )
}

export function AiDisclaimer({ className }: { className?: string }) {
  return (
    <p className={cn('text-[12px] text-faint', className)}>
      Contenido generado con IA. Revisá la información antes de utilizarla en decisiones profesionales.
    </p>
  )
}
