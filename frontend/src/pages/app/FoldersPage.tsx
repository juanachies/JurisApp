import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Briefcase, Plus, Pencil, Trash2 } from 'lucide-react'
import { foldersApi } from '@/lib/api'
import { FolderCard } from '@/components/domain/FolderCard'
import { Button } from '@/components/ui/Button'
import { EmptyState } from '@/components/ui/EmptyState'
import { Input } from '@/components/ui/Input'
import { Textarea } from '@/components/ui/Textarea'
import { Modal, ConfirmDialog } from '@/components/ui/Modal'
import { Skeleton } from '@/components/ui/Loading'

export function FoldersPage() {
  const queryClient = useQueryClient()
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [deleteId, setDeleteId] = useState<string | null>(null)
  const [form, setForm] = useState({ name: '', legalContext: '' })

  const { data: folders, isLoading } = useQuery({
    queryKey: ['folders'],
    queryFn: foldersApi.list,
  })

  const saveMutation = useMutation({
    mutationFn: () =>
      editingId
        ? foldersApi.update(editingId, form)
        : foldersApi.create(form),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['folders'] })
      setShowForm(false)
      setEditingId(null)
      setForm({ name: '', legalContext: '' })
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => foldersApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['folders'] })
      setDeleteId(null)
    },
  })

  const openEdit = (folder: { id: string; name: string; legalContext?: string | null }) => {
    setEditingId(folder.id)
    setForm({
      name: folder.name,
      legalContext: folder.legalContext ?? '',
    })
    setShowForm(true)
  }

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="font-heading text-2xl text-foreground">Expedientes</h2>
          <p className="text-sm text-muted-foreground">
            Organizá tus casos y consultas por expediente
          </p>
        </div>
        <Button onClick={() => { setEditingId(null); setForm({ name: '', legalContext: '' }); setShowForm(true) }}>
          <Plus className="h-4 w-4" aria-hidden="true" />
          Nuevo expediente
        </Button>
      </div>

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-28" />
          ))}
        </div>
      ) : !folders?.length ? (
        <EmptyState
          icon={<Briefcase className="h-6 w-6" aria-hidden="true" />}
          title="Organizá tu primer expediente"
          description="Creá carpetas para agrupar consultas, documentos y tareas por caso o cliente."
          actionLabel="Crear expediente"
          onAction={() => setShowForm(true)}
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {folders.map((folder) => (
            <FolderCard
              key={folder.id}
              folder={folder}
              actions={
                <div className="flex gap-1">
                  <Button variant="ghost" size="icon" onClick={() => openEdit(folder)} aria-label="Editar">
                    <Pencil className="h-4 w-4" />
                  </Button>
                  <Button variant="ghost" size="icon" onClick={() => setDeleteId(folder.id)} aria-label="Eliminar">
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              }
            />
          ))}
        </div>
      )}

      <Modal
        isOpen={showForm}
        onClose={() => setShowForm(false)}
        title={editingId ? 'Editar expediente' : 'Nuevo expediente'}
      >
        <form
          onSubmit={(e) => { e.preventDefault(); saveMutation.mutate() }}
          className="space-y-4"
        >
          <Input
            label="Nombre"
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            required
          />
          <Textarea
            label="Contexto legal"
            value={form.legalContext}
            onChange={(e) => setForm({ ...form, legalContext: e.target.value })}
            placeholder="Descripción del caso, partes, estado procesal..."
          />
          <div className="flex justify-end gap-3">
            <Button variant="secondary" type="button" onClick={() => setShowForm(false)}>
              Cancelar
            </Button>
            <Button type="submit" isLoading={saveMutation.isPending}>
              Guardar
            </Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        isOpen={!!deleteId}
        onClose={() => setDeleteId(null)}
        onConfirm={() => deleteId && deleteMutation.mutate(deleteId)}
        title="Eliminar expediente"
        message="¿Eliminar este expediente? Las consultas asociadas no se eliminarán."
        confirmLabel="Eliminar"
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  )
}
