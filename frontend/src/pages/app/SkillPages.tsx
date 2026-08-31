import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { skillsApi } from '@/api'
import { errorMessage } from '@/api/client'
import { queryKeys } from '@/api/queryKeys'
import { useAuth } from '@/app/AuthContext'
import { AppPage, QueryError } from '@/components/layout/AppShell'
import { Alert } from '@/components/ui/Alert'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { ConfirmDialog } from '@/components/ui/Modal'
import { EmptyState } from '@/components/ui/EmptyState'
import { Input } from '@/components/ui/Input'
import { PageHeader } from '@/components/ui/PageHeader'
import { Textarea } from '@/components/ui/Textarea'
import { useToast } from '@/components/ui/Toast'
import { Skeleton } from '@/components/ui/Loading'
import type { CreateCustomSkillRequest, UpdateCustomSkillRequest } from '@/types/api'

const schema = z.object({
  name: z.string().min(1, 'El nombre es obligatorio'),
  whenToUse: z.string(),
  instructions: z.string().min(1, 'Las instrucciones son obligatorias'),
  examples: z.string(),
  redFlags: z.string(),
  outputFormat: z.string(),
})

type FormValues = z.infer<typeof schema>

export function SkillsPage() {
  const navigate = useNavigate()
  const toast = useToast()
  const queryClient = useQueryClient()
  const [deleteId, setDeleteId] = useState<string | null>(null)
  const skillsQuery = useQuery({ queryKey: queryKeys.skills, queryFn: skillsApi.list })

  const activate = useMutation({
    mutationFn: (id: string) => skillsApi.activate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.skills })
      toast('Skill activada.')
    },
  })
  const deactivate = useMutation({
    mutationFn: (id: string) => skillsApi.deactivate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.skills })
      toast('Skill desactivada.')
    },
  })
  const remove = useMutation({
    mutationFn: (id: string) => skillsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.skills })
      toast('Skill eliminada.')
      setDeleteId(null)
    },
  })

  return (
    <AppPage>
      <PageHeader
        title="Skills"
        description="Definí cómo querés que JurisApp analice y responda."
        actions={<Button onClick={() => navigate('/app/skills/new')}>Nueva skill</Button>}
      />
      {skillsQuery.isLoading ? <Skeleton className="h-40" /> : null}
      {skillsQuery.isError ? (
        <QueryError message="No pudimos cargar tus skills." onRetry={() => skillsQuery.refetch()} />
      ) : null}
      {!skillsQuery.isLoading && (skillsQuery.data ?? []).length === 0 ? (
        <EmptyState
          title="Todavía no creaste skills."
          description="Guardá instrucciones reutilizables para adaptar cómo responde JurisApp."
          action={{ label: 'Nueva skill', onClick: () => navigate('/app/skills/new') }}
        />
      ) : (
        <div className="grid gap-4 md:grid-cols-2">
          {(skillsQuery.data ?? []).map((skill) => (
            <article key={skill.id} className="rounded-[12px] border border-border bg-surface p-5">
              <div className="flex items-start justify-between gap-3">
                <h2 className="text-[16px] font-semibold">{skill.name}</h2>
                <Badge tone={skill.isActive ? 'success' : 'neutral'}>
                  {skill.isActive ? 'Activa' : 'Inactiva'}
                </Badge>
              </div>
              <p className="mt-2 line-clamp-3 text-[13px] text-muted">{skill.instructions}</p>
              <div className="mt-4 flex flex-wrap gap-2">
                <Button size="sm" variant="secondary" onClick={() => navigate(`/app/skills/${skill.id}/edit`)}>
                  Editar
                </Button>
                {skill.isActive ? (
                  <Button size="sm" variant="ghost" onClick={() => deactivate.mutate(skill.id)}>
                    Desactivar
                  </Button>
                ) : (
                  <Button size="sm" variant="ghost" onClick={() => activate.mutate(skill.id)}>
                    Activar
                  </Button>
                )}
                <Button size="sm" variant="ghost" onClick={() => setDeleteId(skill.id)}>
                  Eliminar
                </Button>
              </div>
            </article>
          ))}
        </div>
      )}
      <ConfirmDialog
        open={Boolean(deleteId)}
        title="Eliminar skill"
        description="La skill dejará de estar disponible para chats y análisis."
        confirmLabel="Eliminar"
        danger
        loading={remove.isPending}
        onConfirm={() => deleteId && remove.mutate(deleteId)}
        onClose={() => setDeleteId(null)}
      />
    </AppPage>
  )
}

export function SkillEditorPage() {
  const { skillId } = useParams<{ skillId: string }>()
  const isNew = !skillId
  const { profile } = useAuth()
  const navigate = useNavigate()
  const toast = useToast()
  const queryClient = useQueryClient()
  const [error, setError] = useState<string | null>(null)

  const skillsQuery = useQuery({ queryKey: queryKeys.skills, queryFn: skillsApi.list })
  const existing = skillsQuery.data?.find((s) => s.id === skillId)

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    values: existing
      ? {
          name: existing.name,
          whenToUse: existing.whenToUse,
          instructions: existing.instructions,
          examples: existing.examples,
          redFlags: existing.redFlags,
          outputFormat: existing.outputFormat,
        }
      : {
          name: '',
          whenToUse: '',
          instructions: '',
          examples: '',
          redFlags: '',
          outputFormat: '',
        },
  })

  const mutation = useMutation({
    mutationFn: (values: FormValues) => {
      if (isNew) {
        if (!profile?.id) throw new Error('Necesitás un perfil profesional verificado.')
        const payload: CreateCustomSkillRequest = { ...values, lawyerProfileId: profile.id }
        return skillsApi.create(payload)
      }
      const payload: UpdateCustomSkillRequest = values
      return skillsApi.update(skillId!, payload)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.skills })
      toast(isNew ? 'Skill creada.' : 'Skill actualizada.')
      navigate('/app/skills')
    },
    onError: (err) => setError(errorMessage(err)),
  })

  return (
    <AppPage>
      <Link to="/app/skills" className="text-[13px] text-blue-600 hover:underline">
        ← Skills
      </Link>
      <h1 className="mt-3 mb-6 text-[28px] font-semibold">{isNew ? 'Nueva skill' : 'Editar skill'}</h1>
      {error ? <Alert className="mb-4">{error}</Alert> : null}
      <form
        className="max-w-2xl space-y-4"
        onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
      >
        <Input label="Nombre" {...form.register('name')} error={form.formState.errors.name?.message} />
        <Textarea
          label="Cuándo usarla"
          hint="En qué situaciones conviene aplicar esta instrucción."
          {...form.register('whenToUse')}
        />
        <Textarea
          label="Instrucciones para la IA"
          hint="Lo que escribas se envía tal cual al modelo. No hay prompts ocultos."
          {...form.register('instructions')}
          error={form.formState.errors.instructions?.message}
        />
        <Textarea
          label="Criterios / señales de alerta"
          hint="Indicá qué aspectos debería priorizar o señalar durante el análisis."
          {...form.register('redFlags')}
        />
        <Textarea label="Ejemplos" {...form.register('examples')} />
        <Textarea label="Formato de respuesta" {...form.register('outputFormat')} />
        <div className="flex gap-2">
          <Button type="button" variant="secondary" onClick={() => navigate('/app/skills')}>
            Cancelar
          </Button>
          <Button type="submit" loading={mutation.isPending}>
            Guardar skill
          </Button>
        </div>
      </form>
    </AppPage>
  )
}
