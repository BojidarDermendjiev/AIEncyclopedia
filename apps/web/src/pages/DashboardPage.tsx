import { useQuery } from '@tanstack/react-query'
import { Link, Navigate } from 'react-router-dom'
import { Header } from '../components/layout/Header'
import { useAuthStore } from '../stores/authStore'
import { usersService } from '../services/users'
import { pdfsService } from '../services/pdfs'

export default function DashboardPage() {
  const { isAuthenticated, user } = useAuthStore()

  if (!isAuthenticated) return <Navigate to="/login" replace />

  return (
    <div className="min-h-screen bg-gray-50">
      <Header />
      <main className="max-w-6xl mx-auto px-4 sm:px-6 py-12">
        <div className="flex items-center justify-between mb-8">
          <div>
            <h1 className="text-3xl font-bold text-gray-900 font-heading mb-1">
              Welcome back, {user?.displayName}
            </h1>
            <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-sm font-medium bg-brand-100 text-brand-700 capitalize">
              {user?.subscriptionTier} Plan
            </span>
          </div>
          {user?.isAdmin && (
            <Link to="/admin" className="bg-gray-900 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-gray-800 transition-colors">
              Admin Panel
            </Link>
          )}
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <div className="lg:col-span-2 space-y-8">
            <BookmarksPanel />
            <ReadingHistoryPanel />
          </div>
          <div className="space-y-6">
            <QuickActions tier={user?.subscriptionTier ?? 'free'} />
            <PdfLibraryPanel />
          </div>
        </div>
      </main>
    </div>
  )
}

function BookmarksPanel() {
  const { data: bookmarks, isLoading } = useQuery({
    queryKey: ['bookmarks'],
    queryFn: () => usersService.getBookmarks(),
  })

  return (
    <section className="bg-white rounded-2xl border border-gray-100 p-6">
      <h2 className="text-lg font-semibold text-gray-900 mb-4">Saved Articles</h2>
      {isLoading && <Spinner />}
      {!isLoading && (!bookmarks || bookmarks.length === 0) && (
        <p className="text-gray-400 text-sm">No saved articles yet. <Link to="/search" className="text-brand-600 hover:underline">Explore articles</Link></p>
      )}
      <div className="space-y-3">
        {bookmarks?.slice(0, 5).map(b => (
          <Link
            key={b.articleId}
            to={`/articles/${b.slug}`}
            className="flex items-start gap-3 p-3 rounded-xl hover:bg-gray-50 transition-colors group"
          >
            <div className="w-2 h-2 mt-2 rounded-full bg-brand-400 shrink-0" />
            <div className="min-w-0">
              <p className="font-medium text-gray-900 text-sm truncate group-hover:text-brand-600">{b.title}</p>
              <p className="text-xs text-gray-400 mt-0.5">{b.readingTimeMinutes} min · saved {formatDate(b.bookmarkedAt)}</p>
            </div>
          </Link>
        ))}
      </div>
    </section>
  )
}

function ReadingHistoryPanel() {
  const { data: history, isLoading } = useQuery({
    queryKey: ['reading-history'],
    queryFn: () => usersService.getHistory(10),
  })

  return (
    <section className="bg-white rounded-2xl border border-gray-100 p-6">
      <h2 className="text-lg font-semibold text-gray-900 mb-4">Continue Reading</h2>
      {isLoading && <Spinner />}
      {!isLoading && (!history || history.length === 0) && (
        <p className="text-gray-400 text-sm">No reading history yet.</p>
      )}
      <div className="space-y-3">
        {history?.map(h => (
          <Link
            key={h.articleId}
            to={`/articles/${h.slug}`}
            className="block p-3 rounded-xl hover:bg-gray-50 transition-colors"
          >
            <div className="flex items-center justify-between mb-1.5">
              <p className="font-medium text-gray-900 text-sm truncate mr-3">{h.title}</p>
              <span className="text-xs text-gray-400 shrink-0">{h.progressPercent}%</span>
            </div>
            <div className="w-full bg-gray-100 rounded-full h-1.5">
              <div
                className="bg-brand-500 h-1.5 rounded-full"
                style={{ width: `${h.progressPercent}%` }}
              />
            </div>
          </Link>
        ))}
      </div>
    </section>
  )
}

function PdfLibraryPanel() {
  const { data: pdfs, isLoading } = useQuery({
    queryKey: ['pdf-library'],
    queryFn: () => pdfsService.getLibrary(),
  })

  const downloadPdf = async (pdfId: string, title: string) => {
    const blob = await pdfsService.download(pdfId)
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${title}.pdf`
    a.click()
    URL.revokeObjectURL(url)
  }

  return (
    <section className="bg-white rounded-2xl border border-gray-100 p-6">
      <h2 className="text-lg font-semibold text-gray-900 mb-4">PDF Library</h2>
      {isLoading && <Spinner />}
      {!isLoading && (!pdfs || pdfs.length === 0) && (
        <p className="text-gray-400 text-sm">No PDFs generated yet.</p>
      )}
      <div className="space-y-2">
        {pdfs?.slice(0, 5).map(p => (
          <div key={p.id} className="flex items-center gap-2 p-2 rounded-lg hover:bg-gray-50">
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium text-gray-900 truncate">{p.articleTitle}</p>
              <p className="text-xs text-gray-400">{p.style} · {formatBytes(p.fileSizeBytes)}</p>
            </div>
            <button
              onClick={() => downloadPdf(p.id, p.articleTitle)}
              className="shrink-0 text-brand-600 hover:text-brand-700 text-xs font-medium px-2 py-1 rounded hover:bg-brand-50"
            >
              Download
            </button>
          </div>
        ))}
      </div>
    </section>
  )
}

function QuickActions({ tier }: { tier: string }) {
  return (
    <section className="bg-white rounded-2xl border border-gray-100 p-6">
      <h2 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>
      <div className="space-y-2">
        <Link to="/search" className="flex items-center gap-3 p-3 rounded-xl hover:bg-brand-50 text-gray-700 hover:text-brand-700 transition-colors">
          <span className="text-xl">🔍</span>
          <span className="text-sm font-medium">Browse Articles</span>
        </Link>
        {tier.toLowerCase() === 'free' && (
          <Link to="/pricing" className="flex items-center gap-3 p-3 rounded-xl bg-gradient-to-r from-brand-50 to-accent-50 hover:from-brand-100 hover:to-accent-100 transition-colors">
            <span className="text-xl">⚡</span>
            <div>
              <p className="text-sm font-medium text-gray-900">Upgrade to Pro</p>
              <p className="text-xs text-gray-500">Unlimited PDFs & AI assistant</p>
            </div>
          </Link>
        )}
      </div>
    </section>
  )
}

function Spinner() {
  return (
    <div className="flex justify-center py-6">
      <div className="w-6 h-6 border-2 border-brand-500 border-t-transparent rounded-full animate-spin" />
    </div>
  )
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
}

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}
