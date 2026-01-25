<script setup lang="ts">
import KeepiFatalError from '@/components/KeepiFatalError.vue'
import { ShieldX } from 'lucide-vue-next'
</script>

<template>
  <div class="p-4">
    <KeepiFatalError
      class="mx-auto"
      :icon="ShieldX"
      message="Je account is gedeactiveerd. Om toegang te krijgen neem contact op met de beheerder van deze applicatie."
      :show-recovery-button="false" />
  </div>
</template>
