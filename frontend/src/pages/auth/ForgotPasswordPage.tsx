import { useState } from 'react'
import { Link } from 'react-router-dom'
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
  email: z.string().min(1, 'El email es obligatorio'),
})

export function ForgotPasswordPage() {
  const [error, setError] = useState<string | null>(null)
  const [done, setDone] = useState(false)
  const form = useForm<{ email: string }>({
    resolver: zodResolver(schema),
    defaultValues: { email: '' },
  })

  return (
    <AuthSplit title="Recuperar acceso" subtitle="Te vamos a enviar un enlace para restablecer la contraseña.">
      {error ? <Alert className="mb-4">{error}</Alert> : null}
      {done ? (
        <Alert variant="success" className="mb-4">
          Si el email está registrado, vas a recibir un enlace. En desarrollo el enlace aparece en la consola
          del servidor.
        </Alert>
      ) : null}
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit(async (values) => {
          setError(null)
          try {
            await authApi.forgotPassword(values)
            setDone(true)
          } catch (err) {
            setError(errorMessage(err))
          }
        })}
      >
        <Input label="Email" type="email" {...form.register('email')} error={form.formState.errors.email?.message} />
        <Button type="submit" className="w-full" loading={form.formState.isSubmitting}>
          Enviar enlace
        </Button>
      </form>
      <p className="mt-4 text-[14px]">
        <Link to="/login" className="text-blue-600 hover:underline">
          Volver al inicio de sesión
        </Link>
      </p>
    </AuthSplit>
  )
}
