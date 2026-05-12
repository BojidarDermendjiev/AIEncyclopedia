import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Header } from '../components/layout/Header'
import { articlesService } from '../services/articles'
import { pdfsService, type PdfStyle } from '../services/pdfs'
import { usersService } from '../services/users'
import { useAuthStore } from '../stores/authStore'

export default function ArticlePage() {
  const { slug } = useParams<{ slug: string }>()
  const { isAuthenticated, user } = useAuthStore()
  const queryClient = useQueryClient()
  const [pdfStyle, setPdfStyle] = useState<PdfStyle>('Academic')
  const [pdfLoading, setPdfLoading] = useState(false)

  const { data, isLoading, isError } = useQuery({
    queryKey: ['article', slug],
    queryFn: () => articlesService.getBySlug(slug!),
    enabled: !!slug,
  })

  const bookmarksMutation = useMutation({
    mutationFn: (articleId: string) => usersService.addBookmark(articleId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['bookmarks'] }),
  })

  const handleDownloadPdf = async () => {
    if (!data?.data?.id) return
    setPdfLoading(true)
    try {
      const result = await pdfsService.generate(data.data.id, pdfStyle)
      const blob = await pdfsService.download(result.id)
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `${slug}-${pdfStyle.toLowerCase()}.pdf`
      a.click()
      URL.revokeObjectURL(url)
      queryClient.invalidateQueries({ queryKey: ['pdf-library'] })
    } finally {
      setPdfLoading(false)
    }
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-white">
        <Header />
        <div className="flex justify-center items-center h-96">
          <div className="w-8 h-8 border-4 border-brand-500 border-t-transparent rounded-full animate-spin" />
        </div>
      </div>
    )
  }

  if (isError || !data?.data) {
    return (
      <div className="min-h-screen bg-white">
        <Header />
        <div className="max-w-3xl mx-auto px-4 py-24 text-center">
          <h1 className="text-2xl font-bold text-gray-900 mb-3">Article not found</h1>
          <p className="text-gray-500">The article you're looking for doesn't exist or has been moved.</p>
        </div>
      </div>
    )
  }

  const article = data.data

  return (
    <div className="min-h-screen bg-white">
      <Header />
      <div className="max-w-4xl mx-auto px-4 sm:px-6 py-12">
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
          <article className="lg:col-span-3">
            <header className="mb-8">
              <div className="flex items-center gap-2 text-sm text-gray-400 mb-3">
                {article.topic?.category?.name && (
                  <span className="bg-brand-50 text-brand-700 px-2 py-0.5 rounded">{article.topic.category.name}</span>
                )}
                {article.topic?.title && <span>{article.topic.title}</span>}
              </div>
              <h1 className="text-4xl font-bold text-gray-900 font-heading leading-tight mb-4">
                {article.title}
              </h1>
              {article.summary && (
                <p className="text-xl text-gray-500 leading-relaxed">{article.summary}</p>
              )}
              <div className="flex items-center gap-4 mt-4 text-sm text-gray-400">
                <span>{article.readingTimeMinutes} min read</span>
                {article.aiGenerated && (
                  <span className="bg-accent-100 text-accent-700 px-2 py-0.5 rounded-full text-xs font-medium">
                    AI Generated
                  </span>
                )}
                {article.publishedAt && (
                  <span>{new Date(article.publishedAt).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })}</span>
                )}
              </div>
            </header>

            <div
              className="prose prose-lg prose-gray max-w-none"
              dangerouslySetInnerHTML={{ __html: article.content }}
            />

            {article.articleTags && article.articleTags.length > 0 && (
              <div className="mt-8 pt-6 border-t border-gray-100 flex flex-wrap gap-2">
                {article.articleTags.map((at: { tag: { name: string; slug: string } }) => (
                  <span key={at.tag.slug} className="text-xs bg-gray-100 text-gray-600 px-3 py-1 rounded-full">
                    {at.tag.name}
                  </span>
                ))}
              </div>
            )}
          </article>

          <aside className="lg:col-span-1 space-y-4">
            {isAuthenticated && (
              <>
                <button
                  onClick={() => bookmarksMutation.mutate(article.id)}
                  disabled={bookmarksMutation.isPending}
                  className="w-full flex items-center justify-center gap-2 px-4 py-2.5 border border-gray-200 rounded-xl text-sm font-medium text-gray-700 hover:bg-gray-50 hover:border-brand-200 transition-all"
                >
                  {bookmarksMutation.isPending ? '...' : '🔖 Save Article'}
                </button>

                <div className="bg-gray-50 rounded-xl p-4">
                  <p className="text-sm font-medium text-gray-700 mb-3">Download PDF</p>
                  <select
                    value={pdfStyle}
                    onChange={e => setPdfStyle(e.target.value as PdfStyle)}
                    className="w-full text-sm border border-gray-200 rounded-lg px-3 py-2 mb-3 bg-white focus:outline-none focus:ring-1 focus:ring-brand-300"
                  >
                    {(['Academic', 'Student', 'Presentation', 'Summary'] as PdfStyle[]).map(s => (
                      <option key={s} value={s}>{s}</option>
                    ))}
                  </select>
                  <button
                    onClick={handleDownloadPdf}
                    disabled={pdfLoading || user?.subscriptionTier?.toLowerCase() === 'free'}
                    className="w-full bg-brand-500 text-white text-sm font-medium py-2.5 rounded-xl hover:bg-brand-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {pdfLoading ? 'Generating...' : 'Download PDF'}
                  </button>
                  {user?.subscriptionTier?.toLowerCase() === 'free' && (
                    <p className="text-xs text-gray-400 mt-2 text-center">
                      <a href="/pricing" className="text-brand-600 hover:underline">Upgrade to Pro</a> for PDF downloads
                    </p>
                  )}
                </div>
              </>
            )}
          </aside>
        </div>
      </div>
    </div>
  )
}
