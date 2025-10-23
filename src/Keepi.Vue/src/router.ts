import WeekView from '@/views/WeekView.vue'
import SetupCategoriesView from '@/views/SetupCategoriesView.vue'

import { createRouter, createWebHistory } from 'vue-router'
import ExportView from '@/views/ExportView.vue'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: WeekView },
    { path: '/categories', component: SetupCategoriesView },
    { path: '/export', component: ExportView },
  ],
})
