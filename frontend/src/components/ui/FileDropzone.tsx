import { useRef, useState } from 'react'
import { Upload } from 'lucide-react'
import { cn } from '@/utils/cn'

export function FileDropzone({
  accept,
  file,
  onFile,
  label = 'Arrastrá un archivo acá',
  hint,
}: {
  accept?: string
  file: File | null
  onFile: (file: File) => void
  label?: string
  hint?: string
}) {
  const inputRef = useRef<HTMLInputElement>(null)
  const [drag, setDrag] = useState(false)

  return (
    <div>
      <button
        type="button"
        onClick={() => inputRef.current?.click()}
        onDragOver={(e) => {
          e.preventDefault()
          setDrag(true)
        }}
        onDragLeave={() => setDrag(false)}
        onDrop={(e) => {
          e.preventDefault()
          setDrag(false)
          const next = e.dataTransfer.files[0]
          if (next) onFile(next)
        }}
        className={cn(
          'flex w-full flex-col items-center justify-center rounded-[12px] border border-dashed px-4 py-8 text-center transition-colors',
          drag ? 'border-blue-600 bg-sky-100/60' : 'border-border-strong bg-subtle',
        )}
      >
        <Upload size={18} className="text-muted" />
        <p className="mt-2 text-[14px] font-medium text-ink">{file ? file.name : label}</p>
        <p className="mt-1 text-[12px] text-muted">{hint ?? 'o seleccioná un archivo'}</p>
      </button>
      <input
        ref={inputRef}
        type="file"
        accept={accept}
        className="sr-only"
        onChange={(e) => {
          const next = e.target.files?.[0]
          if (next) onFile(next)
        }}
      />
    </div>
  )
}
