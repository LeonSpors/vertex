import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/login', name: 'login', component: () => import('../views/LoginView.vue'), meta: { public: true } },
    { path: '/', redirect: '/dashboard' },
    { path: '/dashboard', component: () => import('../views/DashboardView.vue') },
    { path: '/applications', component: () => import('../views/ApplicationsView.vue') },
    { path: '/environments', component: () => import('../views/EnvironmentsView.vue') },
    { path: '/secrets', component: () => import('../views/SecretsView.vue') },
    { path: '/databases', component: () => import('../views/DatabasesView.vue') },
    { path: '/logs', component: () => import('../views/LogsView.vue') },
    { path: '/settings', component: () => import('../views/SettingsView.vue') },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (!to.meta.public && !auth.isAuthenticated) return { name: 'login' }
  if (to.name === 'login' && auth.isAuthenticated) return { path: '/dashboard' }
})

export default router

