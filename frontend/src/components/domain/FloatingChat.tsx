import { useEffect, useRef, type ChangeEvent, type KeyboardEvent } from 'react'
import {
  ChevronDown,
  ChevronUp,
  Paperclip,
  Send,
  Sparkles,
  Trash2,
  X,
} from 'lucide-react'
import type { DocumentAnalysisSegmentDto, MessageDto } from '@/lib/api'
import { ChatMessage } from '@/components/domain/ChatMessage'
import { Button } from '@/components/ui/Button'
import { Textarea } from '@/components/ui/Textarea'
import { LegalDisclaimer } from '@/components/ui/LegalDisclaimer'
import { cn } from '@/lib/utils/cn'

type ChatMode = 'normal' | 'task'

interface FloatingChatProps {
  messages: MessageDto[]
  message: string
  onMessageChange: (value: string) => void
  onSend: () => void
  onKeyDown: (e: KeyboardEvent<HTMLTextAreaElement>) => void
  mode: ChatMode
  onModeChange: (mode: ChatMode) => void
  selectedSegment: DocumentAnalysisSegmentDto | null
  onClearSelectedSegment: () => void
  isSending: boolean
  isExpanded: boolean
  onExpandedChange: (expanded: boolean) => void
  fileInputRef: React.RefObject<HTMLInputElement | null>
  onAttachClick: () => void
  onFileChange: (e: ChangeEvent<HTMLInputElement>) => void
  isUploading: boolean
  onDeleteClick: () => void
  messagesEndRef: React.RefObject<HTMLDivElement | null>
}

