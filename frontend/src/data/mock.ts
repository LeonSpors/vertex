import type { Application, Dashboard, Database, Environment, Logs, Secret, SecretDetail } from '../types'

export const mockDashboard: Dashboard = {
  cluster: { status: 'Demo cluster', version: 'v1.30.2', namespaces: 7, storageClasses: 6, ingressClasses: 1 },
  resources: { runningPods: 18, runningDeployments: 3, nodes: 3, cpuUsagePercent: 39, memoryUsagePercent: 54 },
  nodes: [
    { name: 'vertex-worker-01', status: 'Ready', cpu: '32%', memory: '58%', role: 'worker' },
    { name: 'vertex-worker-02', status: 'Ready', cpu: '47%', memory: '64%', role: 'worker' },
    { name: 'vertex-control-01', status: 'Ready', cpu: '18%', memory: '41%', role: 'control-plane' },
  ],
  recentEvents: [
    { type: 'Normal', reason: 'ScalingReplicaSet', message: 'Scaled checkout-api to 3 replicas', namespace: 'production', timestamp: new Date(Date.now() - 8 * 60000).toISOString() },
    { type: 'Normal', reason: 'SuccessfulResync', message: 'Ingress reconciled successfully', namespace: 'staging', timestamp: new Date(Date.now() - 24 * 60000).toISOString() },
    { type: 'Warning', reason: 'BackOff', message: 'catalog-worker container restarted once', namespace: 'production', timestamp: new Date(Date.now() - 60 * 60000).toISOString() },
  ],
}

export const mockApplications: Application[] = [
  { id: 'app-checkout', name: 'checkout-api', namespace: 'production', image: 'ghcr.io/vertex/checkout-api:v1.8.2', status: 'Healthy', replicas: 3, availableReplicas: 3, port: 8080, ingressHost: 'checkout.vertex.local', createdAt: '2026-06-22T09:30:00Z', updatedAt: '2026-07-15T08:10:00Z' },
  { id: 'app-catalog', name: 'catalog-worker', namespace: 'production', image: 'ghcr.io/vertex/catalog-worker:v2.4.0', status: 'Progressing', replicas: 2, availableReplicas: 1, port: 8080, ingressHost: null, createdAt: '2026-06-17T11:00:00Z', updatedAt: '2026-07-15T07:34:00Z' },
  { id: 'app-docs', name: 'docs-site', namespace: 'staging', image: 'ghcr.io/vertex/docs-site:v0.9.1', status: 'Healthy', replicas: 1, availableReplicas: 1, port: 3000, ingressHost: 'docs.staging.vertex.local', createdAt: '2026-06-10T15:45:00Z', updatedAt: '2026-07-14T16:02:00Z' },
]

export const mockEnvironments: Environment[] = [
  { id: 'env-prod', name: 'Production', namespace: 'production', owner: 'platform@vertex.local', status: 'Ready', createdAt: '2026-03-12T10:00:00Z' },
  { id: 'env-stage', name: 'Staging', namespace: 'staging', owner: 'alex@vertex.local', status: 'Ready', createdAt: '2026-04-18T14:20:00Z' },
  { id: 'env-preview', name: 'Preview / PR-482', namespace: 'preview-pr-482', owner: 'maya@vertex.local', status: 'Ready', createdAt: '2026-07-14T08:45:00Z' },
]

export const mockSecrets: Secret[] = [
  { id: 'sec-checkout', name: 'checkout-config', namespace: 'production', keyCount: 2, updatedAt: '2026-07-14T16:20:00Z' },
  { id: 'sec-catalog', name: 'catalog-config', namespace: 'production', keyCount: 2, updatedAt: '2026-07-12T10:05:00Z' },
  { id: 'sec-preview', name: 'preview-env', namespace: 'preview-pr-482', keyCount: 1, updatedAt: '2026-07-15T07:32:00Z' },
]

export const mockSecretDetails: Record<string, SecretDetail> = {
  'sec-checkout': { id: 'sec-checkout', name: 'checkout-config', namespace: 'production', values: { DATABASE_URL: 'postgres://••••••••', STRIPE_KEY: 'sk_live_••••••••' }, updatedAt: '2026-07-14T16:20:00Z' },
  'sec-catalog': { id: 'sec-catalog', name: 'catalog-config', namespace: 'production', values: { REDIS_URL: 'redis://••••••••', SEARCH_TOKEN: '••••••••' }, updatedAt: '2026-07-12T10:05:00Z' },
  'sec-preview': { id: 'sec-preview', name: 'preview-env', namespace: 'preview-pr-482', values: { FEATURE_FLAG: 'new-checkout' }, updatedAt: '2026-07-15T07:32:00Z' },
}

export const mockDatabases: Database[] = [{ id: 'db-checkout', name: 'checkout', namespace: 'production', host: 'checkout-postgres.production.svc.cluster.local', port: 5432, username: 'vertex', password: 'vertex-demo-password', connectionString: 'Host=checkout-postgres.production.svc.cluster.local;Port=5432;Database=checkout;Username=vertex;Password=••••••••', status: 'Ready', createdAt: '2026-05-28T13:30:00Z' }]

export const mockLogs: Logs = {
  application: 'checkout-api', pod: 'checkout-api-7c8bd9b9f8-x2k4m', lines: [
    { timestamp: '09:41:02', level: 'INFO', message: 'checkout-api starting application server' },
    { timestamp: '09:41:07', level: 'INFO', message: 'connected to PostgreSQL and Redis' },
    { timestamp: '09:41:15', level: 'INFO', message: 'health probe passed: /health/ready' },
    { timestamp: '09:41:26', level: 'INFO', message: 'request completed pod=checkout-api-7c8bd9b9f8-x2k4m status=200 duration=42ms' },
    { timestamp: '09:41:37', level: 'INFO', message: 'reconciler heartbeat complete' },
  ],
}

