import { Link } from 'react-router-dom'

export function Footer() {
  return (
    <footer className="border-t border-gray-100 mt-auto py-12">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
          <div>
            <span className="text-lg font-bold text-brand-600 font-heading">AI Encyclopedia</span>
            <p className="mt-2 text-sm text-gray-500">
              AI-powered knowledge for everyone.
            </p>
          </div>
          <div>
            <h4 className="text-sm font-semibold text-gray-900 mb-3">Platform</h4>
            <ul className="space-y-2 text-sm text-gray-500">
              <li><Link to="/search" className="hover:text-brand-600">Explore</Link></li>
              <li><Link to="/pricing" className="hover:text-brand-600">Pricing</Link></li>
            </ul>
          </div>
          <div>
            <h4 className="text-sm font-semibold text-gray-900 mb-3">Account</h4>
            <ul className="space-y-2 text-sm text-gray-500">
              <li><Link to="/login" className="hover:text-brand-600">Log in</Link></li>
              <li><Link to="/register" className="hover:text-brand-600">Sign up</Link></li>
            </ul>
          </div>
        </div>
        <p className="mt-8 text-xs text-gray-400 text-center">
          © {new Date().getFullYear()} AI Encyclopedia. All rights reserved.
        </p>
      </div>
    </footer>
  )
}
