import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/lib/auth/AuthContext'
import type { ApiError } from '@/lib/api'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Alert } from '@/components/ui/Alert'
import { PremiumPanel } from '@/components/ui/Card'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setIsLoading(true)
    try {
      await login({ email, password })
      navigate('/app/dashboard')
    } catch (err) {
      const apiErr = err as ApiError
      setError(apiErr.message || 'Credenciales inválidas.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <PremiumPanel>
      <h1 className="font-heading text-2xl text-foreground">Iniciar sesión</h1>
      <p className="mt-2 text-sm text-muted-foreground">
        Accedé a tu espacio de trabajo legal
      </p>

      {error && (
        <Alert variant="error" className="mt-4">
          {error}
        </Alert>
      )}

      <form onSubmit={handleSubmit} className="mt-6 space-y-4">
        <Input
          label="Email"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          autoComplete="email"
        />
        <Input
          label="Contraseña"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          autoComplete="current-password"
        />
        <Button type="submit" className="w-full" isLoading={isLoading}>
          Ingresar
        </Button>
      </form>

      <div className="mt-6 text-center text-sm text-muted-foreground">
        <Link to="/forgot-password" className="text-accent-secondary">
          ¿Olvidaste tu contraseña?
        </Link>
        <p className="mt-2">
          ¿No tenés cuenta?{' '}
          <Link to="/register" className="font-medium text-accent-secondary">
            Registrate
          </Link>
        </p>
      </div>
    </PremiumPanel>
  )
}
