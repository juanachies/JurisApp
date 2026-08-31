import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { usersApi, plansApi, billingApi, lawyersApi } from '@/api'
import { errorMessage, fileUrl } from '@/api/client'
import { queryKeys } from '@/api/queryKeys'
import { useAuth } from '@/app/AuthContext'
import { AppPage, QueryError } from '@/components/layout/AppShell'
import { Alert } from '@/components/ui/Alert'
import { Badge } from '@/components/ui/Badge'
import { Button, ButtonLink } from '@/components/ui/Button'
import { ConfirmDialog } from '@/components/ui/Modal'
import { FileDropzone } from '@/components/ui/FileDropzone'
import { Input } from '@/components/ui/Input'
import { PageHeader } from '@/components/ui/PageHeader'
import { Select } from '@/components/ui/Select'
import { useToast } from '@/components/ui/Toast'
import { formatPrice, limitLabel, parseLimits, roleLabel, verificationLabel } from '@/utils/format'
import { isPaidPlan } from '@/utils/permissions'
import type { PlanType, UserTheme } from '@/types/api'

const profileSchema = z.object({
  firstName: z.string().min(1),
  lastName: z.string().min(1),
  theme: z.enum(['Bright', 'Dark']),
})

export function ProfilePage() {
  const { user, profile, refreshUser } = useAuth()
  const toast = useToast()
  const [error, setError] = useState<string | null>(null)
  const form = useForm<{ firstName: string; lastName: string; theme: UserTheme }>({
    resolver: zodResolver(profileSchema),
    values: {
      firstName: user?.firstName ?? '',
      lastName: user?.lastName ?? '',
      theme: user?.theme ?? 'Bright',
    },
  })

  const mutation = useMutation({
    mutationFn: usersApi.updateMe,
    onSuccess: async () => {
      await refreshUser()
      toast('Cambios guardados.')
      setError(null)
    },
    onError: (err) => setError(errorMessage(err)),
  })

  return (
    <AppPage>
      <PageHeader title="Mi perfil" description="Datos de la cuenta. El rol no se edita acá." />
      {error ? <Alert className="mb-4">{error}</Alert> : null}
      <form
        className="max-w-lg space-y-4"
        onSubmit={form.handleSubmit((values) => mutation.mutate(values))}
      >
        <h2 className="text-[16px] font-semibold">Información personal</h2>
        <Input label="Nombre" {...form.register('firstName')} />
        <Input label="Apellido" {...form.register('lastName')} />
        <Input label="Email" value={user?.email ?? ''} disabled />
        <Select label="Tema" {...form.register('theme')}>
          <option value="Bright">Claro</option>
          <option value="Dark">Oscuro</option>
        </Select>
        <Button type="submit" loading={mutation.isPending}>
          Guardar
        </Button>
      </form>

      <section className="mt-10 max-w-lg">
        <h2 className="text-[16px] font-semibold">Rol y verificación</h2>
        <p className="mt-2 text-[14px] text-muted">
          Rol: <span className="text-ink">{user ? roleLabel(user.role) : '—'}</span>
        </p>
        <p className="mt-1 text-[14px] text-muted">
          Verificación:{' '}
          {profile ? verificationLabel(profile.verificationStatus) : 'Sin solicitar'}
        </p>
        <Link to="/app/professional-verification" className="mt-3 inline-block text-[14px] text-blue-600 hover:underline">
          Verificación profesional →
        </Link>
      </section>
    </AppPage>
  )
}

