import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { lawyersApi, plansApi, usersApi } from '@/api'
import { errorMessage, fileUrl } from '@/api/client'
import { queryKeys } from '@/api/queryKeys'
import { AppPage, QueryError } from '@/components/layout/AppShell'
import { Alert } from '@/components/ui/Alert'
import { Badge } from '@/components/ui/Badge'
import { Button } from '@/components/ui/Button'
import { ConfirmDialog, Modal } from '@/components/ui/Modal'
import { Input } from '@/components/ui/Input'
import { PageHeader } from '@/components/ui/PageHeader'
import { Select } from '@/components/ui/Select'
import { Table, TBody, Td, Th, THead, Tr } from '@/components/ui/Table'
import { TableSkeleton } from '@/components/ui/Loading'
import { Textarea } from '@/components/ui/Textarea'
import { useToast } from '@/components/ui/Toast'
import { formatDate, formatPrice, fullName, roleLabel, verificationLabel } from '@/utils/format'
import type { LawyerVerificationStatus, PlanType, UserRole } from '@/types/api'

export function AdminHomePage() {
  const users = useQuery({ queryKey: queryKeys.users, queryFn: usersApi.list })
  const requests = useQuery({ queryKey: queryKeys.verifications(), queryFn: () => lawyersApi.listRequests() })
  const plans = useQuery({ queryKey: queryKeys.plans, queryFn: plansApi.list })
  const pending = (requests.data ?? []).filter((r) => r.verificationStatus === 'Pending').length

  return (
    <AppPage>
      <PageHeader title="Administración" description="Gestión de cuentas, verificaciones y planes." />
      <div className="grid gap-4 md:grid-cols-3">
        <AdminLink to="/admin/users" title="Usuarios" body={`${users.data?.length ?? '—'} cuentas`} hint="Gestionar cuentas" />
        <AdminLink to="/admin/verifications" title="Verificaciones" body={`${pending} pendientes`} hint="Revisar solicitudes profesionales" />
        <AdminLink to="/admin/plans" title="Planes" body={`${plans.data?.length ?? '—'} planes`} hint="Administrar suscripciones disponibles" />
      </div>
    </AppPage>
  )
}

function AdminLink({ to, title, body, hint }: { to: string; title: string; body: string; hint: string }) {
  return (
    <Link to={to} className="rounded-[12px] border border-border bg-surface p-5 hover:bg-subtle">
      <p className="text-[13px] text-muted">{title}</p>
      <p className="mt-2 text-[22px] font-semibold">{body}</p>
      <p className="mt-1 text-[13px] text-blue-600">{hint}</p>
    </Link>
  )
}

export function AdminUsersPage() {
  const navigate = useNavigate()
  const [q, setQ] = useState('')
  const usersQuery = useQuery({ queryKey: queryKeys.users, queryFn: usersApi.list })
  const users = (usersQuery.data ?? []).filter((u) => {
    const hay = `${u.firstName} ${u.lastName} ${u.email}`.toLowerCase()
    return hay.includes(q.toLowerCase())
  })

  return (
    <AppPage>
      <PageHeader title="Usuarios" />
      <input
        value={q}
        onChange={(e) => setQ(e.target.value)}
        placeholder="Buscar"
        className="mb-4 h-10 max-w-sm rounded-[8px] border border-border-strong px-3 text-[14px]"
      />
      {usersQuery.isLoading ? <TableSkeleton /> : null}
      {usersQuery.isError ? (
        <QueryError message="No pudimos cargar los usuarios." onRetry={() => usersQuery.refetch()} />
      ) : !usersQuery.isLoading ? (
        <Table>
          <THead>
            <tr>
              <Th>Usuario</Th>
              <Th>Email</Th>
              <Th>Rol</Th>
              <Th>Estado</Th>
            </tr>
          </THead>
          <TBody>
            {users.map((user) => (
              <Tr key={user.id} onClick={() => navigate(`/admin/users/${user.id}`)}>
                <Td className="font-medium">{fullName(user.firstName, user.lastName)}</Td>
                <Td>{user.email}</Td>
                <Td>{roleLabel(user.role)}</Td>
                <Td>
                  <Badge tone={user.isActive ? 'success' : 'neutral'}>{user.isActive ? 'Activo' : 'Inactivo'}</Badge>
                </Td>
              </Tr>
            ))}
          </TBody>
        </Table>
      ) : null}
    </AppPage>
  )
}

