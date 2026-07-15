<script setup lang="ts">
defineProps<{ label: string; value: string | number; detail: string; icon: any; accent?: string }>()
</script>

<template><div class="surface relative overflow-hidden p-5"><div class="flex items-start justify-between"><div><p class="text-xs font-medium text-slate-500">{{ label }}</p><p class="mt-3 text-2xl font-bold tracking-tight text-white">{{ value }}</p><p class="mt-1 text-xs text-slate-600">{{ detail }}</p></div><div :class="['grid h-9 w-9 place-items-center rounded-xl', accent ?? 'bg-accent/10 text-accent']"><component :is="icon" class="h-[18px] w-[18px]" /></div></div><div class="absolute -bottom-10 -right-8 h-24 w-24 rounded-full bg-accent/[.03] blur-2xl" /></div></template>

