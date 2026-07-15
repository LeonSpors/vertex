export type Application = {
  id: string
  name: string
  namespace: string
  image: string
  status: 'Healthy' | 'Progressing' | 'Degraded' | 'Unknown'
  replicas: number
  availableReplicas: number
  port: number
  ingressHost?: string | null
  createdAt: string
  updatedAt: string
}

export type Dashboard = {
  cluster: { status: string; version: string; namespaces: number; storageClasses: number; ingressClasses: number }
  resources: { runningPods: number; runningDeployments: number; nodes: number; cpuUsagePercent: number | null; memoryUsagePercent: number | null }
  nodes: { name: string; status: string; cpu: string; memory: string; role: string }[]
  recentEvents: { type: string; reason: string; message: string; namespace: string; timestamp: string }[]
}

export type ClusterSetup = {
  status: 'Connected' | 'Demo' | 'SetupRequired' | string
  mode: string
  version: string | null
  error: string | null
  steps: { number: number; title: string; description: string; command: string }[]
}

export type Environment = { id: string; name: string; namespace: string; owner: string; status: string; createdAt: string }
export type Secret = { id: string; name: string; namespace: string; keyCount: number; updatedAt: string }
export type SecretDetail = { id: string; name: string; namespace: string; values: Record<string, string>; updatedAt: string }
export type Database = { id: string; name: string; namespace: string; host: string; port: number; username: string; password: string; connectionString: string; status: string; createdAt: string }
export type Logs = { application: string; pod: string; lines: { timestamp: string; level: string; message: string }[] }