export function AdminUserDetailPage() {
  const { userId } = useParams<{ userId: string }>()
  const toast = useToast()
  const queryClient = useQueryClient()
  const [error, setError] = useState<string | null>(null)
  const userQuery = useQuery({
    queryKey: queryKeys.user(userId ?? ''),
    queryFn: () => usersApi.getById(userId!),
    enabled: Boolean(userId),
  })
  const user = userQuery.data

  const update = useMutation({
    mutationFn: (data: { role?: UserRole; isActive?: boolean }) => usersApi.adminUpdate(userId!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.users })
      queryClient.invalidateQueries({ queryKey: queryKeys.user(userId!) })
      toast('Usuario actualizado.')
      setError(null)
    },
    onError: (err) => setError(errorMessage(err)),
  })

  return (
    <AppPage>
      <Link to="/admin/users" className="text-[13px] text-blue-600 hover:underline">
        ← Usuarios
      </Link>
      {userQuery.isError ? <QueryError message="No pudimos cargar el usuario." /> : null}
      <h1 className="mt-3 text-[28px] font-semibold">{user ? fullName(user.firstName, user.lastName) : 'Usuario'}</h1>
      <p className="text-[14px] text-muted">{user?.email}</p>
      {error ? <Alert className="mt-4">{error}</Alert> : null}
      {user ? (
        <div className="mt-6 max-w-md space-y-4">
          <Select
            label="Rol"
            value={user.role}
            onChange={(e) => update.mutate({ role: e.target.value as UserRole })}
          >
            <option value="User">Usuario</option>
            <option value="Lawyer">Abogado</option>
            <option value="Admin">Administrador</option>
          </Select>
          <p className="text-[12px] text-muted">
            El backend solo permite asignar Abogado si la persona ya tiene perfil verificado.
          </p>
          <Button
            variant="secondary"
            loading={update.isPending}
            onClick={() => update.mutate({ isActive: !user.isActive })}
          >
            {user.isActive ? 'Desactivar cuenta' : 'Activar cuenta'}
          </Button>
        </div>
      ) : null}
    </AppPage>
  )
}

export function AdminVerificationsPage() {
  const navigate = useNavigate()
  const [status, setStatus] = useState('')
  const query = useQuery({
    queryKey: queryKeys.verifications(status || undefined),
    queryFn: () =>
      lawyersApi.listRequests(status ? (status as LawyerVerificationStatus) : undefined),
  })

  return (
    <AppPage>
      <PageHeader title="Solicitudes de verificación" />
      <Select
        className="mb-4 max-w-xs"
        value={status}
        onChange={(e) => setStatus(e.target.value)}
        label="Estado"
      >
        <option value="">Todas</option>
        <option value="Pending">Pendientes</option>
        <option value="Verified">Verificadas</option>
        <option value="Rejected">Rechazadas</option>
      </Select>
      {query.isLoading ? <TableSkeleton /> : null}
      {query.isError ? <QueryError message="No pudimos cargar las solicitudes." onRetry={() => query.refetch()} /> : null}
      {!query.isLoading ? (
        <Table>
          <THead>
            <tr>
              <Th>Usuario</Th>
              <Th>Matrícula</Th>
              <Th>Fecha</Th>
              <Th>Estado</Th>
            </tr>
          </THead>
          <TBody>
            {(query.data ?? []).map((row) => (
              <Tr key={row.id} onClick={() => navigate(`/admin/verifications/${row.id}`)}>
                <Td>
                  <p className="font-medium">{fullName(row.userFirstName, row.userLastName)}</p>
                  <p className="text-[12px] text-muted">{row.userEmail}</p>
                </Td>
                <Td>{row.licenseNumber}</Td>
                <Td>{formatDate(row.createdAt)}</Td>
                <Td>{verificationLabel(row.verificationStatus)}</Td>
              </Tr>
            ))}
          </TBody>
        </Table>
      ) : null}
    </AppPage>
  )
}

