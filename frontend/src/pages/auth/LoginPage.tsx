import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useAuth } from '@/app/AuthContext'
import { AuthSplit } from '@/components/layout/PublicChrome'
import { Alert } from '@/components/ui/Alert'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { errorMessage } from '@/api/client'

const schema = z.object({
  email: z.string().min(1, 'El email es obligatorio'),
  password: z.string().min(1, 'La contraseña es obligatoria'),
})

type FormValues = z.infer<typeof schema>

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [error, setError] = useState<string | null>(null)
  const expired = Boolean((location.state as { session?: boolean } | null)?.session)
  const from = (location.state as { from?: string } | null)?.from

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { email: '', password: '' },
  })

  return (
    <AuthSplit title="Bienvenido de nuevo" subtitle="Ingresá para continuar tu trabajo.">
      {expired ? <Alert className="mb-4">Tu sesión venció. Volvé a iniciar sesión.</Alert> : null}
      {error ? <Alert className="mb-4">{error}</Alert> : null}
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit(async (values) => {
          setError(null)
          try {
            await login(values.email, values.password)
            navigate(from && from.startsWith('/app') ? from : '/app', { replace: true })
          } catch (err) {
            setError(errorMessage(err, 'El email o la contraseña no son correctos.'))
          }
        })}
      >
        <Input label="Email" type="email" autoComplete="email" {...form.register('email')} error={form.formState.errors.email?.message} />
        <Input
          label="Contraseña"
          type="password"
          autoComplete="current-password"
          {...form.register('password')}
          error={form.formState.errors.password?.message}
        />
        <Button type="submit" className="w-full" loading={form.formState.isSubmitting}>
          Iniciar sesión
        </Button>
      </form>
      <p className="mt-4 text-[14px]">
        <Link to="/forgot-password" className="text-blue-600 hover:underline">
          ¿Olvidaste tu contraseña?
        </Link>
      </p>
      <p className="mt-3 text-[14px] text-muted">
        ¿No tenés cuenta?{' '}
        <Link to="/register" className="text-blue-600 hover:underline">
          Crear cuenta
        </Link>
      </p>
    </AuthSplit>
  )
}