export function SubscriptionPage() {
  const toast = useToast()
  const queryClient = useQueryClient()
  const [error, setError] = useState<string | null>(null)
  const [cancelOpen, setCancelOpen] = useState(false)
  const currentQuery = useQuery({ queryKey: queryKeys.currentPlan, queryFn: plansApi.current })
  const plansQuery = useQuery({ queryKey: queryKeys.plans, queryFn: plansApi.list })
  const current = currentQuery.data
  const limits = current ? parseLimits(current.limitsJson) : null

  const subscribeFree = useMutation({
    mutationFn: (planId: string) => plansApi.subscribe(planId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.currentPlan })
      toast('Suscripción activada.')
    },
    onError: (err) => setError(errorMessage(err)),
  })
  const change = useMutation({
    mutationFn: (planId: string) => plansApi.change(planId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.currentPlan })
      toast('Plan actualizado.')
    },
    onError: (err) => setError(errorMessage(err)),
  })
  const cancel = useMutation({
    mutationFn: plansApi.cancel,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.currentPlan })
      toast('Suscripción cancelada.')
      setCancelOpen(false)
    },
    onError: (err) => setError(errorMessage(err)),
  })
  const checkout = useMutation({
    mutationFn: async (planId: string) => {
      if (import.meta.env.DEV) {
        return billingApi.simulatePurchase({ planId })
      }
      const session = await billingApi.createCheckoutSession({ planId })
      window.location.href = session.url
      return session
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.currentPlan })
      if (import.meta.env.DEV) toast('Compra simulada (entorno de desarrollo).')
    },
    onError: (err) => setError(errorMessage(err)),
  })

  const choose = (planId: string, type: PlanType) => {
    setError(null)
    if (current?.hasActiveSubscription) {
      change.mutate(planId)
      return
    }
    if (type === 'Free') subscribeFree.mutate(planId)
    else checkout.mutate(planId)
  }

  return (
    <AppPage>
      <PageHeader title="Suscripción" description="Tu plan actual y las opciones disponibles." />
      {error ? <Alert className="mb-4">{error}</Alert> : null}
      {currentQuery.isError ? (
        <QueryError message="No pudimos cargar tu plan." onRetry={() => currentQuery.refetch()} />
      ) : (
        <div className="mb-10 rounded-[12px] border border-navy-900 bg-surface p-6">
          <p className="text-[13px] text-muted">Tu plan</p>
          <p className="mt-1 text-[28px] font-semibold">{current?.planName ?? '—'}</p>
          <p className="text-[14px] text-muted">{current ? formatPrice(current.price) : ''}</p>
          {limits ? (
            <ul className="mt-4 space-y-1 text-[14px]">
              <li>Chats: {limitLabel(limits.chats)}</li>
              <li>Documentos: {limitLabel(limits.documents)}</li>
              <li>Tareas con IA: {limitLabel(limits.aiTasks)}</li>
            </ul>
          ) : null}
          {current?.hasActiveSubscription ? (
            <Button className="mt-5" variant="secondary" onClick={() => setCancelOpen(true)}>
              Cancelar suscripción
            </Button>
          ) : (
            <p className="mt-4 text-[13px] text-muted">No tenés una suscripción activa. Estás en el plan Free de referencia.</p>
          )}
        </div>
      )}

      <h2 className="mb-4 text-[20px] font-semibold">Cambiar plan</h2>
      <div className="grid gap-4 md:grid-cols-3">
        {(plansQuery.data ?? []).map((plan) => {
          const lim = parseLimits(plan.limitsJson)
          const currentOne = plan.id === current?.planId && current.hasActiveSubscription
          return (
            <div key={plan.id} className="rounded-[12px] border border-border bg-surface p-5">
              <p className="font-semibold">{plan.name}</p>
              <p className="mt-1 text-[22px] font-semibold">{formatPrice(plan.price)}</p>
              {isPaidPlan(plan.type) ? (
                <p className="text-[12px] text-blue-600">Habilita verificación profesional</p>
              ) : null}
              <ul className="mt-3 space-y-1 text-[13px] text-muted">
                <li>Chats {limitLabel(lim.chats)}</li>
                <li>Documentos {limitLabel(lim.documents)}</li>
                <li>Tareas {limitLabel(lim.aiTasks)}</li>
              </ul>
              <Button
                className="mt-4 w-full"
                variant={plan.type === 'Pro' ? 'primary' : 'secondary'}
                disabled={currentOne}
                loading={checkout.isPending || change.isPending || subscribeFree.isPending}
                onClick={() => choose(plan.id, plan.type)}
              >
                {currentOne ? 'Plan actual' : 'Elegir'}
              </Button>
            </div>
          )
        })}
      </div>
      {import.meta.env.DEV ? (
        <p className="mt-4 text-[12px] text-faint">
          En desarrollo, los planes pagos se activan con una compra simulada (Stripe mock).
        </p>
      ) : null}
      <ConfirmDialog
        open={cancelOpen}
        title="Cancelar suscripción"
        description="Tu suscripción pasará a estado cancelada. Vas a poder volver a suscribirte más adelante."
        confirmLabel="Cancelar suscripción"
        danger
        loading={cancel.isPending}
        onConfirm={() => cancel.mutate()}
        onClose={() => setCancelOpen(false)}
      />
    </AppPage>
  )
}

const verificationSchema = z.object({
  licenseNumber: z.string().min(1, 'La matrícula es obligatoria'),
  barAssociation: z.string().min(1, 'El colegio es obligatorio'),
  province: z.string().min(1, 'La provincia es obligatoria'),
  specialty: z.string().min(1, 'La especialidad es obligatoria'),
})

