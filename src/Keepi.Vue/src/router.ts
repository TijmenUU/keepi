import { createRouter, createWebHistory } from 'vue-router'

import CustomizationsView from '@/views/CustomizationsView.vue'
import ExportView from '@/views/ExportView.vue'
import ProjectsView from '@/views/ProjectsView.vue'
import WeekView from '@/views/WeekView.vue'
import { getWeekNumber } from '@/date'
import ErrorView from '@/views/ErrorView.vue'
import SignedOutView from '@/views/SignedOutView.vue'
import NotFoundView from '@/views/NotFoundView.vue'
import UsersView from '@/views/UsersView.vue'
import DisabledUserView from '@/views/DisabledUserView.vue'

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth: boolean
  }
}

export const router = createRouter({
  history: createWebHistory(),
  routes: [
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
    { path: '/users', component: UsersView, meta: { requiresAuth: true } },
    { path: '/disableduser', component: DisabledUserView, meta: { requiresAuth: false } },
    // Public paths
    { path: '/error', component: ErrorView, meta: { requiresAuth: false } },
    { path: '/signedout', component: SignedOutView, meta: { requiresAuth: false } },
    { path: '/:catchAll(.*)', component: NotFoundView, meta: { requiresAuth: false } },
  ],
})
