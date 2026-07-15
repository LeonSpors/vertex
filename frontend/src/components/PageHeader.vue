<script setup lang="ts">
defineProps<{ eyebrow: string; title: string; description: string }>()
</script>

<template>
  <div class="mb-8 flex flex-col justify-between gap-5 sm:flex-row sm:items-end"><div><p class="eyebrow text-accent/80">{{ eyebrow }}</p><h1 class="mt-2 text-2xl font-bold tracking-tight text-white sm:text-3xl">{{ title }}</h1><p class="mt-2 max-w-2xl text-sm leading-6 text-slate-500">{{ description }}</p></div><div class="shrink-0"><slot /></div></div>
</template>

