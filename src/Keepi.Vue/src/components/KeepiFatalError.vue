<script setup lang="ts">
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import type { LucideIcon } from 'lucide-vue-next'

const props = withDefaults(
  defineProps<{
    icon: LucideIcon
    message: string
    recoveryButtonHref?: string
    recoveryButtonLabel?: string
  }>(),
  {
    recoveryButtonHref: '/',
    recoveryButtonLabel: 'Terug naar start',
  },
)
</script>

<template>
  <Card class="w-full max-w-sm">
    <CardContent>
      <component :is="props.icon" class="mx-auto h-24 w-24 text-gray-300 dark:text-gray-700" />

      <p class="mt-2">{{ props.message }}</p>

      <div class="mt-2 flex justify-end">
        <Button as="a" :href="props.recoveryButtonHref" variant="outline">
          {{ props.recoveryButtonLabel }}
        </Button>
      </div>
    </CardContent>
  </Card>
</template>
