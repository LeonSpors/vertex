<script setup lang="ts">
import { RouterView, useRoute } from 'vue-router'
import AppShell from './components/AppShell.vue'

const route = useRoute()
</script>

<template>
  <RouterView v-slot="{ Component }">
    <component :is="route.meta.public ? Component : AppShell">
      <component :is="Component" v-if="!route.meta.public" />
    </component>
  </RouterView>
</template>

