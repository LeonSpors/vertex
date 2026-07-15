<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { ChartBarIcon, CircleStackIcon, Cog6ToothIcon, CubeTransparentIcon, KeyIcon, RectangleStackIcon, ServerStackIcon, Squares2X2Icon, XMarkIcon, Bars3Icon } from '@heroicons/vue/24/outline'

const route = useRoute(); const router = useRouter(); const auth = useAuthStore(); const mobileOpen = ref(false); const search = ref('')
const navigation = [
  { label: 'Overview', path: '/dashboard', icon: Squares2X2Icon },
  { label: 'Applications', path: '/applications', icon: CubeTransparentIcon },
  { label: 'Environments', path: '/environments', icon: RectangleStackIcon },
  { label: 'Secrets', path: '/secrets', icon: KeyIcon },
  { label: 'Databases', path: '/databases', icon: CircleStackIcon },
  { label: 'Logs', path: '/logs', icon: ChartBarIcon },
]
function logout() { auth.logout(); router.push('/login') }
function goToSearch() { if (search.value.trim()) router.push({ path: '/applications', query: { q: search.value.trim() } }) }
</script>

<template>
  <div class="min-h-screen bg-ink text-slate-100">
    <div v-if="mobileOpen" class="fixed inset-0 z-30 bg-black/70 lg:hidden" @click="mobileOpen = false" />
    <aside :class="['fixed inset-y-0 left-0 z-40 flex w-64 flex-col border-r border-line bg-[#0a1526] transition-transform lg:translate-x-0', mobileOpen ? 'translate-x-0' : '-translate-x-full']">
      <div class="flex h-20 items-center justify-between px-6"><router-link to="/dashboard" class="flex items-center gap-3" @click="mobileOpen = false"><span class="grid h-9 w-9 place-items-center rounded-xl bg-accent text-ink shadow-[0_0_28px_rgba(99,230,190,.22)]"><svg viewBox="0 0 24 24" class="h-5 w-5 fill-none stroke-current stroke-[2.4]"><path d="M6 5h12M6 12h8M6 19h12" stroke-linecap="round"/><circle cx="17" cy="12" r="2"/></svg></span><span class="text-lg font-bold tracking-tight">vertex<span class="text-accent">.</span></span></router-link><button class="button-ghost lg:hidden" @click="mobileOpen = false"><XMarkIcon class="h-5 w-5" /></button></div>
      <div class="px-4 pb-4"><div class="rounded-xl border border-accent/15 bg-accent/[.05] px-3 py-2.5"><div class="flex items-center justify-between"><span class="eyebrow text-accent/70">Workspace</span><span class="h-1.5 w-1.5 rounded-full bg-accent shadow-[0_0_9px_#63e6be]" /></div><div class="mt-1 text-sm font-medium">Vertex platform</div><div class="mt-0.5 text-xs text-slate-500">Development cluster</div></div></div>
      <nav class="flex-1 space-y-1 px-3"><p class="px-3 pb-2 pt-3 text-[10px] font-semibold uppercase tracking-[.2em] text-slate-600">Workspace</p><router-link v-for="item in navigation" :key="item.path" :to="item.path" :class="['group flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition', route.path === item.path ? 'bg-white/[.07] font-semibold text-white shadow-[inset_2px_0_0_#63e6be]' : 'text-slate-400 hover:bg-white/[.04] hover:text-slate-200']" @click="mobileOpen = false"><component :is="item.icon" class="h-[18px] w-[18px]" :class="route.path === item.path ? 'text-accent' : 'text-slate-500 group-hover:text-slate-300'" />{{ item.label }}</router-link><p class="px-3 pb-2 pt-8 text-[10px] font-semibold uppercase tracking-[.2em] text-slate-600">System</p><router-link to="/settings" :class="['group flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition', route.path === '/settings' ? 'bg-white/[.07] font-semibold text-white shadow-[inset_2px_0_0_#63e6be]' : 'text-slate-400 hover:bg-white/[.04] hover:text-slate-200']" @click="mobileOpen = false"><Cog6ToothIcon class="h-[18px] w-[18px] text-slate-500 group-hover:text-slate-300" />Settings</router-link></nav>
      <div class="border-t border-line px-4 py-4"><div class="flex items-center gap-3 rounded-xl px-2 py-2"><div class="grid h-8 w-8 shrink-0 place-items-center rounded-full bg-gradient-to-br from-violet to-accent text-xs font-bold text-ink">AM</div><div class="min-w-0 flex-1"><p class="truncate text-xs font-semibold text-slate-200">{{ auth.displayName }}</p><p class="truncate text-[11px] text-slate-500">{{ auth.email }}</p></div><button class="button-ghost px-1 text-slate-600 hover:text-slate-200" title="Sign out" @click="logout"><svg class="h-4 w-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M9 5H5v14h4M14 8l4 4-4 4M18 12H9" stroke-linecap="round" stroke-linejoin="round" /></svg></button></div></div>
    </aside>
    <div class="lg:pl-64"><header class="sticky top-0 z-20 flex h-20 items-center justify-between border-b border-line/70 bg-ink/85 px-4 backdrop-blur-xl sm:px-8"><button class="button-ghost lg:hidden" @click="mobileOpen = true"><Bars3Icon class="h-5 w-5" /></button><div class="relative hidden w-full max-w-md sm:block"><svg class="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-600" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="7"/><path d="m20 20-4-4" stroke-linecap="round" /></svg><input v-model="search" class="field h-10 border-transparent bg-white/[.035] pl-9 pr-20 text-xs" placeholder="Search applications, namespaces..." @keyup.enter="goToSearch" /><span class="absolute right-2 top-1/2 -translate-y-1/2 rounded border border-line px-1.5 py-0.5 font-mono text-[10px] text-slate-600">⌘ K</span></div><div class="ml-auto flex items-center gap-3"><div class="hidden items-center gap-2 rounded-full border border-accent/15 bg-accent/[.05] px-3 py-1.5 text-xs text-accent md:flex"><span class="h-1.5 w-1.5 rounded-full bg-accent" />All systems operational</div><button class="button-ghost relative"><svg class="h-5 w-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7"><path d="M18 9a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9ZM10 21h4" stroke-linecap="round" /></svg><span class="absolute right-1.5 top-1.5 h-1.5 w-1.5 rounded-full bg-violet" /></button><div class="h-7 w-px bg-line" /><div class="grid h-8 w-8 place-items-center rounded-full bg-gradient-to-br from-violet to-accent text-xs font-bold text-ink">AM</div></div></header><main class="mx-auto max-w-[1440px] p-4 sm:p-8"><slot /></main></div>
  </div>
</template>

