import { createBrowserRouter, Navigate } from 'react-router-dom'
import { MarketingLayout } from '@/components/layout/MarketingLayout'
import { AuthLayout } from '@/components/layout/AuthLayout'
import { AppShell } from '@/components/layout/AppShell'
import { ProtectedRoute, GuestRoute, RoleRoute } from '@/lib/auth/guards'
import { HomePage } from '@/pages/marketing/HomePage'
import { PricingPage } from '@/pages/marketing/PricingPage'
import { LoginPage } from '@/pages/auth/LoginPage'
import { RegisterPage } from '@/pages/auth/RegisterPage'
import { ForgotPasswordPage } from '@/pages/auth/ForgotPasswordPage'
import { ResetPasswordPage } from '@/pages/auth/ResetPasswordPage'
import { DashboardPage } from '@/pages/app/DashboardPage'
import { ChatsPage } from '@/pages/app/ChatsPage'
import { ChatDetailPage } from '@/pages/app/ChatDetailPage'
import { FoldersPage } from '@/pages/app/FoldersPage'
import { SkillsPage } from '@/pages/app/SkillsPage'
import { PlansPage } from '@/pages/app/PlansPage'
import { SettingsPage } from '@/pages/app/SettingsPage'
import { AdminUsersPage } from '@/pages/app/AdminUsersPage'

export const router = createBrowserRouter([
  {
    element: <MarketingLayout />,
    children: [
      { path: '/', element: <HomePage /> },
      { path: '/pricing', element: <PricingPage /> },
    ],
  },
  {
    element: <GuestRoute />,
    children: [
      {
        element: <AuthLayout />,
        children: [
          { path: '/login', element: <LoginPage /> },
          { path: '/register', element: <RegisterPage /> },
          { path: '/forgot-password', element: <ForgotPasswordPage /> },
          { path: '/reset-password', element: <ResetPasswordPage /> },
        ],
      },
    ],
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: '/app',
        element: <AppShell />,
        children: [
          { index: true, element: <Navigate to="dashboard" replace /> },
          { path: 'dashboard', element: <DashboardPage /> },
          { path: 'chats', element: <ChatsPage /> },
          { path: 'chats/:id', element: <ChatDetailPage /> },
          { path: 'plans', element: <PlansPage /> },
          { path: 'settings', element: <SettingsPage /> },
          {
            element: <RoleRoute roles={['Lawyer', 'Admin']} />,
            children: [
              { path: 'folders', element: <FoldersPage /> },
              { path: 'skills', element: <SkillsPage /> },
            ],
          },
          {
            element: <RoleRoute roles={['Admin']} />,
            children: [{ path: 'admin/users', element: <AdminUsersPage /> }],
          },
        ],
      },
    ],
  },
  { path: '*', element: <Navigate to="/" replace /> },
])
