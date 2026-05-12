import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Header } from '../components/layout/Header'
import { searchService } from '../services/search'

export default function SearchPage() {
  const [input, setInput] = useState('')
  const [query, setQuery] = useState('')
  const [page, setPage] = useState(1)

  const { data, isLoading, isFetching } = useQuery({
    queryKey: ['search', query, page],
    queryFn: () => searchService.search(query, page),
    enabled: query.length > 0,
  })

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    if (input.trim()) {
      setQuery(input.trim())
      setPage(1)
    }
  }

  return (
    <div className="min-h-screen bg-white">
      <Header />
      <main className="max-w-4xl mx-auto px-4 sm:px-6 py-12">
        <h1 className="text-3xl font-bold text-gray-900 font-heading mb-8">Explore Knowledge</h1>

        <form onSubmit={handleSearch} className="flex gap-3 mb-10">
          <input
            type="text"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Search articles, topics, concepts..."
            className="flex-1 border border-gray-200 rounded-xl px-5 py-3 text-base focus:outline-none focus:ring-2 focus:ring-brand-300 focus:border-brand-400"
          />
          <button
            type="submit"
            disabled={isLoading}
            className="bg-brand-500 text-white font-medium px-6 py-3 rounded-xl hover:bg-brand-600 transition-colors disabled:opacity-60"
          >
            Search
          </button>
        </form>

        {(isLoading || isFetching) && (
          <div className="flex justify-center py-12">
            <div className="w-8 h-8 border-4 border-brand-500 border-t-transparent rounded-full animate-spin" />
          </div>
        )}

        {!isLoading && !isFetching && query && data?.items?.length === 0 && (
          <div className="text-center py-16">
            <p className="text-2xl mb-3">🔍</p>
            <p className="text-gray-700 font-medium mb-1">No results for "{query}"</p>
            <p className="text-gray-400 text-sm">Try different keywords or browse categories</p>
          </div>
        )}

        {data && data.items.length > 0 && (
          <>
            <p className="text-sm text-gray-400 mb-4">{data.totalCount} results for "{query}"</p>
            <div className="space-y-4">
              {data.items.map(article => (
                <Link
                  key={article.id}
                  to={`/articles/${article.slug}`}
                  className="block p-6 border border-gray-100 rounded-xl hover:border-brand-200 hover:shadow-sm transition-all"
                >
                  <div className="flex items-start justify-between gap-4">
                    <div className="min-w-0">
                      <h2 className="text-lg font-semibold text-gray-900 mb-1 group-hover:text-brand-600">
                        {article.title}
                      </h2>
                      {article.summary && (
                        <p className="text-gray-500 text-sm line-clamp-2 mb-2">{article.summary}</p>
                      )}
                      <div className="flex items-center gap-3 text-xs text-gray-400">
                        <span className="bg-gray-100 text-gray-600 px-2 py-0.5 rounded">{article.categoryName}</span>
                        <span>{article.topicTitle}</span>
                        <span>{article.readingTimeMinutes} min read</span>
                      </div>
                    </div>
                  </div>
                </Link>
              ))}
            </div>

            {data.totalPages > 1 && (
              <div className="flex items-center justify-center gap-3 mt-10">
                <button
                  disabled={!data.hasPrev}
                  onClick={() => setPage(p => p - 1)}
                  className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  Previous
                </button>
                <span className="text-sm text-gray-500">Page {data.page} of {data.totalPages}</span>
                <button
                  disabled={!data.hasNext}
                  onClick={() => setPage(p => p + 1)}
                  className="px-4 py-2 text-sm border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  Next
                </button>
              </div>
            )}
          </>
        )}

        {!query && (
          <div className="text-center py-16 text-gray-400">
            <p className="text-4xl mb-4">📚</p>
            <p className="font-medium text-gray-600 mb-1">Start searching</p>
            <p className="text-sm">Enter keywords above to find articles across all topics</p>
          </div>
        )}
      </main>
    </div>
  )
}
