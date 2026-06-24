import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/lib/auth/AuthContext'
import type { ApiError } from '@/lib/api'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Alert } from '@/components/ui/Alert'
import { PremiumPanel } from '@/components/ui/Card'

export function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
  })
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setIsLoading(true)
    try {
      await register(form)
      navigate('/app/dashboard')
    } catch (err) {
      const apiErr = err as ApiError
      setError(apiErr.message || 'No se pudo crear la cuenta.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <PremiumPanel>
      <h1 className="font-heading text-2xl text-foreground">Crear cuenta</h1>
      <p className="mt-2 text-sm text-muted-foreground">
        Empezá a usar JurisApp en minutos
      </p>

      {error && (
        <Alert variant="error" className="mt-4">
          {error}
        </Alert>
      )}

      <form onSubmit={handleSubmit} className="mt-6 space-y-4">
        <div className="grid gap-4 sm:grid-cols-2">
          <Input
            label="Nombre"
            value={form.firstName}
            onChange={(e) => setForm({ ...form, firstName: e.target.value })}
            required
          />
          <Input
            label="Apellido"
            value={form.lastName}
            onChange={(e) => setForm({ ...form, lastName: e.target.value })}
            required
          />
        </div>
        <Input
          label="Email"
          type="email"
          value={form.email}
          onChange={(e) => setForm({ ...form, email: e.target.value })}
          required
        />
        <Input
          label="Contraseña"
          type="password"
          value={form.password}
          onChange={(e) => setForm({ ...form, password: e.target.value })}
          required
          minLength={8}
          helperText="Mínimo 8 caracteres"
        />
        <Button type="submit" className="w-full" isLoading={isLoading}>
          Registrarse
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-muted-foreground">
        ¿Ya tenés cuenta?{' '}
        <Link to="/login" className="font-medium text-accent-secondary">
          Iniciar sesión
        </Link>
      </p>
    </PremiumPanel>
  )
}
