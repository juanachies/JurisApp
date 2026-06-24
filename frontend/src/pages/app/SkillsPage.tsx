import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Wand2, Plus, Pencil, Trash2 } from 'lucide-react'
import { skillsApi, lawyerProfilesApi } from '@/lib/api'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { Badge } from '@/components/ui/Badge'
import { EmptyState } from '@/components/ui/EmptyState'
import { Input } from '@/components/ui/Input'
import { Textarea } from '@/components/ui/Textarea'
import { Modal, ConfirmDialog } from '@/components/ui/Modal'
import { Alert } from '@/components/ui/Alert'
import { Skeleton } from '@/components/ui/Loading'

const emptyForm = {
  name: '',
  whenToUse: '',
  instructions: '',
  examples: '',
  redFlags: '',
  outputFormat: '',
}

export function SkillsPage() {
  const queryClient = useQueryClient()
  const [showForm, setShowForm] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [deleteId, setDeleteId] = useState<string | null>(null)
  const [form, setForm] = useState(emptyForm)

  const { data: profile } = useQuery({
    queryKey: ['lawyer-profile'],
    queryFn: lawyerProfilesApi.getMe,
    retry: false,
  })

  const { data: skills, isLoading } = useQuery({
    queryKey: ['skills', 'me'],
    queryFn: skillsApi.listMine,
    enabled: !!profile,
  })

  const saveMutation = useMutation({
    mutationFn: () =>
      editingId
        ? skillsApi.update(editingId, form)
        : skillsApi.create({ ...form, lawyerProfileId: profile!.id }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['skills'] })
      setShowForm(false)
      setEditingId(null)
      setForm(emptyForm)
    },
  })

  const toggleMutation = useMutation({
    mutationFn: ({ id, active }: { id: string; active: boolean }) =>
      active ? skillsApi.deactivate(id) : skillsApi.activate(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['skills'] }),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => skillsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['skills'] })
      setDeleteId(null)
    },
  })

  if (!profile) {
    return (
      <div className="mx-auto max-w-2xl">
        <Alert variant="warning">
          Necesitás un{' '}
          <Link to="/app/settings" className="font-medium underline">
            perfil de abogado
          </Link>{' '}
          para crear custom skills.
        </Alert>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="font-heading text-2xl text-foreground">Custom Skills</h2>
          <p className="text-sm text-muted-foreground">
            Instrucciones especializadas que la IA aplica en tus consultas
          </p>
        </div>
        <Button onClick={() => { setEditingId(null); setForm(emptyForm); setShowForm(true) }}>
          <Plus className="h-4 w-4" aria-hidden="true" />
          Nueva skill
        </Button>
      </div>

      {isLoading ? (
        <div className="space-y-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-24" />
          ))}
        </div>
      ) : !skills?.length ? (
        <EmptyState
          icon={<Wand2 className="h-6 w-6" aria-hidden="true" />}
          title="Creá tu primera skill"
          description="Definí cómo debe comportarse la IA al revisar contratos, redactar escritos o analizar casos específicos."
          actionLabel="Crear skill"
          onAction={() => setShowForm(true)}
        />
      ) : (
        <div className="space-y-3">
          {skills.map((skill) => (
            <Card key={skill.id} className="flex items-start justify-between gap-4">
              <div>
                <div className="flex items-center gap-2">
                  <h3 className="font-medium text-foreground">{skill.name}</h3>
                  <Badge variant={skill.isActive ? 'success' : 'default'}>
                    {skill.isActive ? 'Activa' : 'Inactiva'}
                  </Badge>
                </div>
                <p className="mt-1 text-sm text-muted-foreground line-clamp-2">
                  {skill.whenToUse}
                </p>
              </div>
              <div className="flex shrink-0 gap-1">
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() =>
                    toggleMutation.mutate({ id: skill.id, active: skill.isActive })
                  }
                >
                  {skill.isActive ? 'Desactivar' : 'Activar'}
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => {
                    setEditingId(skill.id)
                    setForm({
                      name: skill.name,
                      whenToUse: skill.whenToUse,
                      instructions: skill.instructions,
                      examples: skill.examples,
                      redFlags: skill.redFlags,
                      outputFormat: skill.outputFormat,
                    })
                    setShowForm(true)
                  }}
                  aria-label="Editar"
                >
                  <Pencil className="h-4 w-4" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => setDeleteId(skill.id)}
                  aria-label="Eliminar"
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
            </Card>
          ))}
        </div>
      )}

      <Modal
        isOpen={showForm}
        onClose={() => setShowForm(false)}
        title={editingId ? 'Editar skill' : 'Nueva skill'}
        size="lg"
      >
        <form
          onSubmit={(e) => { e.preventDefault(); saveMutation.mutate() }}
          className="space-y-4"
        >
          <Input label="Nombre" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
          <Input label="Cuándo usar" value={form.whenToUse} onChange={(e) => setForm({ ...form, whenToUse: e.target.value })} required />
          <Textarea label="Instrucciones" value={form.instructions} onChange={(e) => setForm({ ...form, instructions: e.target.value })} required />
          <Textarea label="Ejemplos" value={form.examples} onChange={(e) => setForm({ ...form, examples: e.target.value })} />
          <Textarea label="Red flags" value={form.redFlags} onChange={(e) => setForm({ ...form, redFlags: e.target.value })} />
          <Input label="Formato de salida" value={form.outputFormat} onChange={(e) => setForm({ ...form, outputFormat: e.target.value })} />
          <div className="flex justify-end gap-3">
            <Button variant="secondary" type="button" onClick={() => setShowForm(false)}>Cancelar</Button>
            <Button type="submit" isLoading={saveMutation.isPending}>Guardar</Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        isOpen={!!deleteId}
        onClose={() => setDeleteId(null)}
        onConfirm={() => deleteId && deleteMutation.mutate(deleteId)}
        title="Eliminar skill"
        message="¿Eliminar esta skill permanentemente?"
        confirmLabel="Eliminar"
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  )
}
