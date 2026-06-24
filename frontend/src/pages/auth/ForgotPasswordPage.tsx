import { useState, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { authApi } from '@/lib/api'
import type { ApiError } from '@/lib/api'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Alert } from '@/components/ui/Alert'
import { PremiumPanel } from '@/components/ui/Card'

export function ForgotPasswordPage() {
  const [email, setEmail] = useState('')
  const [sent, setSent] = useState(false)
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setIsLoading(true)
    try {
      await authApi.forgotPassword(email)
      setSent(true)
    } catch (err) {
      const apiErr = err as ApiError
      setError(apiErr.message)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <PremiumPanel>
      <h1 className="font-heading text-2xl text-foreground">Recuperar acceso</h1>
      <p className="mt-2 text-sm text-muted-foreground">
        Te enviaremos instrucciones si el email está registrado
      </p>

      {sent ? (
        <Alert variant="success" className="mt-6">
          Si el email existe en nuestro sistema, recibirás instrucciones para restablecer tu
          contraseña. En desarrollo, revisá los logs del servidor.
        </Alert>
      ) : (
        <form onSubmit={handleSubmit} className="mt-6 space-y-4">
          {error && <Alert variant="error">{error}</Alert>}
          <Input
            label="Email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
          <Button type="submit" className="w-full" isLoading={isLoading}>
            Enviar instrucciones
          </Button>
        </form>
      )}

      <p className="mt-6 text-center text-sm">
        <Link to="/login" className="text-accent-secondary">
          Volver al login
        </Link>
      </p>
    </PremiumPanel>
  )
}
