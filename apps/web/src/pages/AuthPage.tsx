import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { authService } from '../services/auth'
import { useAuthStore } from '../stores/authStore'

interface AuthPageProps {
  mode: 'login' | 'register'
}

export default function AuthPage({ mode }: AuthPageProps) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const { setAuth } = useAuthStore()
  const navigate = useNavigate()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      let result
      if (mode === 'login') {
        result = await authService.login({ email, password })
      } else {
        result = await authService.register({ email, password, displayName })
      }

      if (result.success) {
        const { accessToken, refreshToken } = result.data
        setAuth(
          {
            id: '',
            email,
            displayName: displayName || email,
            subscriptionTier: 'free',
            isAdmin: false,
          },
          accessToken,
          refreshToken,
        )
        navigate('/dashboard')
      } else {
        setError(result.message || 'Authentication failed.')
      }
    } catch {
      setError('An error occurred. Please try again.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center px-4">
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-8 w-full max-w-md">
        <Link to="/" className="block text-center mb-6">
          <span className="text-xl font-bold text-brand-600 font-heading">AI Encyclopedia</span>
        </Link>

        <h1 className="text-2xl font-bold text-gray-900 font-heading text-center mb-2">
          {mode === 'login' ? 'Welcome back' : 'Create an account'}
        </h1>
        <p className="text-gray-500 text-center text-sm mb-8">
          {mode === 'login'
            ? 'Log in to access your knowledge library'
            : 'Start exploring AI-generated knowledge'}
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          {mode === 'register' && (
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
              <input
                type="text"
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                required
                className="w-full border border-gray-200 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-brand-300"
                placeholder="Your name"
              />
            </div>
          )}
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              className="w-full border border-gray-200 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-brand-300"
              placeholder="you@example.com"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full border border-gray-200 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-brand-300"
              placeholder="••••••••"
            />
          </div>

          {error && <p className="text-red-500 text-sm">{error}</p>}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-brand-500 text-white font-medium py-3 rounded-lg hover:bg-brand-600 transition-colors disabled:opacity-50"
          >
            {loading ? 'Please wait...' : mode === 'login' ? 'Log in' : 'Create account'}
          </button>
        </form>

        <p className="text-center text-sm text-gray-500 mt-6">
          {mode === 'login' ? (
            <>Don't have an account? <Link to="/register" className="text-brand-600 font-medium">Sign up</Link></>
          ) : (
            <>Already have an account? <Link to="/login" className="text-brand-600 font-medium">Log in</Link></>
          )}
        </p>
      </div>
    </div>
  )
}
