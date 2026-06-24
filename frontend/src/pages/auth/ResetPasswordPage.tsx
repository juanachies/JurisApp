import { useState, type FormEvent } from 'react'
import { Link, useSearchParams, useNavigate } from 'react-router-dom'
import { authApi } from '@/lib/api'
import type { ApiError } from '@/lib/api'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Alert } from '@/components/ui/Alert'
import { PremiumPanel } from '@/components/ui/Card'

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [token, setToken] = useState(searchParams.get('token') ?? '')
  const [newPassword, setNewPassword] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setIsLoading(true)
    try {
      await authApi.resetPassword(token, newPassword)
      navigate('/login')
    } catch (err) {
      const apiErr = err as ApiError
      setError(apiErr.message)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <PremiumPanel>
      <h1 className="font-heading text-2xl text-foreground">Nueva contraseña</h1>
      <p className="mt-2 text-sm text-muted-foreground">
        Ingresá el token de recuperación y tu nueva contraseña
      </p>

      <form onSubmit={handleSubmit} className="mt-6 space-y-4">
        {error && <Alert variant="error">{error}</Alert>}
        <Input
          label="Token de recuperación"
          value={token}
          onChange={(e) => setToken(e.target.value)}
          required
          helperText="En desarrollo, copiá el token de los logs del servidor"
        />
        <Input
          label="Nueva contraseña"
          type="password"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          required
          minLength={8}
        />
        <Button type="submit" className="w-full" isLoading={isLoading}>
          Restablecer contraseña
        </Button>
      </form>

      <p className="mt-6 text-center text-sm">
        <Link to="/login" className="text-accent-secondary">
          Volver al login
        </Link>
      </p>
    </PremiumPanel>
  )
}
