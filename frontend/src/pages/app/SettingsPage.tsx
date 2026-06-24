import { useState, useEffect, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { usersApi, lawyerProfilesApi } from '@/lib/api'
import { useAuth } from '@/lib/auth/AuthContext'
import type { ApiError } from '@/lib/api'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Card } from '@/components/ui/Card'
import { Tabs, TabPanel } from '@/components/ui/Tabs'
import { Alert } from '@/components/ui/Alert'

export function SettingsPage() {
  const { user, refreshUser } = useAuth()
  const queryClient = useQueryClient()
  const [tab, setTab] = useState('profile')
  const [profileForm, setProfileForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
  })
  const [lawyerForm, setLawyerForm] = useState({
    licenseNumber: '',
    barAssociation: '',
    province: '',
    specialty: '',
  })
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    if (user) {
      setProfileForm({
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
      })
    }
  }, [user])

  const { data: lawyerProfile } = useQuery({
    queryKey: ['lawyer-profile'],
    queryFn: lawyerProfilesApi.getMe,
    retry: false,
  })

  useEffect(() => {
    if (lawyerProfile) {
      setLawyerForm({
        licenseNumber: lawyerProfile.licenseNumber,
        barAssociation: lawyerProfile.barAssociation,
        province: lawyerProfile.province,
        specialty: lawyerProfile.specialty,
      })
    }
  }, [lawyerProfile])

  const updateProfileMutation = useMutation({
    mutationFn: () => usersApi.updateMe(profileForm),
    onSuccess: async () => {
      await refreshUser()
      setMessage('Perfil actualizado.')
      setError('')
    },
    onError: (err: ApiError) => setError(err.message),
  })

  const saveLawyerMutation = useMutation({
    mutationFn: () =>
      lawyerProfile
        ? lawyerProfilesApi.update(lawyerForm)
        : lawyerProfilesApi.create(lawyerForm),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['lawyer-profile'] })
      setMessage('Perfil de abogado guardado.')
      setError('')
    },
    onError: (err: ApiError) => setError(err.message),
  })

  const handleProfileSubmit = (e: FormEvent) => {
    e.preventDefault()
    updateProfileMutation.mutate()
  }

  const handleLawyerSubmit = (e: FormEvent) => {
    e.preventDefault()
    saveLawyerMutation.mutate()
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div>
        <h2 className="font-heading text-2xl text-foreground">Configuración</h2>
        <p className="text-sm text-muted-foreground">Gestioná tu cuenta y perfil profesional</p>
      </div>

      {message && <Alert variant="success">{message}</Alert>}
      {error && <Alert variant="error">{error}</Alert>}

      <Tabs
        tabs={[
          { id: 'profile', label: 'Perfil' },
          { id: 'lawyer', label: 'Abogado' },
          { id: 'security', label: 'Seguridad' },
        ]}
        activeTab={tab}
        onChange={setTab}
      />

      <TabPanel id="profile" activeTab={tab}>
        <Card>
          <form onSubmit={handleProfileSubmit} className="space-y-4">
            <Input
              label="Nombre"
              value={profileForm.firstName}
              onChange={(e) => setProfileForm({ ...profileForm, firstName: e.target.value })}
              required
            />
            <Input
              label="Apellido"
              value={profileForm.lastName}
              onChange={(e) => setProfileForm({ ...profileForm, lastName: e.target.value })}
              required
            />
            <Input
              label="Email"
              type="email"
              value={profileForm.email}
              onChange={(e) => setProfileForm({ ...profileForm, email: e.target.value })}
              required
            />
            <Button type="submit" isLoading={updateProfileMutation.isPending}>
              Guardar cambios
            </Button>
          </form>
        </Card>
      </TabPanel>

      <TabPanel id="lawyer" activeTab={tab}>
        <Card>
          <form onSubmit={handleLawyerSubmit} className="space-y-4">
            <Input
              label="Número de matrícula"
              value={lawyerForm.licenseNumber}
              onChange={(e) => setLawyerForm({ ...lawyerForm, licenseNumber: e.target.value })}
              required
            />
            <Input
              label="Colegio de abogados"
              value={lawyerForm.barAssociation}
              onChange={(e) => setLawyerForm({ ...lawyerForm, barAssociation: e.target.value })}
              required
            />
            <Input
              label="Provincia"
              value={lawyerForm.province}
              onChange={(e) => setLawyerForm({ ...lawyerForm, province: e.target.value })}
              required
            />
            <Input
              label="Especialidad"
              value={lawyerForm.specialty}
              onChange={(e) => setLawyerForm({ ...lawyerForm, specialty: e.target.value })}
              required
            />
            <Button type="submit" isLoading={saveLawyerMutation.isPending}>
              {lawyerProfile ? 'Actualizar perfil' : 'Crear perfil de abogado'}
            </Button>
          </form>
        </Card>
      </TabPanel>

      <TabPanel id="security" activeTab={tab}>
        <Card className="space-y-4">
          <p className="text-sm text-muted-foreground">
            Para cambiar tu contraseña, solicitá un enlace de recuperación.
          </p>
          <Link to="/forgot-password">
            <Button variant="secondary">Recuperar contraseña</Button>
          </Link>
        </Card>
      </TabPanel>
    </div>
  )
}
