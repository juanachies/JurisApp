import type { Components } from 'react-markdown'
import Markdown from 'react-markdown'
import rehypeSanitize, { defaultSchema } from 'rehype-sanitize'
import remarkGfm from 'remark-gfm'
import { normalizeMarkdownTables } from '@/utils/markdownTables'
import { cn } from '@/utils/cn'

const tableTags = ['table', 'thead', 'tbody', 'tfoot', 'tr', 'th', 'td']

const schema = {
  ...defaultSchema,
  tagNames: Array.from(new Set([...(defaultSchema.tagNames ?? []), ...tableTags])),
}

const components: Components = {
  table: ({ children }) => (
    <div className="md-table-wrap">
      <table>{children}</table>
    </div>
  ),
}

export function MarkdownBody({ content, className }: { content: string; className?: string }) {
  return (
    <div className={cn('prose-legal', className)}>
      <Markdown
        remarkPlugins={[remarkGfm]}
        rehypePlugins={[[rehypeSanitize, schema]]}
        components={components}
      >
        {normalizeMarkdownTables(content)}
      </Markdown>
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
