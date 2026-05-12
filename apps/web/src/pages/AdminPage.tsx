import { useState } from 'react'
import { Navigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { Header } from '../components/layout/Header'
import { useAuthStore } from '../stores/authStore'
import { apiClient as api } from '../services/api'

type Tab = 'articles' | 'users' | 'jobs'

export default function AdminPage() {
  const { user, isAuthenticated } = useAuthStore()
  const [tab, setTab] = useState<Tab>('articles')

  if (!isAuthenticated) return <Navigate to="/login" replace />
  if (!user?.isAdmin) return <Navigate to="/dashboard" replace />

  return (
    <div className="min-h-screen bg-gray-50">
      <Header />
      <main className="max-w-7xl mx-auto px-4 sm:px-6 py-10">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">Admin Panel</h1>

        <div className="flex gap-1 mb-6 bg-white rounded-xl border border-gray-100 p-1 w-fit">
          {(['articles', 'users', 'jobs'] as Tab[]).map(t => (
            <button
              key={t}
              onClick={() => setTab(t)}
              className={`px-4 py-2 rounded-lg text-sm font-medium capitalize transition-colors ${
                tab === t ? 'bg-brand-500 text-white' : 'text-gray-600 hover:bg-gray-50'
              }`}
            >
              {t === 'jobs' ? 'AI Jobs' : t}
            </button>
          ))}
        </div>

        {tab === 'articles' && <ArticlesTab />}
        {tab === 'users' && <UsersTab />}
        {tab === 'jobs' && <AIJobsTab />}
      </main>
    </div>
  )
}

function ArticlesTab() {
  const queryClient = useQueryClient()
  const [statusFilter, setStatusFilter] = useState('')

  const { data, isLoading } = useQuery({
    queryKey: ['admin-articles', statusFilter],
    queryFn: async () => {
      const params: Record<string, string> = { page: '1', pageSize: '50' }
      if (statusFilter) params.status = statusFilter
      const { data } = await api.get<{ success: boolean; data: { items: AdminArticle[]; totalCount: number } }>('/api/v1/admin/articles', { params })
      return data.data
    },
  })

  const publishMutation = useMutation({
    mutationFn: (id: string) => api.put(`/api/v1/admin/articles/${id}/publish`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-articles'] }),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/api/v1/admin/articles/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-articles'] }),
  })

  return (
    <div className="bg-white rounded-2xl border border-gray-100">
      <div className="flex items-center justify-between p-4 border-b border-gray-100">
        <p className="text-sm text-gray-500">{data?.totalCount ?? 0} articles</p>
        <select
          value={statusFilter}
          onChange={e => setStatusFilter(e.target.value)}
          className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none"
        >
          <option value="">All statuses</option>
          <option value="0">Draft</option>
          <option value="1">Review</option>
          <option value="2">Published</option>
        </select>
      </div>
      {isLoading && <div className="flex justify-center py-8"><Spinner /></div>}
      <table className="w-full text-sm">
        <thead>
          <tr className="text-left text-xs text-gray-400 border-b border-gray-50">
            <th className="px-4 py-3 font-medium">Title</th>
            <th className="px-4 py-3 font-medium">Status</th>
            <th className="px-4 py-3 font-medium">AI</th>
            <th className="px-4 py-3 font-medium">Views</th>
            <th className="px-4 py-3 font-medium">Created</th>
            <th className="px-4 py-3 font-medium">Actions</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-50">
          {data?.items.map(a => (
            <tr key={a.id} className="hover:bg-gray-50">
              <td className="px-4 py-3 max-w-xs">
                <p className="font-medium text-gray-900 truncate">{a.title}</p>
                <p className="text-xs text-gray-400 truncate">{a.slug}</p>
              </td>
              <td className="px-4 py-3">
                <StatusBadge status={a.status} />
              </td>
              <td className="px-4 py-3">
                {a.aiGenerated && <span className="text-xs bg-accent-100 text-accent-700 px-2 py-0.5 rounded-full">{a.aiProvider ?? 'AI'}</span>}
              </td>
              <td className="px-4 py-3 text-gray-500">{a.viewCount.toLocaleString()}</td>
              <td className="px-4 py-3 text-gray-400">{new Date(a.createdAt).toLocaleDateString()}</td>
              <td className="px-4 py-3">
                <div className="flex gap-2">
                  {a.status !== 'Published' && (
                    <button
                      onClick={() => publishMutation.mutate(a.id)}
                      disabled={publishMutation.isPending}
                      className="text-xs text-green-600 hover:text-green-700 font-medium"
                    >
                      Publish
                    </button>
                  )}
                  <button
                    onClick={() => {
                      if (confirm(`Delete "${a.title}"?`)) deleteMutation.mutate(a.id)
                    }}
                    className="text-xs text-red-500 hover:text-red-600 font-medium"
                  >
                    Delete
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function UsersTab() {
  const { data, isLoading } = useQuery({
    queryKey: ['admin-users'],
    queryFn: async () => {
      const { data } = await api.get<{ success: boolean; data: { items: AdminUser[]; totalCount: number } }>('/api/v1/admin/users', { params: { page: 1, pageSize: 50 } })
      return data.data
    },
  })

  return (
    <div className="bg-white rounded-2xl border border-gray-100">
      <div className="p-4 border-b border-gray-100">
        <p className="text-sm text-gray-500">{data?.totalCount ?? 0} users</p>
      </div>
      {isLoading && <div className="flex justify-center py-8"><Spinner /></div>}
      <table className="w-full text-sm">
        <thead>
          <tr className="text-left text-xs text-gray-400 border-b border-gray-50">
            <th className="px-4 py-3 font-medium">User</th>
            <th className="px-4 py-3 font-medium">Plan</th>
            <th className="px-4 py-3 font-medium">Role</th>
            <th className="px-4 py-3 font-medium">Status</th>
            <th className="px-4 py-3 font-medium">Joined</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-50">
          {data?.items.map(u => (
            <tr key={u.id} className="hover:bg-gray-50">
              <td className="px-4 py-3">
                <p className="font-medium text-gray-900">{u.displayName}</p>
                <p className="text-xs text-gray-400">{u.email}</p>
              </td>
              <td className="px-4 py-3">
                <span className="text-xs bg-brand-50 text-brand-700 px-2 py-0.5 rounded-full capitalize">{u.subscriptionTier}</span>
              </td>
              <td className="px-4 py-3">{u.isAdmin && <span className="text-xs bg-red-50 text-red-600 px-2 py-0.5 rounded-full">Admin</span>}</td>
              <td className="px-4 py-3">
                <span className={`text-xs px-2 py-0.5 rounded-full ${u.isActive ? 'bg-green-50 text-green-600' : 'bg-gray-100 text-gray-500'}`}>
                  {u.isActive ? 'Active' : 'Inactive'}
                </span>
              </td>
              <td className="px-4 py-3 text-gray-400">{new Date(u.createdAt).toLocaleDateString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function AIJobsTab() {
  const [statusFilter, setStatusFilter] = useState('')

  const { data, isLoading } = useQuery({
    queryKey: ['admin-ai-jobs', statusFilter],
    queryFn: async () => {
      const params: Record<string, string> = { page: '1', pageSize: '50' }
      if (statusFilter) params.status = statusFilter
      const { data } = await api.get<{ success: boolean; data: { items: AIJobItem[]; totalCount: number } }>('/api/v1/admin/ai-jobs', { params })
      return data.data
    },
  })

  return (
    <div className="bg-white rounded-2xl border border-gray-100">
      <div className="flex items-center justify-between p-4 border-b border-gray-100">
        <p className="text-sm text-gray-500">{data?.totalCount ?? 0} jobs</p>
        <select
          value={statusFilter}
          onChange={e => setStatusFilter(e.target.value)}
          className="text-sm border border-gray-200 rounded-lg px-3 py-1.5 focus:outline-none"
        >
          <option value="">All</option>
          <option value="0">Pending</option>
          <option value="1">Processing</option>
          <option value="2">Done</option>
          <option value="3">Failed</option>
        </select>
      </div>
      {isLoading && <div className="flex justify-center py-8"><Spinner /></div>}
      <table className="w-full text-sm">
        <thead>
          <tr className="text-left text-xs text-gray-400 border-b border-gray-50">
            <th className="px-4 py-3 font-medium">Type</th>
            <th className="px-4 py-3 font-medium">Status</th>
            <th className="px-4 py-3 font-medium">Provider</th>
            <th className="px-4 py-3 font-medium">Error</th>
            <th className="px-4 py-3 font-medium">Created</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-50">
          {data?.items.map(j => (
            <tr key={j.id} className="hover:bg-gray-50">
              <td className="px-4 py-3 font-medium text-gray-900">{j.jobType}</td>
              <td className="px-4 py-3"><JobStatusBadge status={j.status} /></td>
              <td className="px-4 py-3 text-gray-500">{j.provider}</td>
              <td className="px-4 py-3 text-red-500 text-xs max-w-xs truncate">{j.errorMessage}</td>
              <td className="px-4 py-3 text-gray-400">{new Date(j.createdAt).toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function StatusBadge({ status }: { status: string }) {
  const map: Record<string, string> = {
    Draft: 'bg-gray-100 text-gray-600',
    Review: 'bg-yellow-50 text-yellow-700',
    Published: 'bg-green-50 text-green-700',
  }
  return <span className={`text-xs px-2 py-0.5 rounded-full ${map[status] ?? 'bg-gray-100 text-gray-500'}`}>{status}</span>
}

function JobStatusBadge({ status }: { status: string }) {
  const map: Record<string, string> = {
    Pending: 'bg-yellow-50 text-yellow-700',
    Processing: 'bg-blue-50 text-blue-700',
    Done: 'bg-green-50 text-green-700',
    Failed: 'bg-red-50 text-red-600',
  }
  return <span className={`text-xs px-2 py-0.5 rounded-full ${map[status] ?? 'bg-gray-100 text-gray-500'}`}>{status}</span>
}

function Spinner() {
  return <div className="w-6 h-6 border-2 border-brand-500 border-t-transparent rounded-full animate-spin" />
}

interface AdminArticle { id: string; title: string; slug: string; status: string; aiGenerated: boolean; aiProvider: string | null; viewCount: number; createdAt: string; publishedAt: string | null }
interface AdminUser { id: string; email: string; displayName: string; subscriptionTier: string; isAdmin: boolean; isActive: boolean; createdAt: string }
interface AIJobItem { id: string; jobType: string; status: string; provider: string; errorMessage: string | null; createdAt: string; completedAt: string | null }
