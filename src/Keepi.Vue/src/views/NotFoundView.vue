<script setup lang="ts">
import { FileQuestionMark } from 'lucide-vue-next'
import KeepiFatalError from '@/components/KeepiFatalError.vue'
</script>

<template>
  <div class="p-4">
    <KeepiFatalError
      class="mx-auto"
      :icon="FileQuestionMark"
      message="De opgevraagde pagina bestaat niet." />
  </div>
</template>
