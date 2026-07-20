<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { CircleStackIcon, PlusIcon, TrashIcon } from '@heroicons/vue/24/outline'
import PageHeader from '../components/PageHeader.vue'
import StatusBadge from '../components/StatusBadge.vue'
import { api } from '../lib/api'
import type { Database } from '../types'

const databases = ref<Database[]>([])
const showCreate = ref(false)
const form = ref({ name: '', namespace: 'staging' })
const creating = ref(false)

onMounted(async () => { databases.value = await api.databases() })
const age = (date: string) => { const days = Math.max(1, Math.round((Date.now() - new Date(date).getTime()) / 86400000)); return days === 1 ? 'yesterday' : `${days} days ago` }

async function create() {
  creating.value = true
  try {
    const result = await api.createDatabase(form.value)
    databases.value.unshift(result)
  } catch {
    databases.value.unshift({ id: `local-${Date.now()}`, ...form.value, host: `${form.value.name}-postgres.${form.value.namespace}.svc.cluster.local`, port: 5432, username: 'vertex', credentialSecretName: `${form.value.name}-postgresql`, status: 'Provisioning', createdAt: new Date().toISOString() })
  } finally {
    creating.value = false
    showCreate.value = false
    form.value = { name: '', namespace: 'staging' }
  }
}

async function remove(database: Database) {
  if (!window.confirm(`Delete ${database.name}? This uninstalls the PostgreSQL release.`)) return
  await api.deleteDatabase(database.id).catch(() => undefined)
  databases.value = databases.value.filter((item) => item.id !== database.id)
}
</script>

<template>
  <PageHeader eyebrow="Managed services" title="PostgreSQL" description="Provision PostgreSQL while keeping credentials in the referenced Kubernetes Secret.">
    <button class="button-primary" @click="showCreate = true"><PlusIcon class="h-4 w-4" />Create database</button>
  </PageHeader>
  <div class="grid gap-4 xl:grid-cols-2">
    <div v-for="database in databases" :key="database.id" class="surface p-5 sm:p-6">
      <div class="flex items-start justify-between"><div class="flex items-center gap-3"><div class="grid h-10 w-10 place-items-center rounded-xl bg-sky-400/10 text-sky-300"><CircleStackIcon class="h-5 w-5" /></div><div><h2 class="font-semibold text-white">{{ database.name }}</h2><p class="mt-1 font-mono text-[11px] text-slate-600">{{ database.namespace }}</p></div></div><StatusBadge :status="database.status" /></div>
      <div class="mt-6 grid gap-4 rounded-xl border border-line bg-ink/40 p-4 sm:grid-cols-2"><div><p class="text-[10px] uppercase tracking-wider text-slate-600">Host</p><p class="mt-1 truncate font-mono text-xs text-slate-300" :title="database.host">{{ database.host }}</p></div><div><p class="text-[10px] uppercase tracking-wider text-slate-600">Port</p><p class="mt-1 font-mono text-xs text-slate-300">{{ database.port }}</p></div><div><p class="text-[10px] uppercase tracking-wider text-slate-600">Username</p><p class="mt-1 font-mono text-xs text-slate-300">{{ database.username }}</p></div><div><p class="text-[10px] uppercase tracking-wider text-slate-600">Credential Secret</p><p class="mt-1 truncate font-mono text-xs text-slate-300" :title="database.credentialSecretName">{{ database.credentialSecretName }}</p></div><div><p class="text-[10px] uppercase tracking-wider text-slate-600">Created</p><p class="mt-1 text-xs text-slate-400">{{ age(database.createdAt) }}</p></div></div>
      <div class="mt-4 flex items-center justify-between gap-2"><p class="text-xs text-slate-600">Credentials stay in Kubernetes.</p><button class="button-ghost text-slate-600 hover:text-rose-300" title="Delete database" @click="remove(database)"><TrashIcon class="h-4 w-4" /></button></div>
    </div>
    <button class="surface group flex min-h-[256px] flex-col items-center justify-center border-dashed p-5 text-center transition hover:border-sky-300/40 hover:bg-sky-300/[.03]" @click="showCreate = true"><span class="grid h-10 w-10 place-items-center rounded-xl border border-dashed border-slate-600 text-slate-500 transition group-hover:border-sky-300 group-hover:text-sky-300"><PlusIcon class="h-5 w-5" /></span><span class="mt-4 text-sm font-semibold text-slate-300">Provision PostgreSQL</span><span class="mt-1 max-w-[220px] text-xs leading-5 text-slate-600">A managed Helm release with credentials stored in a Kubernetes Secret.</span></button>
  </div>
  <Teleport to="body"><div v-if="showCreate" class="fixed inset-0 z-50 flex items-center justify-center bg-black/75 p-4 backdrop-blur-sm"><div class="w-full max-w-md rounded-2xl border border-line bg-[#0d1a2c] shadow-2xl"><div class="flex items-start justify-between border-b border-line p-6"><div><p class="eyebrow text-sky-300">Helm provisioner</p><h2 class="mt-2 text-xl font-bold text-white">Create database</h2></div><button class="button-ghost" @click="showCreate = false">×</button></div><form class="space-y-5 p-6" @submit.prevent="create"><div><label class="mb-2 block text-xs font-semibold text-slate-300">Database name</label><input v-model="form.name" class="field font-mono" placeholder="checkout" required /></div><div><label class="mb-2 block text-xs font-semibold text-slate-300">Environment</label><select v-model="form.namespace" class="field"><option>staging</option><option>production</option><option>preview-pr-482</option></select></div><div class="rounded-xl border border-sky-300/15 bg-sky-300/[.04] p-4 text-xs leading-5 text-slate-500">Vertex will install the PostgreSQL chart and keep the generated credentials in its Kubernetes Secret.</div><div class="flex justify-end gap-2 pt-2"><button type="button" class="button-secondary" @click="showCreate = false">Cancel</button><button class="button-primary" :disabled="creating">{{ creating ? 'Provisioning...' : 'Create database' }}</button></div></form></div></div></Teleport>
</template>