export function VerificationPage() {
  const { profile, refreshUser } = useAuth()
  const toast = useToast()
  const queryClient = useQueryClient()
  const currentQuery = useQuery({ queryKey: queryKeys.currentPlan, queryFn: plansApi.current })
  const [file, setFile] = useState<File | null>(null)
  const [error, setError] = useState<string | null>(null)
  const hasPro = isPaidPlan(currentQuery.data?.planType) && currentQuery.data?.hasActiveSubscription

  const form = useForm({
    resolver: zodResolver(verificationSchema),
    values: {
      licenseNumber: profile?.licenseNumber ?? '',
      barAssociation: profile?.barAssociation ?? '',
      province: profile?.province ?? '',
      specialty: profile?.specialty ?? '',
    },
  })

  const create = useMutation({
    mutationFn: (values: z.infer<typeof verificationSchema>) => {
      if (!file) throw new Error('Adjuntá el documento de matrícula (JPG, PNG o PDF, máximo 5 MB).')
      return lawyersApi.create({ ...values, licenseDocument: file })
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.lawyerProfile })
      await refreshUser()
      toast('Solicitud enviada.')
      setError(null)
    },
    onError: (err) => setError(errorMessage(err)),
  })

  const update = useMutation({
    mutationFn: lawyersApi.updateMe,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.lawyerProfile })
      toast('Solicitud actualizada.')
    },
    onError: (err) => setError(errorMessage(err)),
  })

  const status = profile?.verificationStatus ?? 'NotSubmitted'

  return (
    <AppPage>
      <PageHeader
        title="Verificación profesional"
        description="Para gestionar casos y skills necesitás un plan Pro o Max y la aprobación de un administrador."
      />

      {!hasPro && status !== 'Verified' ? (
        <div className="max-w-xl rounded-[12px] border border-border bg-surface p-6">
          <p className="text-[15px] font-medium">Para solicitar la verificación como abogado necesitás un plan Pro activo.</p>
          <ButtonLink to="/app/subscription" className="mt-4">
            Ver planes
          </ButtonLink>
        </div>
      ) : null}

      {status === 'Pending' ? (
        <div className="max-w-xl rounded-[12px] border border-border bg-surface p-6">
          <Badge tone="warning">Pendiente</Badge>
          <h2 className="mt-3 text-[18px] font-semibold">Verificación en revisión</h2>
          <p className="mt-2 text-[14px] text-muted">Recibimos tu solicitud. Te vamos a notificar cuando haya una resolución.</p>
        </div>
      ) : null}

      {status === 'Verified' ? (
        <div className="max-w-xl rounded-[12px] border border-border bg-surface p-6">
          <Badge tone="success">Verificado</Badge>
          <h2 className="mt-3 text-[18px] font-semibold">Perfil profesional verificado</h2>
          <dl className="mt-4 space-y-2 text-[14px]">
            <div>Matrícula: {profile?.licenseNumber}</div>
            <div>Colegio: {profile?.barAssociation}</div>
            <div>Provincia: {profile?.province}</div>
            <div>Especialidad: {profile?.specialty}</div>
          </dl>
        </div>
      ) : null}

      {status === 'Rejected' ? (
        <Alert variant="warning" className="mb-6 max-w-xl">
          Solicitud rechazada
          {profile?.rejectionReason ? `: ${profile.rejectionReason}` : '.'} Podés corregir los datos y volver a
          enviar el documento.
        </Alert>
      ) : null}

      {hasPro && (status === 'NotSubmitted' || status === 'Rejected') ? (
        <form
          className="mt-6 max-w-xl space-y-4"
          onSubmit={form.handleSubmit((values) => create.mutate(values))}
        >
          {error ? <Alert>{error}</Alert> : null}
          <Input label="Matrícula" {...form.register('licenseNumber')} error={form.formState.errors.licenseNumber?.message} />
          <Input label="Colegio de abogados" {...form.register('barAssociation')} error={form.formState.errors.barAssociation?.message} />
          <Input label="Provincia" {...form.register('province')} error={form.formState.errors.province?.message} />
          <Input label="Especialidad" {...form.register('specialty')} error={form.formState.errors.specialty?.message} />
          <FileDropzone
            file={file}
            onFile={setFile}
            accept="image/jpeg,image/png,application/pdf"
            hint="JPG, PNG o PDF · máximo 5 MB"
          />
          <Button type="submit" loading={create.isPending}>
            Enviar solicitud
          </Button>
        </form>
      ) : null}

      {hasPro && status === 'Pending' ? (
        <form
          className="mt-6 max-w-xl space-y-4"
          onSubmit={form.handleSubmit((values) => update.mutate(values))}
        >
          <Input label="Matrícula" {...form.register('licenseNumber')} />
          <Input label="Colegio de abogados" {...form.register('barAssociation')} />
          <Input label="Provincia" {...form.register('province')} />
          <Input label="Especialidad" {...form.register('specialty')} />
          <Button type="submit" loading={update.isPending} variant="secondary">
            Actualizar datos
          </Button>
        </form>
      ) : null}

      {profile?.licenseDocumentUrl ? (
        <p className="mt-4 text-[13px]">
          <a className="text-blue-600 hover:underline" href={fileUrl(profile.licenseDocumentUrl) ?? undefined} target="_blank" rel="noreferrer">
            Ver documentación presentada
          </a>
        </p>
      ) : null}
    </AppPage>
  )
}

export function BillingSuccessPage() {
  return (
    <AppPage>
      <h1 className="text-[28px] font-semibold">Pago recibido</h1>
      <p className="mt-2 text-[14px] text-muted">
        Si el cobro se confirmó, tu plan debería actualizarse en unos instantes.
      </p>
      <ButtonLink to="/app/subscription" className="mt-6">
        Ver suscripción
      </ButtonLink>
    </AppPage>
  )
}