export function FloatingChat({
  messages,
  message,
  onMessageChange,
  onSend,
  onKeyDown,
  mode,
  onModeChange,
  selectedSegment,
  onClearSelectedSegment,
  isSending,
  isExpanded,
  onExpandedChange,
  fileInputRef,
  onAttachClick,
  onFileChange,
  isUploading,
  onDeleteClick,
  messagesEndRef,
}: FloatingChatProps) {
  const collapsedInputRef = useRef<HTMLTextAreaElement>(null)
  const expandedInputRef = useRef<HTMLTextAreaElement>(null)

  useEffect(() => {
    if (selectedSegment) {
      if (isExpanded) {
        expandedInputRef.current?.focus()
      } else {
        collapsedInputRef.current?.focus()
      }
    }
  }, [selectedSegment, isExpanded])

  const placeholder =
    mode === 'task'
      ? 'Describí el encargo legal completo...'
      : selectedSegment
        ? `Preguntá sobre "${selectedSegment.title}"...`
        : 'Preguntá sobre el documento o un segmento...'

  const modeToggle = (
    <div className="flex shrink-0 items-center gap-2 text-xs">
      <label className="flex cursor-pointer items-center gap-1.5 whitespace-nowrap">
        <input
          type="radio"
          name="floatingChatMode"
          checked={mode === 'normal'}
          onChange={() => onModeChange('normal')}
          className="accent-accent"
        />
        Normal
      </label>
      <label className="flex cursor-pointer items-center gap-1.5 whitespace-nowrap">
        <input
          type="radio"
          name="floatingChatMode"
          checked={mode === 'task'}
          onChange={() => onModeChange('task')}
          className="accent-accent"
        />
        <Sparkles className="h-3 w-3 text-ai" aria-hidden="true" />
        Tarea IA
      </label>
    </div>
  )

  const segmentChip = selectedSegment && (
    <div className="flex flex-wrap items-center gap-2">
      <span className="inline-flex items-center gap-1.5 rounded-full border border-accent/25 bg-accent/8 px-3 py-1 text-xs font-medium text-foreground">
        Consultando segmento: {selectedSegment.title}
        <button
          type="button"
          onClick={onClearSelectedSegment}
          className="rounded-full p-0.5 text-muted-foreground transition-colors hover:bg-background hover:text-foreground"
          aria-label="Quitar contexto del segmento"
        >
          <X className="h-3 w-3" aria-hidden="true" />
        </button>
      </span>
    </div>
  )

  const sendButton = (
    <Button
      size="sm"
      onClick={onSend}
      isLoading={isSending}
      disabled={!message.trim()}
      className="shrink-0"
    >
      <Send className="h-4 w-4" aria-hidden="true" />
      <span className="hidden sm:inline">
        {mode === 'task' ? 'Generar plan' : 'Enviar'}
      </span>
    </Button>
  )

  return (
    <>
      {isExpanded && (
        <button
          type="button"
          className="floating-chat-backdrop"
          aria-label="Colapsar chat"
          onClick={() => onExpandedChange(false)}
        />
      )}

      <div
        className={cn(
          'floating-chat',
          isExpanded ? 'floating-chat--expanded' : 'floating-chat--collapsed',
        )}
      >
        {isExpanded ? (
          <div className="floating-chat-panel flex h-full flex-col">
            <div className="flex items-center justify-between gap-3 border-b border-border px-4 py-3">
              <h3 className="font-heading text-sm text-foreground">Copiloto legal</h3>
              <div className="flex items-center gap-2">
                {modeToggle}
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => onExpandedChange(false)}
                  aria-label="Colapsar chat"
                >
                  <ChevronDown className="h-4 w-4" aria-hidden="true" />
                </Button>
              </div>
            </div>

            <div className="flex-1 space-y-4 overflow-y-auto chat-scrollbar px-4 py-4">
              {messages.length === 0 ? (
                <p className="py-8 text-center text-sm text-muted-foreground">
                  Sin mensajes. Escribí tu consulta legal.
                </p>
              ) : (
                messages.map((msg) => <ChatMessage key={msg.id} message={msg} />)
              )}
              <div ref={messagesEndRef} />
            </div>

            <div className="border-t border-border px-4 py-3">
              {segmentChip}
              <div className="mt-2 space-y-2">
                <Textarea
                  ref={expandedInputRef}
                  value={message}
                  onChange={(e) => onMessageChange(e.target.value)}
                  onKeyDown={onKeyDown}
                  placeholder={placeholder}
                  rows={3}
                />
                <p className="text-xs text-muted-foreground">
                  {mode === 'task'
                    ? 'Genera un plan de trabajo paso a paso para tu encargo.'
                    : 'Ctrl+Enter para enviar. '}
                  <LegalDisclaimer />
                </p>
                <div className="flex flex-wrap items-center gap-2">
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept=".pdf,.doc,.docx,.txt,.rtf,.odt"
                    className="sr-only"
                    onChange={onFileChange}
                  />
                  <Button
                    variant="secondary"
                    size="sm"
                    onClick={onAttachClick}
                    isLoading={isUploading}
                  >
                    <Paperclip className="h-4 w-4" aria-hidden="true" />
                    Adjuntar
                  </Button>
                  {sendButton}
                  <Button variant="ghost" size="sm" onClick={onDeleteClick}>
                    <Trash2 className="h-4 w-4" aria-hidden="true" />
                    Eliminar
                  </Button>
                </div>
              </div>
            </div>
          </div>
        ) : (
          <div className="floating-chat-bar">
            <div className="flex min-w-0 flex-1 flex-col gap-2">
              {segmentChip}
              <div className="flex items-end gap-2">
                <Textarea
                  ref={collapsedInputRef}
                  value={message}
                  onChange={(e) => onMessageChange(e.target.value)}
                  onKeyDown={onKeyDown}
                  placeholder={placeholder}
                  rows={1}
                  className="min-h-[40px] resize-none py-2.5"
                />
                {sendButton}
                <Button
                  variant="secondary"
                  size="sm"
                  onClick={() => onExpandedChange(true)}
                  aria-label="Expandir chat"
                  className="shrink-0"
                >
                  <ChevronUp className="h-4 w-4" aria-hidden="true" />
                </Button>
              </div>
            </div>
            <div className="mt-2 flex flex-wrap items-center justify-between gap-2 border-t border-border/60 pt-2">
              {modeToggle}
            </div>
          </div>
        )}
      </div>
    </>
  )
}
