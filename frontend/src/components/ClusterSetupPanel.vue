<script setup lang="ts">
import { ref } from 'vue'
import { ArrowPathIcon, CheckCircleIcon, ClipboardDocumentIcon, ExclamationTriangleIcon } from '@heroicons/vue/24/outline'
import type { ClusterSetup } from '../types'

defineProps<{ setup: ClusterSetup; refreshing?: boolean }>()
const emit = defineEmits<{ refresh: [] }>()
const copied = ref<number | null>(null)

async function copyCommand(number: number, command: string) {
  await navigator.clipboard.writeText(command)
  copied.value = number
  window.setTimeout(() => { if (copied.value === number) copied.value = null }, 1600)
}
</script>

<template>
  <section class="mb-5 overflow-hidden rounded-2xl border border-amber-300/20 bg-amber-300/[.04]">
    <div class="flex flex-col gap-4 border-b border-amber-300/15 p-5 sm:flex-row sm:items-start sm:justify-between sm:p-6">
      <div class="flex items-start gap-3">
        <div class="grid h-9 w-9 shrink-0 place-items-center rounded-lg bg-amber-300/10 text-amber-300"><ExclamationTriangleIcon class="h-5 w-5" /></div>
        <div>
          <p class="eyebrow text-amber-300/70">Assisted cluster setup</p>
          <h2 class="mt-2 text-base font-semibold text-white">Vertex is ready, but the cluster is not connected</h2>
          <p class="mt-1 max-w-3xl text-xs leading-5 text-slate-400">The API stays available while you provide Kubernetes credentials. Nothing is stored in Vertex; the client uses a kubeconfig or the in-cluster service account.</p>
        </div>
      </div>
      <button class="button-secondary shrink-0 text-xs" :disabled="refreshing" @click="emit('refresh')"><ArrowPathIcon class="h-4 w-4" :class="refreshing ? 'animate-spin' : ''" />{{ refreshing ? 'Checking...' : 'Test connection' }}</button>
    </div>
    <div v-if="setup.error" class="border-b border-amber-300/15 px-5 py-4 text-xs leading-5 text-amber-200/80 sm:px-6"><span class="font-semibold text-amber-200">Connection check:</span> {{ setup.error }}</div>
    <div class="grid gap-px bg-amber-300/10 sm:grid-cols-2 xl:grid-cols-4">
      <div v-for="step in setup.steps" :key="step.number" class="bg-[#101d2e] p-5">
        <div class="flex items-center gap-2"><span class="grid h-6 w-6 place-items-center rounded-full bg-amber-300/10 text-xs font-bold text-amber-300">{{ step.number }}</span><h3 class="text-sm font-semibold text-slate-200">{{ step.title }}</h3></div>
        <p class="mt-3 min-h-16 text-xs leading-5 text-slate-500">{{ step.description }}</p>
        <div class="mt-4 flex items-start gap-2 rounded-lg border border-line bg-ink/70 p-2"><code class="min-w-0 flex-1 break-words font-mono text-[10px] leading-4 text-slate-400">{{ step.command }}</code><button class="button-ghost shrink-0 px-1.5" title="Copy command" @click="copyCommand(step.number, step.command)"><CheckCircleIcon v-if="copied === step.number" class="h-4 w-4 text-accent" /><ClipboardDocumentIcon v-else class="h-4 w-4" /></button></div>
      </div>
    </div>
  </section>
</template>
