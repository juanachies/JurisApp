import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
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
  firstName: z.string().min(1, 'El nombre es obligatorio'),
  lastName: z.string().min(1, 'El apellido es obligatorio'),
  email: z.string().min(1, 'El email es obligatorio'),
  password: z.string().min(8, 'La contraseña debe tener al menos 8 caracteres'),
})

type FormValues = z.infer<typeof schema>

export function RegisterPage() {
  const { register: registerUser } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { firstName: '', lastName: '', email: '', password: '' },
  })

  return (
    <AuthSplit title="Crear cuenta" subtitle="Empezá a trabajar con casos, documentos e IA en contexto.">
      {error ? <Alert className="mb-4">{error}</Alert> : null}
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit(async (values) => {
          setError(null)
          try {
            await registerUser(values)
            navigate(`/verify-email?email=${encodeURIComponent(values.email)}`, { replace: true })
          } catch (err) {
            setError(errorMessage(err, 'No pudimos crear la cuenta.'))
          }
        })}
      >
        <div className="grid gap-4 sm:grid-cols-2">
          <Input label="Nombre" autoComplete="given-name" {...form.register('firstName')} error={form.formState.errors.firstName?.message} />
          <Input label="Apellido" autoComplete="family-name" {...form.register('lastName')} error={form.formState.errors.lastName?.message} />
        </div>
        <Input label="Email" type="email" autoComplete="email" {...form.register('email')} error={form.formState.errors.email?.message} />
        <Input
          label="Contraseña"
          type="password"
          autoComplete="new-password"
          hint="Mínimo 8 caracteres"
          {...form.register('password')}
          error={form.formState.errors.password?.message}
        />
        <Button type="submit" className="w-full" loading={form.formState.isSubmitting}>
          Crear cuenta
        </Button>
      </form>
      <p className="mt-4 text-[14px] text-muted">
        ¿Ya tenés cuenta?{' '}
        <Link to="/login" className="text-blue-600 hover:underline">
          Iniciar sesión
        </Link>
      </p>
    </AuthSplit>
  )
}
