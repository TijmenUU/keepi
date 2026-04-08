<script setup lang="ts">
import type { RegleFieldStatus } from '@regle/core'
import { computed, ref } from 'vue'
import { useAttrs } from 'vue'
import Input from '@/components/ui/input/Input.vue'
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip'
import { useMediaQuery } from '@vueuse/core'

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

const mediaQueryTouch = useMediaQuery('(pointer: coarse)')
const tooltipOpen = ref<undefined | boolean>()
</script>

<template>
  <Tooltip
    :disabled="!hasErrorMessage"
    :open="tooltipOpen && hasErrorMessage"
    disable-closing-trigger>
    <TooltipTrigger
      @focus="tooltipOpen = true"
      @blur="tooltipOpen = false"
      @mouseenter="mediaQueryTouch && (tooltipOpen = true)"
      @mouseleave="mediaQueryTouch && (tooltipOpen = false)"
      as-child>
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