export function AdminVerificationDetailPage() {
  const { requestId } = useParams<{ requestId: string }>()
  const toast = useToast()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [rejectOpen, setRejectOpen] = useState(false)
  const [approveOpen, setApproveOpen] = useState(false)
  const [reason, setReason] = useState('')
  const [error, setError] = useState<string | null>(null)

  const detail = useQuery({
    queryKey: queryKeys.verification(requestId ?? ''),
    queryFn: () => lawyersApi.getRequest(requestId!),
    enabled: Boolean(requestId),
  })
  const req = detail.data

  const approve = useMutation({
    mutationFn: () => lawyersApi.approve(requestId!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'verifications'] })
      toast('Solicitud aprobada.')
      navigate('/admin/verifications')
    },
    onError: (err) => setError(errorMessage(err)),
  })
  const reject = useMutation({
    mutationFn: () => lawyersApi.reject(requestId!, { reason }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin', 'verifications'] })
      toast('Solicitud rechazada.')
      navigate('/admin/verifications')
    },
    onError: (err) => setError(errorMessage(err)),
  })

  return (
    <AppPage>
      <Link to="/admin/verifications" className="text-[13px] text-blue-600 hover:underline">
        ← Verificaciones
      </Link>
      {detail.isError ? <QueryError message="No pudimos cargar la solicitud." /> : null}
      <h1 className="mt-3 text-[28px] font-semibold">
        {req ? fullName(req.userFirstName, req.userLastName) : 'Solicitud'}
      </h1>
      {error ? <Alert className="mt-4">{error}</Alert> : null}
      {req ? (
        <div className="mt-6 max-w-xl space-y-6">
          <section>
            <h2 className="text-[16px] font-semibold">Información del usuario</h2>
            <p className="mt-2 text-[14px]">{req.userEmail}</p>
          </section>
          <section>
            <h2 className="text-[16px] font-semibold">Información profesional</h2>
            <dl className="mt-2 space-y-1 text-[14px]">
              <div>Matrícula: {req.licenseNumber}</div>
              <div>Colegio: {req.barAssociation}</div>
              <div>Provincia: {req.province}</div>
              <div>Especialidad: {req.specialty}</div>
              <div>Estado: {verificationLabel(req.verificationStatus)}</div>
            </dl>
          </section>
          <section>
            <h2 className="text-[16px] font-semibold">Documentación presentada</h2>
            {req.licenseDocumentUrl ? (
              <a className="mt-2 inline-block text-[14px] text-blue-600 hover:underline" href={fileUrl(req.licenseDocumentUrl) ?? undefined} target="_blank" rel="noreferrer">
                Abrir documento
              </a>
            ) : (
              <p className="mt-2 text-[14px] text-muted">Sin archivo</p>
            )}
          </section>
          {req.verificationStatus === 'Pending' ? (
            <div className="flex gap-2">
              <Button variant="secondary" onClick={() => setRejectOpen(true)}>
                Rechazar
              </Button>
              <Button onClick={() => setApproveOpen(true)}>Aprobar</Button>
            </div>
          ) : null}
        </div>
      ) : null}

      <ConfirmDialog
        open={approveOpen}
        title="Aprobar verificación"
        description="La cuenta pasará a rol de abogado verificado y podrá gestionar casos y skills."
        confirmLabel="Aprobar"
        loading={approve.isPending}
        onConfirm={() => approve.mutate()}
        onClose={() => setApproveOpen(false)}
      />
      <Modal
        open={rejectOpen}
        title="Rechazar solicitud"
        onClose={() => setRejectOpen(false)}
        footer={
          <>
            <Button variant="secondary" onClick={() => setRejectOpen(false)}>
              Volver
            </Button>
            <Button variant="danger" loading={reject.isPending} onClick={() => reject.mutate()}>
              Rechazar
            </Button>
          </>
        }
      >
        <Textarea
          label="Motivo (opcional)"
          value={reason}
          onChange={(e) => setReason(e.target.value)}
        />
      </Modal>
    </AppPage>
  )
}

const planSchema = z.object({
  name: z.string().min(1),
  type: z.enum(['Free', 'Pro', 'Max']),
  price: z.coerce.number().min(0),
  chats: z.coerce.number(),
  documents: z.coerce.number(),
  aiTasks: z.coerce.number(),
})

