<script setup lang="ts">
import KeepiSidebar from '@/components/KeepiSidebar.vue'
import KeepiSpinner from '@/components/KeepiSpinner.vue'
import SidebarProvider from '@/components/ui/sidebar/SidebarProvider.vue'
import SidebarTrigger from '@/components/ui/sidebar/SidebarTrigger.vue'
import { useColorMode } from '@vueuse/core'
import { Moon, Sun } from 'lucide-vue-next'
import { Button } from '@/components/ui/button'
import { Toaster } from '@/components/ui/sonner'
import 'vue-sonner/style.css'
import { onMounted, ref } from 'vue'
import ApiClient from '@/api-client'
import { router } from '@/router'
import { TooltipProvider } from '@/components/ui/tooltip'
import { clearUserContext, setUserContext } from '@/user-context'
import { getUserRole } from '@/user-roles'

const buildDate: string = import.meta.env.VITE_APPLICATION_BUILD_DATE
const buildCommit: string = import.meta.env.VITE_APPLICATION_BUILD_COMMIT

const mode = useColorMode({ disableTransition: false })
const onToggleTheme = () => {
  if (mode.value === 'light') {
    mode.value = 'dark'
  } else {
    mode.value = 'light'
  }
}

const hasLoaded = ref(false)
const showSideBar = ref(false)

onMounted(async () => {
  const apiClient = new ApiClient()
  try {
    await apiClient.getUser().match(
      (user) => {
        const userRole = getUserRole(user)
        setUserContext(user.id, user.name, userRole)

        if (userRole !== 'none') {
          showSideBar.value = true
        } else if (router.currentRoute.value.path !== '/disableduser') {
          router.push('/disableduser')
        }
      },
      () => {
        showSideBar.value = false
        clearUserContext()

        if (router.currentRoute.value.meta.requiresAuth) {
          location.href = `/signin?returnUrl=${encodeURIComponent(location.toString())}`
        }
      },
    )
  } finally {
    hasLoaded.value = true
  }
})
</script>

<template>
  <div class="flex h-screen w-screen flex-col items-center justify-center" v-if="!hasLoaded">
    <KeepiSpinner />
  </div>

  <TooltipProvider v-else-if="!showSideBar">
    <RouterView v-slot="{ Component, route }">
      <template v-if="Component != null && !route.meta.requiresAuth">
        <!-- Using a transition here causes the error <TypeError: can't access property "nextSibling", node is null> -->
        <Suspense timeout="0">
          <component :is="Component"></component>

          <template #fallback>
            <div class="flex grow flex-col items-center justify-center">
              <KeepiSpinner />
            </div>
          </template>
        </Suspense>
      </template>
    </RouterView>
  </TooltipProvider>

  <!-- The SidebarProvider also functions as the tooltip provider -->
  <SidebarProvider v-else>
    <KeepiSidebar />

    <Toaster />

    <div class="flex min-h-screen w-full flex-col lg:max-w-5xl">
      <div class="h-14 max-w-screen px-4 py-2">
        <div class="flex flex-nowrap items-center justify-between space-x-2">
          <div class="flex space-x-4">
            <SidebarTrigger class="lg:hidden" />

            <Button class="h-10 w-10" variant="outline" size="icon" @click="onToggleTheme">
              <Transition name="fade" mode="out-in">
                <Moon :size="48" v-if="mode === 'light'" />
                <Sun :size="48" v-else />
              </Transition>
              <span class="sr-only">Toggle theme</span>
            </Button>
          </div>

          <div class="block w-auto">
            <div
              id="header-bar"
              class="mt-0 flex flex-row space-x-3 rounded-lg border-0 p-0 font-medium sm:space-x-8 rtl:space-x-reverse">
              <!-- Content is put here using Teleport -->
            </div>
          </div>
        </div>
      </div>

      <RouterView v-slot="{ Component }">
        <template v-if="Component != null">
          <!-- Using a transition here causes the error <TypeError: can't access property "nextSibling", node is null> -->
          <Suspense timeout="0">
            <component class="p-4" :is="Component"></component>

            <template #fallback>
              <div class="flex grow flex-col items-center justify-center">
                <KeepiSpinner />
              </div>
            </template>
          </Suspense>

          <div class="grow"></div>
        </template>

        <div v-else class="grow"></div>
      </RouterView>

      <footer class="mb-2 max-w-screen text-center text-sm">
        <p>
          Opmerkingen of suggesties? Laat
          <a class="cursor-pointer text-blue-500" href="https://github.com/TijmenUU/keepi/issues"
            >hier</a
          >
          je feedback achter.
        </p>
        <p class="text-xs">{{ buildDate }}+{{ buildCommit }}</p>
      </footer>
    </div>
  </SidebarProvider>
</template>

<style>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
