import { mockApplications, mockDashboard, mockDatabases, mockEnvironments, mockLogs, mockSecretDetails, mockSecrets } from '../data/mock'
import type { Application, Dashboard, Database, Environment, Logs, Secret, SecretDetail } from '../types'

const API_URL = import.meta.env.VITE_API_URL ?? ''

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const token = localStorage.getItem('vertex_token')
  const response = await fetch(`${API_URL}${path}`, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}), ...init?.headers },
  })
  const body = await response.json()
  if (!response.ok || body.success === false) throw new Error(body.error ?? 'Request failed')
  return body.data as T
}

async function safe<T>(call: () => Promise<T>, fallback: T): Promise<T> {
  try { return await call() } catch { return fallback }
}

export const api = {
  login: (email: string, password: string) => request<{ token: string; email: string; displayName: string; expiresAt: string }>('/api/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }),
  dashboard: () => safe<Dashboard>(() => request('/api/dashboard'), mockDashboard),
  applications: () => safe<Application[]>(() => request('/api/applications'), mockApplications),
  deploy: (payload: { name: string; namespace: string; image: string; replicas: number; port: number; ingressHost?: string }) => request<Application>('/api/applications', { method: 'POST', body: JSON.stringify(payload) }),
  scale: (id: string, replicas: number) => request(`/api/applications/${id}/scale`, { method: 'PUT', body: JSON.stringify({ replicas }) }),
  restart: (id: string) => request(`/api/applications/${id}/restart`, { method: 'POST' }),
  deleteApplication: (id: string) => request(`/api/applications/${id}`, { method: 'DELETE' }),
  environments: () => safe<Environment[]>(() => request('/api/environments'), mockEnvironments),
  createEnvironment: (payload: { name: string; namespace: string }) => request<Environment>('/api/environments', { method: 'POST', body: JSON.stringify(payload) }),
  deleteEnvironment: (id: string) => request(`/api/environments/${id}`, { method: 'DELETE' }),
  secrets: () => safe<Secret[]>(() => request('/api/secrets'), mockSecrets),
  secret: (id: string) => safe<SecretDetail>(() => request(`/api/secrets/${id}`), mockSecretDetails[id] ?? mockSecretDetails['sec-checkout']),
  createSecret: (payload: { name: string; namespace: string; values: Record<string, string> }) => request<SecretDetail>('/api/secrets', { method: 'POST', body: JSON.stringify(payload) }),
  deleteSecret: (id: string) => request(`/api/secrets/${id}`, { method: 'DELETE' }),
  databases: () => safe<Database[]>(() => request('/api/databases'), mockDatabases),
  createDatabase: (payload: { name: string; namespace: string }) => request<Database>('/api/databases', { method: 'POST', body: JSON.stringify(payload) }),
  deleteDatabase: (id: string) => request(`/api/databases/${id}`, { method: 'DELETE' }),
  logs: (application: string, pod?: string) => safe<Logs>(() => request(`/api/logs?application=${encodeURIComponent(application)}${pod ? `&pod=${encodeURIComponent(pod)}` : ''}`), mockLogs),
}