export function AdminPlansPage() {
  const toast = useToast()
  const queryClient = useQueryClient()
  const [open, setOpen] = useState(false)
  const [editId, setEditId] = useState<string | null>(null)
  const [deleteId, setDeleteId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const plansQuery = useQuery({ queryKey: queryKeys.plans, queryFn: plansApi.list })
  const editing = plansQuery.data?.find((p) => p.id === editId)

  const form = useForm({
    resolver: zodResolver(planSchema),
    values: editing
      ? {
          name: editing.name,
          type: editing.type,
          price: editing.price,
          ...parseLimitsSafe(editing.limitsJson),
        }
      : { name: '', type: 'Free' as PlanType, price: 0, chats: 5, documents: 10, aiTasks: 3 },
  })

  const save = useMutation({
    mutationFn: (values: z.infer<typeof planSchema>) => {
      const payload = {
        name: values.name,
        type: values.type,
        price: values.price,
        limitsJson: JSON.stringify({
          chats: values.chats,
          documents: values.documents,
          aiTasks: values.aiTasks,
        }),
      }
      return editId ? plansApi.update(editId, payload) : plansApi.create(payload)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.plans })
      toast(editId ? 'Plan actualizado.' : 'Plan creado.')
      setOpen(false)
      setEditId(null)
    },
    onError: (err) => setError(errorMessage(err)),
  })

  const remove = useMutation({
    mutationFn: (id: string) => plansApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.plans })
      toast('Plan eliminado.')
      setDeleteId(null)
    },
    onError: (err) => setError(errorMessage(err)),
  })

  return (
    <AppPage>
      <PageHeader title="Planes" actions={<Button onClick={() => { setEditId(null); setOpen(true) }}>Nuevo plan</Button>} />
      {error ? <Alert className="mb-4">{error}</Alert> : null}
      {plansQuery.isLoading ? <TableSkeleton /> : null}
      <Table>
        <THead>
          <tr>
            <Th>Nombre</Th>
            <Th>Tipo</Th>
            <Th>Precio</Th>
            <Th>Límites</Th>
            <Th></Th>
          </tr>
        </THead>
        <TBody>
          {(plansQuery.data ?? []).map((plan) => (
            <Tr key={plan.id}>
              <Td className="font-medium">{plan.name}</Td>
              <Td>{plan.type}</Td>
              <Td>{formatPrice(plan.price)}</Td>
              <Td className="text-[12px] text-muted">{plan.limitsJson}</Td>
              <Td>
                <button type="button" className="mr-3 text-[13px] text-blue-600" onClick={() => { setEditId(plan.id); setOpen(true) }}>
                  Editar
                </button>
                <button type="button" className="text-[13px] text-danger" onClick={() => setDeleteId(plan.id)}>
                  Eliminar
                </button>
              </Td>
            </Tr>
          ))}
        </TBody>
      </Table>

      <Modal
        open={open}
        title={editId ? 'Editar plan' : 'Nuevo plan'}
        onClose={() => setOpen(false)}
        footer={
          <>
            <Button variant="secondary" onClick={() => setOpen(false)}>
              Cancelar
            </Button>
            <Button loading={save.isPending} onClick={form.handleSubmit((v) => save.mutate(v))}>
              Guardar
            </Button>
          </>
        }
      >
        <div className="space-y-3">
          <Input label="Nombre" {...form.register('name')} />
          <Select label="Tipo" {...form.register('type')}>
            <option value="Free">Free</option>
            <option value="Pro">Pro</option>
            <option value="Max">Max</option>
          </Select>
          <Input label="Precio (USD)" type="number" step="0.01" {...form.register('price')} />
          <Input label="Límite de chats (-1 ilimitado)" type="number" {...form.register('chats')} />
          <Input label="Límite de documentos" type="number" {...form.register('documents')} />
          <Input label="Límite de tareas IA" type="number" {...form.register('aiTasks')} />
        </div>
      </Modal>
      <ConfirmDialog
        open={Boolean(deleteId)}
        title="Eliminar plan"
        description="No se puede eliminar si hay suscripciones asociadas."
        confirmLabel="Eliminar"
        danger
        loading={remove.isPending}
        onConfirm={() => deleteId && remove.mutate(deleteId)}
        onClose={() => setDeleteId(null)}
      />
    </AppPage>
  )
}

function parseLimitsSafe(json: string) {
  try {
    const parsed = JSON.parse(json) as { chats?: number; documents?: number; aiTasks?: number }
    return {
      chats: parsed.chats ?? 0,
      documents: parsed.documents ?? 0,
      aiTasks: parsed.aiTasks ?? 0,
    }
  } catch {
    return { chats: 0, documents: 0, aiTasks: 0 }
  }
}
