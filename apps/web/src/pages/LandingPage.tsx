import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Header } from '../components/layout/Header'
import { Footer } from '../components/layout/Footer'

export default function LandingPage() {
  return (
    <div className="min-h-screen flex flex-col bg-white">
      <Header />

      {/* Hero */}
      <main className="flex-1">
        <section className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-24 text-center">
          <motion.div
            initial={{ opacity: 0, y: 24 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
          >
            <span className="inline-flex items-center gap-2 bg-brand-50 text-brand-700 text-sm font-medium px-4 py-1.5 rounded-full mb-6">
              AI-Powered Knowledge Platform
            </span>

            <h1 className="text-5xl sm:text-6xl lg:text-7xl font-bold text-gray-900 font-heading leading-tight mb-6">
              Knowledge, generated <br />
              <span className="text-brand-500">by AI, for you.</span>
            </h1>

            <p className="text-xl text-gray-500 max-w-2xl mx-auto mb-10">
              Explore thousands of educational articles, download professional PDFs, and
              learn with an AI assistant — all in one place.
            </p>

            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link
                to="/search"
                className="bg-brand-500 text-white font-semibold px-8 py-4 rounded-xl text-lg hover:bg-brand-600 transition-colors"
              >
                Start exploring
              </Link>
              <Link
                to="/pricing"
                className="border border-gray-200 text-gray-700 font-semibold px-8 py-4 rounded-xl text-lg hover:border-brand-300 hover:text-brand-600 transition-colors"
              >
                View pricing
              </Link>
            </div>
          </motion.div>
        </section>

        {/* Features */}
        <section className="bg-gray-50 py-20">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <h2 className="text-3xl font-bold text-center text-gray-900 font-heading mb-12">
              Everything you need to learn
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
              {features.map((f) => (
                <motion.div
                  key={f.title}
                  initial={{ opacity: 0, y: 16 }}
                  whileInView={{ opacity: 1, y: 0 }}
                  viewport={{ once: true }}
                  className="bg-white rounded-2xl p-6 border border-gray-100 shadow-sm"
                >
                  <div className="w-10 h-10 bg-brand-50 rounded-lg flex items-center justify-center mb-4 text-xl">
                    {f.icon}
                  </div>
                  <h3 className="text-lg font-semibold text-gray-900 mb-2">{f.title}</h3>
                  <p className="text-gray-500 text-sm leading-relaxed">{f.description}</p>
                </motion.div>
              ))}
            </div>
          </div>
        </section>
      </main>

      <Footer />
    </div>
  )
}

const features = [
  {
    icon: '🔍',
    title: 'Smart Search',
    description: 'Semantic search across thousands of topics. Find exactly what you need in seconds.',
  },
  {
    icon: '📄',
    title: 'PDF Downloads',
    description: 'Export any article as a beautifully formatted PDF in academic, student, or summary style.',
  },
  {
    icon: '🤖',
    title: 'AI Assistant',
    description: 'Ask questions about any article. Get instant, contextual answers powered by GPT-4.',
  },
]
