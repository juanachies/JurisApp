import { useCallback, useRef, useState, type DragEvent } from 'react'
import { Upload } from 'lucide-react'
import { cn } from '@/lib/utils/cn'

interface FileUploadProps {
  onFileSelect: (file: File) => void
  accept?: string
  disabled?: boolean
  isUploading?: boolean
}

export function FileUpload({
  onFileSelect,
  accept = '.pdf,.doc,.docx,.txt,.rtf,.odt',
  disabled,
  isUploading,
}: FileUploadProps) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [isDragging, setIsDragging] = useState(false)

  const handleFile = useCallback(
    (file: File) => {
      if (!disabled && !isUploading) onFileSelect(file)
    },
    [disabled, isUploading, onFileSelect],
  )

  const onDrop = (e: DragEvent) => {
    e.preventDefault()
    setIsDragging(false)
    const file = e.dataTransfer.files[0]
    if (file) handleFile(file)
  }

  return (
    <div
      className={cn(
        'flex cursor-pointer flex-col items-center justify-center rounded-[10px] border border-dashed border-accent bg-background-alt px-6 py-8 transition-colors focus-ring',
        isDragging && 'bg-accent/8',
        (disabled || isUploading) && 'cursor-not-allowed opacity-55',
      )}
      onDragOver={(e) => {
        e.preventDefault()
        setIsDragging(true)
      }}
      onDragLeave={() => setIsDragging(false)}
      onDrop={onDrop}
      onClick={() => !disabled && !isUploading && inputRef.current?.click()}
      onKeyDown={(e) => e.key === 'Enter' && inputRef.current?.click()}
      role="button"
      tabIndex={0}
      aria-label="Subir documento"
    >
      <input
        ref={inputRef}
        type="file"
        accept={accept}
        className="sr-only"
        disabled={disabled || isUploading}
        onChange={(e) => {
          const file = e.target.files?.[0]
          if (file) handleFile(file)
          e.target.value = ''
        }}
      />
      <Upload className="mb-2 h-6 w-6 text-accent" aria-hidden="true" />
      <p className="text-sm font-medium text-foreground">
        {isUploading ? 'Subiendo...' : 'Arrastrá un archivo o hacé clic'}
      </p>
      <p className="mt-1 text-xs text-muted-foreground">PDF, DOC, DOCX, TXT, RTF, ODT</p>
    </div>
  )
}
