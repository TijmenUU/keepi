<script setup lang="ts">
import KeepiFatalError from '@/components/KeepiFatalError.vue'
import { HatGlasses } from 'lucide-vue-next'
</script>

<template>
  <div class="p-4">
    <KeepiFatalError
      class="mx-auto"
      :icon="HatGlasses"
      message="Je bent uitgelogd."
      recovery-button-href="/"
      recovery-button-label="Opnieuw inloggen" />
  </div>
</template>
