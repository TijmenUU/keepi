import WeekView from '@/views/WeekView.vue'
import SetupCategoriesView from '@/views/SetupCategoriesView.vue'

import { createRouter, createWebHistory } from 'vue-router'

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: WeekView },
    { path: '/categories', component: SetupCategoriesView },
  ],
})
