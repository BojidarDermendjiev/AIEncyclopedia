import { Link, useNavigate } from 'react-router-dom'
import { useAuthStore } from '../../stores/authStore'

export function Header() {
  const { isAuthenticated, user, clearAuth } = useAuthStore()
  const navigate = useNavigate()

  const handleLogout = () => {
    clearAuth()
    navigate('/')
  }

  return (
    <header className="sticky top-0 z-50 bg-white/90 backdrop-blur border-b border-gray-100">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
        <Link to="/" className="flex items-center gap-2">
          <span className="text-xl font-bold text-brand-600 font-heading">AI Encyclopedia</span>
        </Link>

        <nav className="hidden md:flex items-center gap-6">
          <Link to="/search" className="text-gray-600 hover:text-brand-600 transition-colors text-sm font-medium">
            Explore
          </Link>
          <Link to="/pricing" className="text-gray-600 hover:text-brand-600 transition-colors text-sm font-medium">
            Pricing
          </Link>
        </nav>

        <div className="flex items-center gap-3">
          {isAuthenticated ? (
            <>
              <Link to="/dashboard" className="text-sm font-medium text-gray-700 hover:text-brand-600">
                {user?.displayName}
              </Link>
              <button
                onClick={handleLogout}
                className="text-sm text-gray-500 hover:text-gray-800 transition-colors"
              >
                Log out
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="text-sm font-medium text-gray-700 hover:text-brand-600">
                Log in
              </Link>
              <Link
                to="/register"
                className="bg-brand-500 text-white text-sm font-medium px-4 py-2 rounded-lg hover:bg-brand-600 transition-colors"
              >
                Get started
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  )
}
