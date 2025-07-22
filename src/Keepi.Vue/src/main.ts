import '@/style.css'

import { createApp } from 'vue'
import App from '@/App.vue'
import { router } from '@/router'
import ApiClient from '@/api-client'

if (location.pathname === '/error') {
  mountApp()
} else {
  const apiClient = new ApiClient()
  apiClient.ensureUserIsRegistered().match(
    () => mountApp(),
    (error) => {
      if (error === 'forbidden' || error == 'unauthorized') {
        location.pathname = '/signin'
      } else {
        location.pathname = '/error'
      }
    },
  )
}

function mountApp() {
  const app = createApp(App)
  app.use(router)
  app.mount('#app')
}
