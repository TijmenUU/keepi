<script setup lang="ts">
import type { RegleFieldStatus } from '@regle/core'
import { computed } from 'vue'
import { useAttrs } from 'vue'
import Input from '@/components/ui/input/Input.vue'
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip'

defineOptions({
  inheritAttrs: false,
})

const model = defineModel<string>({ required: true })

const props = defineProps<{
  field: RegleFieldStatus
  id: string
}>()

const emits = defineEmits<{
  (e: 'keydown', ev: KeyboardEvent): void
}>()

const errorMessage = computed<string>(() => {
  if (props.field.$errors.length > 0) {
    return props.field.$errors[0].trim()
  }
  return ''
})

const hasErrorMessage = computed<boolean>(() => {
  return errorMessage.value !== ''
})
</script>

<template>
  <Tooltip :disabled="!hasErrorMessage">
    <TooltipTrigger as-child>
      <Input
        :class="{ 'border-red-500': props.field.$error }"
        v-bind="useAttrs()"
        :id="props.id"
        type="text"
        v-model="model"
        @keydown="emits('keydown', $event)" />
    </TooltipTrigger>

    <TooltipContent>
      {{ errorMessage }}
    </TooltipContent>
  </Tooltip>
</template>
