import { useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { authApi } from '@/api'
import { errorMessage } from '@/api/client'
import { AuthSplit } from '@/components/layout/PublicChrome'
import { Alert } from '@/components/ui/Alert'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'

const schema = z.object({
  newPassword: z.string().min(8, 'La contraseña debe tener al menos 8 caracteres'),
})

export function ResetPasswordPage() {
  const [params] = useSearchParams()
  const token = params.get('token') ?? ''
  const [error, setError] = useState<string | null>(null)
  const [done, setDone] = useState(false)
  const form = useForm<{ newPassword: string }>({
    resolver: zodResolver(schema),
    defaultValues: { newPassword: '' },
  })

  return (
    <AuthSplit title="Nueva contraseña" subtitle="Elegí una contraseña de al menos 8 caracteres.">
      {!token ? <Alert className="mb-4">El enlace no incluye un token válido.</Alert> : null}
      {error ? <Alert className="mb-4">{error}</Alert> : null}
      {done ? (
        <Alert variant="success" className="mb-4">
          Contraseña actualizada.{' '}
          <Link to="/login" className="underline">
            Iniciar sesión
          </Link>
        </Alert>
      ) : null}
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit(async (values) => {
          setError(null)
          try {
            await authApi.resetPassword({ token, newPassword: values.newPassword })
            setDone(true)
          } catch (err) {
            setError(errorMessage(err))
          }
        })}
      >
        <Input
          label="Nueva contraseña"
          type="password"
          autoComplete="new-password"
          disabled={!token || done}
          {...form.register('newPassword')}
          error={form.formState.errors.newPassword?.message}
        />
        <Button type="submit" className="w-full" loading={form.formState.isSubmitting} disabled={!token || done}>
          Guardar contraseña
        </Button>
      </form>
    </AuthSplit>
  )
}
