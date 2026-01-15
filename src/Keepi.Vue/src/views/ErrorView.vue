<script setup lang="ts">
import KeepiFatalError from '@/components/KeepiFatalError.vue'
import { X } from 'lucide-vue-next'
</script>

<template>
  <div class="p-4">
    <KeepiFatalError
      class="mx-auto"
      :icon="X"
      message="Er ging iets helemaal mis, probeer het later nogmaals." />
  </div>
</template>
