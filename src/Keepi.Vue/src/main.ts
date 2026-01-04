import '@/style.css'

import { createApp } from 'vue'
import App from '@/App.vue'
import { router } from '@/router'
import { RegleVuePlugin } from '@regle/core'

const app = createApp(App)
app.use(router)
app.use(RegleVuePlugin)
app.mount('#app')
