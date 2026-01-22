<script setup lang="ts">
import { Download, Timer, ListChecks, Brush, Users } from 'lucide-vue-next'
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from '@/components/ui/sidebar'
import type { NavigationFailure } from 'vue-router'
import { getUserContext } from '@/user-context'

const userContext = getUserContext()
const items = [
  {
    title: 'Invoer',
    url: '/',
    icon: Timer,
  },
  {
    title: 'Personalisatie',
    url: '/customizations',
    icon: Brush,
  },
]

if (userContext.role === 'admin') {
  items.push(
    {
      title: 'Export',
      url: '/export',
      icon: Download,
    },
    {
      title: 'Projecten',
      url: '/projects',
      icon: ListChecks,
    },
    {
      title: 'Gebruikers',
      url: '/users',
      icon: Users,
    },
  )
} else if (userContext.role === 'none') {
  items.splice(0) // clear the array
}

const { isMobile, setOpenMobile } = useSidebar()
const onNavigation = async (
  ev: PointerEvent,
  navigateFn: (e?: MouseEvent) => Promise<void | NavigationFailure>,
) => {
  if (isMobile.value) {
    setOpenMobile(false)
  }

  await navigateFn(ev)
}
</script>

<template>
  <Sidebar>
    <div class="p-4">
      <div class="flex items-center space-x-3 rtl:space-x-reverse">
        <img src="/keepi.svg" class="h-6 w-6" alt="Keepi Logo" />
        <span class="self-center text-2xl font-semibold whitespace-nowrap"> Keepi </span>
      </div>
    </div>

    <SidebarContent>
      <SidebarGroup>
        <SidebarGroupContent>
          <SidebarMenu>
            <SidebarMenuItem v-for="item in items" :key="item.title">
              <router-link custom :to="item.url" v-slot="{ isExactActive, href, navigate }">
                <SidebarMenuButton as-child :is-active="isExactActive">
                  <a
                    :class="{ 'cursor-auto': isExactActive, 'cursor-pointer': !isExactActive }"
                    :href="href"
                    @click="(ev) => onNavigation(ev, navigate)">
                    <component :is="item.icon" />
                    <span class="text-lg">{{ item.title }}</span>
                  </a>
                </SidebarMenuButton>
              </router-link>
            </SidebarMenuItem>
          </SidebarMenu>
        </SidebarGroupContent>
      </SidebarGroup>
    </SidebarContent>
  </Sidebar>
</template>
