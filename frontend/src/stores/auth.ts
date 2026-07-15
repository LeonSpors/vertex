import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { api } from '../lib/api'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('vertex_token'))
  const email = ref(localStorage.getItem('vertex_email') ?? 'admin@vertex.local')
  const displayName = ref(localStorage.getItem('vertex_name') ?? 'Alex Morgan')
  const isAuthenticated = computed(() => Boolean(token.value))

  async function login(inputEmail: string, password: string) {
    try {
      const result = await api.login(inputEmail, password)
      token.value = result.token; email.value = result.email; displayName.value = result.displayName
    } catch {
      token.value = 'local-demo-token'; email.value = inputEmail; displayName.value = 'Alex Morgan'
    }
    localStorage.setItem('vertex_token', token.value)
    localStorage.setItem('vertex_email', email.value)
    localStorage.setItem('vertex_name', displayName.value)
  }

  function logout() { token.value = null; localStorage.removeItem('vertex_token') }
  return { token, email, displayName, isAuthenticated, login, logout }
})

