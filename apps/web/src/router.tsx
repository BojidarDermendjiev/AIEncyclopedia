import { createBrowserRouter } from 'react-router-dom'
import { lazy, Suspense } from 'react'

const LandingPage = lazy(() => import('./pages/LandingPage'))
const SearchPage = lazy(() => import('./pages/SearchPage'))
const ArticlePage = lazy(() => import('./pages/ArticlePage'))
const AuthPage = lazy(() => import('./pages/AuthPage'))
const PricingPage = lazy(() => import('./pages/PricingPage'))
const DashboardPage = lazy(() => import('./pages/DashboardPage'))
const AdminPage = lazy(() => import('./pages/AdminPage'))

function Loading() {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="w-8 h-8 border-4 border-brand-500 border-t-transparent rounded-full animate-spin" />
    </div>
  )
}

function Wrap({ children }: { children: React.ReactNode }) {
  return <Suspense fallback={<Loading />}>{children}</Suspense>
}

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Wrap><LandingPage /></Wrap>,
  },
  {
    path: '/search',
    element: <Wrap><SearchPage /></Wrap>,
  },
  {
    path: '/articles/:slug',
    element: <Wrap><ArticlePage /></Wrap>,
  },
  {
    path: '/login',
    element: <Wrap><AuthPage mode="login" /></Wrap>,
  },
  {
    path: '/register',
    element: <Wrap><AuthPage mode="register" /></Wrap>,
  },
  {
    path: '/pricing',
    element: <Wrap><PricingPage /></Wrap>,
  },
  {
    path: '/dashboard',
    element: <Wrap><DashboardPage /></Wrap>,
  },
  {
    path: '/admin',
    element: <Wrap><AdminPage /></Wrap>,
  },
])
