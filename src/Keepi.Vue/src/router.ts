import { createRouter, createWebHistory } from 'vue-router'

import CustomizationsView from '@/views/CustomizationsView.vue'
import ExportView from '@/views/ExportView.vue'
import ProjectsView from '@/views/ProjectsView.vue'
import WeekView from '@/views/WeekView.vue'
import type { ApiError } from '@/types'
import { toast } from 'vue-sonner'
import { getWeekNumber } from '@/date'
import ErrorView from '@/views/ErrorView.vue'
import SignedOutView from '@/views/SignedOutView.vue'

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth: boolean
  }
}

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    // TODO add not found page
    {
      path: '/',
      redirect: () => {
        const today = new Date()
        const weekNumber = getWeekNumber(new Date())
        return { path: `/input/year/${today.getFullYear()}/week/${weekNumber}` }
      },
      meta: { requiresAuth: true },
    },
    {
      path: '/input/year/:year/week/:weekNumber',
      component: WeekView,
      props: true,
      meta: { requiresAuth: true },
    },
    { path: '/export', component: ExportView, meta: { requiresAuth: true } },
    { path: '/customizations', component: CustomizationsView, meta: { requiresAuth: true } },
    { path: '/projects', component: ProjectsView, meta: { requiresAuth: true } },

    // Public paths
    { path: '/error', component: ErrorView, meta: { requiresAuth: false } },
    { path: '/signedout', component: SignedOutView, meta: { requiresAuth: false } },
  ],
})

let hasShownSessionExpired = false

export function handleApiError(error: ApiError) {
  if (error.type === 'unauthorized') {
    if (!hasShownSessionExpired) {
      hasShownSessionExpired = true
      toast('Je sessie is verlopen. Stel eventuele wijzigingen veilig en kies herlaad.', {
        dismissible: false,
        closeButton: false,
        duration: Infinity,
        action: {
          label: 'Herlaad',
          onClick: () => (location.href = '/signin'),
        },
      })
    }

    return
  }

  // This could happen if the user entity is somehow evicted from the database
  // unexpectedly yet the session/login is valid.
  if (error.type === 'forbidden') {
    toast.error('Je hebt onvoldoende bevoegdheden om deze actie uit te voeren.')
    return
  }

  toast.error('Een onverwachte fout is opgetreden en eventuele wijzigingen zijn niet opgeslagen.')
}
