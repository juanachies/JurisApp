import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { authApi } from '@/api'
import { errorMessage } from '@/api/client'
import { useAuth } from '@/app/AuthContext'
import { AuthSplit } from '@/components/layout/PublicChrome'
import { Alert } from '@/components/ui/Alert'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'

const schema = z.object({
  email: z.string().min(1, 'El email es obligatorio'),
  code: z.string().min(6, 'Ingresá el código de 6 dígitos').max(6),
})

type FormValues = z.infer<typeof schema>

export function VerifyEmailPage() {
  const [params] = useSearchParams()
  const { applyAuth, user } = useAuth()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)
  const [info, setInfo] = useState<string | null>(null)
  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { email: params.get('email') || user?.email || '', code: '' },
  })

  return (
    <AuthSplit
      title="Verificá tu email"
      subtitle="Te enviamos un código de 6 dígitos. En desarrollo aparece en la consola del servidor."
    >
      {error ? <Alert className="mb-4">{error}</Alert> : null}
      {info ? (
        <Alert variant="success" className="mb-4">
          {info}
        </Alert>
      ) : null}
      <form
        className="space-y-4"
        onSubmit={form.handleSubmit(async (values) => {
          setError(null)
          try {
            const response = await authApi.verifyEmail(values)
            applyAuth(response)
            navigate('/app', { replace: true })
          } catch (err) {
            setError(errorMessage(err, 'No pudimos verificar el email.'))
          }
        })}
      >
        <Input label="Email" type="email" {...form.register('email')} error={form.formState.errors.email?.message} />
        <Input label="Código" inputMode="numeric" maxLength={6} {...form.register('code')} error={form.formState.errors.code?.message} />
        <Button type="submit" className="w-full" loading={form.formState.isSubmitting}>
          Verificar
        </Button>
      </form>
      <button
        type="button"
        className="mt-4 text-[14px] text-blue-600 hover:underline"
        onClick={async () => {
          setError(null)
          try {
            await authApi.resendVerification({ email: form.getValues('email') })
            setInfo('Si el email existe, te enviamos un código nuevo.')
          } catch (err) {
            setError(errorMessage(err))
          }
        }}
      >
        Reenviar código
      </button>
      <p className="mt-4 text-[14px] text-muted">
        <Link to="/login" className="text-blue-600 hover:underline">
          Volver al inicio de sesión
        </Link>
      </p>
    </AuthSplit>
  )
}
