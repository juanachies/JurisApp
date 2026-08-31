import { lazy, Suspense } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { GuestOnly, RequireAdmin, RequireAuth, RequireLawyer, BootScreen } from '@/app/guards'
import { AdminShell, AppShell } from '@/components/layout/AppShell'

const HomePage = lazy(() => import('@/pages/public/HomePage').then((m) => ({ default: m.HomePage })))
const PricingPage = lazy(() => import('@/pages/public/PricingPage').then((m) => ({ default: m.PricingPage })))
const LoginPage = lazy(() => import('@/pages/auth/LoginPage').then((m) => ({ default: m.LoginPage })))
const RegisterPage = lazy(() => import('@/pages/auth/RegisterPage').then((m) => ({ default: m.RegisterPage })))
const VerifyEmailPage = lazy(() => import('@/pages/auth/VerifyEmailPage').then((m) => ({ default: m.VerifyEmailPage })))
const ForgotPasswordPage = lazy(() =>
  import('@/pages/auth/ForgotPasswordPage').then((m) => ({ default: m.ForgotPasswordPage })),
)
const ResetPasswordPage = lazy(() =>
  import('@/pages/auth/ResetPasswordPage').then((m) => ({ default: m.ResetPasswordPage })),
)
const DashboardPage = lazy(() => import('@/pages/app/DashboardPage').then((m) => ({ default: m.DashboardPage })))
const ChatsPage = lazy(() => import('@/pages/app/ChatPages').then((m) => ({ default: m.ChatsPage })))
const ChatWorkspacePage = lazy(() => import('@/pages/app/ChatPages').then((m) => ({ default: m.ChatWorkspacePage })))
const CasesPage = lazy(() => import('@/pages/app/CasePages').then((m) => ({ default: m.CasesPage })))
const CaseDetailPage = lazy(() => import('@/pages/app/CasePages').then((m) => ({ default: m.CaseDetailPage })))
const DocumentsPage = lazy(() => import('@/pages/app/DocumentPages').then((m) => ({ default: m.DocumentsPage })))
const DocumentDetailPage = lazy(() =>
  import('@/pages/app/DocumentPages').then((m) => ({ default: m.DocumentDetailPage })),
)
const SkillsPage = lazy(() => import('@/pages/app/SkillPages').then((m) => ({ default: m.SkillsPage })))
const SkillEditorPage = lazy(() => import('@/pages/app/SkillPages').then((m) => ({ default: m.SkillEditorPage })))
const ProfilePage = lazy(() => import('@/pages/app/AccountPages').then((m) => ({ default: m.ProfilePage })))
const SubscriptionPage = lazy(() => import('@/pages/app/AccountPages').then((m) => ({ default: m.SubscriptionPage })))
const VerificationPage = lazy(() => import('@/pages/app/AccountPages').then((m) => ({ default: m.VerificationPage })))
const BillingSuccessPage = lazy(() =>
  import('@/pages/app/AccountPages').then((m) => ({ default: m.BillingSuccessPage })),
)
const AdminHomePage = lazy(() => import('@/pages/admin/AdminPages').then((m) => ({ default: m.AdminHomePage })))
const AdminUsersPage = lazy(() => import('@/pages/admin/AdminPages').then((m) => ({ default: m.AdminUsersPage })))
const AdminUserDetailPage = lazy(() =>
  import('@/pages/admin/AdminPages').then((m) => ({ default: m.AdminUserDetailPage })),
)
const AdminVerificationsPage = lazy(() =>
  import('@/pages/admin/AdminPages').then((m) => ({ default: m.AdminVerificationsPage })),
)
const AdminVerificationDetailPage = lazy(() =>
  import('@/pages/admin/AdminPages').then((m) => ({ default: m.AdminVerificationDetailPage })),
)
const AdminPlansPage = lazy(() => import('@/pages/admin/AdminPages').then((m) => ({ default: m.AdminPlansPage })))

function Screen() {
  return (
    <Suspense fallback={<BootScreen />}>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/pricing" element={<PricingPage />} />

        <Route element={<GuestOnly />}>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/reset-password" element={<ResetPasswordPage />} />
        </Route>
        <Route path="/verify-email" element={<VerifyEmailPage />} />

        <Route element={<RequireAuth />}>
          <Route element={<AppShell />}>
            <Route path="/app" element={<DashboardPage />} />
            <Route path="/app/chats" element={<ChatsPage />} />
            <Route path="/app/chats/:chatId" element={<ChatWorkspacePage />} />
            <Route path="/app/documents" element={<DocumentsPage />} />
            <Route path="/app/documents/:documentId" element={<DocumentDetailPage />} />
            <Route path="/app/profile" element={<ProfilePage />} />
            <Route path="/app/subscription" element={<SubscriptionPage />} />
            <Route path="/app/professional-verification" element={<VerificationPage />} />
            <Route path="/billing/success" element={<BillingSuccessPage />} />
            <Route element={<RequireLawyer />}>
              <Route path="/app/cases" element={<CasesPage />} />
              <Route path="/app/cases/:caseId" element={<CaseDetailPage />} />
              <Route path="/app/skills" element={<SkillsPage />} />
              <Route path="/app/skills/new" element={<SkillEditorPage />} />
              <Route path="/app/skills/:skillId/edit" element={<SkillEditorPage />} />
            </Route>
          </Route>

          <Route element={<RequireAdmin />}>
            <Route element={<AdminShell />}>
              <Route path="/admin" element={<AdminHomePage />} />
              <Route path="/admin/users" element={<AdminUsersPage />} />
              <Route path="/admin/users/:userId" element={<AdminUserDetailPage />} />
              <Route path="/admin/verifications" element={<AdminVerificationsPage />} />
              <Route path="/admin/verifications/:requestId" element={<AdminVerificationDetailPage />} />
              <Route path="/admin/plans" element={<AdminPlansPage />} />
            </Route>
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Suspense>
  )
}

export function AppRouter() {
  return <Screen />
}
