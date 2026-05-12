import { Link } from 'react-router-dom'
import { Header } from '../components/layout/Header'
import { Footer } from '../components/layout/Footer'

const plans = [
  {
    name: 'Free',
    price: '$0',
    period: 'forever',
    features: ['Access all articles', '3 PDF downloads/month', 'Basic search'],
    cta: 'Get started',
    href: '/register',
    highlight: false,
  },
  {
    name: 'Pro',
    price: '$9.99',
    period: '/month',
    features: ['Everything in Free', 'Unlimited PDF downloads', 'AI study assistant', 'Saved library', 'Advanced search'],
    cta: 'Start Pro',
    href: '/register',
    highlight: true,
  },
  {
    name: 'Enterprise',
    price: '$29.99',
    period: '/month',
    features: ['Everything in Pro', 'Team access', 'API access', 'Custom branding', 'Analytics'],
    cta: 'Contact us',
    href: '/register',
    highlight: false,
  },
]

export default function PricingPage() {
  return (
    <div className="min-h-screen flex flex-col bg-white">
      <Header />
      <main className="flex-1 max-w-5xl mx-auto px-4 sm:px-6 py-20">
        <h1 className="text-4xl font-bold text-center text-gray-900 font-heading mb-4">Simple, transparent pricing</h1>
        <p className="text-center text-gray-500 mb-14">Start free, upgrade when you're ready.</p>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {plans.map((plan) => (
            <div
              key={plan.name}
              className={`rounded-2xl border p-8 flex flex-col ${
                plan.highlight
                  ? 'border-brand-400 shadow-lg shadow-brand-100 bg-brand-50'
                  : 'border-gray-100'
              }`}
            >
              <h2 className="text-xl font-bold text-gray-900 font-heading mb-1">{plan.name}</h2>
              <div className="mb-6">
                <span className="text-4xl font-bold text-gray-900">{plan.price}</span>
                <span className="text-gray-400 text-sm">{plan.period}</span>
              </div>
              <ul className="space-y-3 flex-1 mb-8">
                {plan.features.map((f) => (
                  <li key={f} className="flex items-center gap-2 text-sm text-gray-600">
                    <span className="text-brand-500">✓</span>
                    {f}
                  </li>
                ))}
              </ul>
              <Link
                to={plan.href}
                className={`text-center font-medium py-3 rounded-xl transition-colors ${
                  plan.highlight
                    ? 'bg-brand-500 text-white hover:bg-brand-600'
                    : 'border border-gray-200 text-gray-700 hover:border-brand-300 hover:text-brand-600'
                }`}
              >
                {plan.cta}
              </Link>
            </div>
          ))}
        </div>
      </main>
      <Footer />
    </div>
  )
}
