import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { usersApi } from '@/lib/api'
import { useAuth } from '@/lib/auth/AuthContext'
import type { UserDto, UserRole } from '@/lib/api'
import { Button } from '@/components/ui/Button'
import { Badge, StatusBadge } from '@/components/ui/Badge'
import { Table, TableRow, TableCell, DataList, DataListItem } from '@/components/ui/Table'
import { Modal, ConfirmDialog } from '@/components/ui/Modal'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { Skeleton } from '@/components/ui/Loading'
import { formatDate } from '@/lib/utils/format'

export function AdminUsersPage() {
  const { user: currentUser } = useAuth()
  const queryClient = useQueryClient()
  const [editingUser, setEditingUser] = useState<UserDto | null>(null)
  const [deleteId, setDeleteId] = useState<string | null>(null)
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    role: '' as UserRole | '',
    isActive: '' as string,
  })

  const { data: users, isLoading } = useQuery({
    queryKey: ['users', 'admin'],
    queryFn: usersApi.list,
  })

  const updateMutation = useMutation({
    mutationFn: () => {
      const body: Record<string, unknown> = {}
      if (form.firstName) body.firstName = form.firstName
      if (form.lastName) body.lastName = form.lastName
      if (form.email) body.email = form.email
      if (form.role) body.role = form.role
      if (form.isActive !== '') body.isActive = form.isActive === 'true'
      return usersApi.adminUpdate(editingUser!.id, body)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] })
      setEditingUser(null)
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => usersApi.adminDelete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['users'] })
      setDeleteId(null)
    },
  })

  const openEdit = (u: UserDto) => {
    setEditingUser(u)
    setForm({
      firstName: u.firstName,
      lastName: u.lastName,
      email: u.email,
      role: u.role,
      isActive: String(u.isActive),
    })
  }

  const isSelf = (id: string) => currentUser?.id === id

  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <div>
        <h2 className="font-heading text-2xl text-foreground">Administración de usuarios</h2>
        <p className="text-sm text-muted-foreground">Gestioná cuentas, roles y estados</p>
      </div>

      {isLoading ? (
        <Skeleton className="h-64" />
      ) : (
        <>
          <Table headers={['Nombre', 'Email', 'Rol', 'Estado', 'Creado', 'Acciones']}>
            {users?.map((u) => (
              <TableRow key={u.id}>
                <TableCell>
                  {u.firstName} {u.lastName}
                </TableCell>
                <TableCell>{u.email}</TableCell>
                <TableCell>
                  <Badge variant="info">{u.role}</Badge>
                </TableCell>
                <TableCell>
                  <StatusBadge
                    status={u.isActive ? 'Active' : 'Inactive'}
                    label={u.isActive ? 'Activo' : 'Inactivo'}
                  />
                </TableCell>
                <TableCell className="text-muted-foreground text-xs">
                  {formatDate(u.createdAt)}
                </TableCell>
                <TableCell>
                  {!isSelf(u.id) && (
                    <div className="flex gap-2">
                      <Button variant="ghost" size="sm" onClick={() => openEdit(u)}>
                        Editar
                      </Button>
                      <Button variant="ghost" size="sm" onClick={() => setDeleteId(u.id)}>
                        Eliminar
                      </Button>
                    </div>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </Table>

          <DataList>
            {users?.map((u) => (
              <DataListItem key={u.id}>
                <p className="font-medium">
                  {u.firstName} {u.lastName}
                </p>
                <p className="text-sm text-muted-foreground">{u.email}</p>
                <div className="mt-2 flex gap-2">
                  <Badge variant="info">{u.role}</Badge>
                  <StatusBadge
                    status={u.isActive ? 'Active' : 'Inactive'}
                    label={u.isActive ? 'Activo' : 'Inactivo'}
                  />
                </div>
                {!isSelf(u.id) && (
                  <div className="mt-3 flex gap-2">
                    <Button variant="secondary" size="sm" onClick={() => openEdit(u)}>
                      Editar
                    </Button>
                    <Button variant="ghost" size="sm" onClick={() => setDeleteId(u.id)}>
                      Eliminar
                    </Button>
                  </div>
                )}
              </DataListItem>
            ))}
          </DataList>
        </>
      )}

      <Modal
        isOpen={!!editingUser}
        onClose={() => setEditingUser(null)}
        title="Editar usuario"
      >
        <form
          onSubmit={(e) => { e.preventDefault(); updateMutation.mutate() }}
          className="space-y-4"
        >
          <Input
            label="Nombre"
            value={form.firstName}
            onChange={(e) => setForm({ ...form, firstName: e.target.value })}
          />
          <Input
            label="Apellido"
            value={form.lastName}
            onChange={(e) => setForm({ ...form, lastName: e.target.value })}
          />
          <Input
            label="Email"
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
          />
          <Select
            label="Rol"
            value={form.role}
            onChange={(e) => setForm({ ...form, role: e.target.value as UserRole })}
          >
            <option value="User">User</option>
            <option value="Lawyer">Lawyer</option>
            <option value="Admin">Admin</option>
          </Select>
          <Select
            label="Estado"
            value={form.isActive}
            onChange={(e) => setForm({ ...form, isActive: e.target.value })}
          >
            <option value="true">Activo</option>
            <option value="false">Inactivo</option>
          </Select>
          <div className="flex justify-end gap-3">
            <Button variant="secondary" type="button" onClick={() => setEditingUser(null)}>
              Cancelar
            </Button>
            <Button type="submit" isLoading={updateMutation.isPending}>
              Guardar
            </Button>
          </div>
        </form>
      </Modal>

      <ConfirmDialog
        isOpen={!!deleteId}
        onClose={() => setDeleteId(null)}
        onConfirm={() => deleteId && deleteMutation.mutate(deleteId)}
        title="Eliminar usuario"
        message="¿Eliminar este usuario permanentemente?"
        confirmLabel="Eliminar"
        variant="danger"
        isLoading={deleteMutation.isPending}
      />
    </div>
  )
}
