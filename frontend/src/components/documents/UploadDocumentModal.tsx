import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { documentsApi, chatsApi, foldersApi } from '@/api'
import { queryKeys } from '@/api/queryKeys'
import { errorMessage } from '@/api/client'
import { useAuth } from '@/app/AuthContext'
import { Alert } from '@/components/ui/Alert'
import { Button } from '@/components/ui/Button'
import { FileDropzone } from '@/components/ui/FileDropzone'
import { Modal } from '@/components/ui/Modal'
import { Select } from '@/components/ui/Select'
import { useToast } from '@/components/ui/Toast'

const ANALYZABLE =
  '.pdf,.docx,.rtf,.txt,.md,.csv,.json,.xml,.html,.htm,.log'

export function UploadDocumentModal({
  open,
  onClose,
  lockChatId,
  lockFolderId,
  onUploaded,
}: {
  open: boolean
  onClose: () => void
  lockChatId?: string
  lockFolderId?: string
  onUploaded?: (id: string) => void
}) {
  const { canManageCases } = useAuth()
  const toast = useToast()
  const queryClient = useQueryClient()
  const [file, setFile] = useState<File | null>(null)
  const [destType, setDestType] = useState<'chat' | 'folder'>(lockFolderId ? 'folder' : 'chat')
  const [chatId, setChatId] = useState(lockChatId ?? '')
  const [folderId, setFolderId] = useState(lockFolderId ?? '')
  const [error, setError] = useState<string | null>(null)

  const chats = useQuery({
    queryKey: queryKeys.chats,
    queryFn: chatsApi.list,
    enabled: open && !lockChatId && !lockFolderId,
  })
  const folders = useQuery({
    queryKey: queryKeys.folders,
    queryFn: foldersApi.list,
    enabled: open && canManageCases && !lockChatId && !lockFolderId,
  })

  const mutation = useMutation({
    mutationFn: () => {
      if (!file) throw new Error('Seleccioná un archivo.')
      const destination =
        lockChatId || destType === 'chat'
          ? { chatId: lockChatId || chatId }
          : { folderId: lockFolderId || folderId }
      if (!destination.chatId && !destination.folderId) {
        throw new Error('Elegí un caso o un chat de destino.')
      }
      return documentsApi.upload(file, destination)
    },
    onSuccess: (doc) => {
      if (doc.chatId) queryClient.invalidateQueries({ queryKey: queryKeys.chatDocuments(doc.chatId) })
      if (doc.folderId) queryClient.invalidateQueries({ queryKey: queryKeys.folderDocuments(doc.folderId) })
      toast('Documento subido.')
      setFile(null)
      onUploaded?.(doc.id)
      onClose()
    },
    onError: (err) => setError(errorMessage(err, 'No pudimos subir el documento.')),
  })

  const locked = Boolean(lockChatId || lockFolderId)

  return (
    <Modal
      open={open}
      title="Subir documento"
      onClose={onClose}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button loading={mutation.isPending} onClick={() => mutation.mutate()}>
            Subir documento
          </Button>
        </>
      }
    >
      {error ? <Alert className="mb-3">{error}</Alert> : null}
      <div className="space-y-4">
        <FileDropzone
          file={file}
          onFile={setFile}
          accept={ANALYZABLE}
          hint="PDF, DOCX, RTF o texto plano"
        />
        {!locked ? (
          <>
            <p className="text-[13px] font-medium">Guardar en</p>
            <div className="flex gap-4 text-[14px]">
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="dest"
                  checked={destType === 'chat'}
                  onChange={() => setDestType('chat')}
                />
                Chat
              </label>
              {canManageCases ? (
                <label className="flex items-center gap-2">
                  <input
                    type="radio"
                    name="dest"
                    checked={destType === 'folder'}
                    onChange={() => setDestType('folder')}
                  />
                  Caso
                </label>
              ) : null}
            </div>
            {destType === 'chat' ? (
              <Select label="Chat" value={chatId} onChange={(e) => setChatId(e.target.value)}>
                <option value="">Seleccionar chat</option>
                {(chats.data ?? []).map((chat) => (
                  <option key={chat.id} value={chat.id}>
                    {chat.title}
                  </option>
                ))}
              </Select>
            ) : (
              <Select label="Caso" value={folderId} onChange={(e) => setFolderId(e.target.value)}>
                <option value="">Seleccionar caso</option>
                {(folders.data ?? []).map((folder) => (
                  <option key={folder.id} value={folder.id}>
                    {folder.name}
                  </option>
                ))}
              </Select>
            )}
          </>
        ) : null}
      </div>
    </Modal>
  )
}
