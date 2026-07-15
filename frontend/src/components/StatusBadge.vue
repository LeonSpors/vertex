<script setup lang="ts">
defineProps<{ status: string }>()
const tone = (status: string) => status.toLowerCase().includes('healthy') || status.toLowerCase().includes('ready') || status.toLowerCase().includes('connected') ? 'success' : status.toLowerCase().includes('progress') || status.toLowerCase().includes('demo') || status.toLowerCase().includes('setup') ? 'warning' : status.toLowerCase().includes('degrad') || status.toLowerCase().includes('fail') ? 'danger' : 'muted'
</script>

<template><span :class="['inline-flex items-center gap-1.5 rounded-full px-2 py-1 text-[11px] font-semibold', tone(status) === 'success' ? 'bg-accent/10 text-accent' : tone(status) === 'warning' ? 'bg-amber-400/10 text-amber-300' : tone(status) === 'danger' ? 'bg-rose-400/10 text-rose-300' : 'bg-slate-400/10 text-slate-400']"><span :class="['h-1.5 w-1.5 rounded-full', tone(status) === 'success' ? 'bg-accent' : tone(status) === 'warning' ? 'bg-amber-300' : tone(status) === 'danger' ? 'bg-rose-300' : 'bg-slate-500']" />{{ status }}</span></template>
