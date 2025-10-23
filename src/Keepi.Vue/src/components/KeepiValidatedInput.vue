<script setup lang="ts">
import type { RegleFieldStatus } from '@regle/core'
import { computed } from 'vue'
import Popper from 'vue3-popper'
import { useAttrs } from 'vue'

defineOptions({
  inheritAttrs: false,
})

const model = defineModel<string>({ required: true })

const props = defineProps<{
  forceShowError: boolean
  field: RegleFieldStatus
  id: string
}>()

const emits = defineEmits<{
  (e: 'keydown', ev: KeyboardEvent): void
}>()

const errorMessage = computed<string>(() => {
  if (props.field.$errors.length > 0) {
    return props.field.$errors[0]
  }
  return ''
})

const hasError = computed<boolean>(() => props.field.$error || props.forceShowError)
</script>

<template>
  <div>
    <Popper
      :content="errorMessage"
      :show="hasError && errorMessage !== ''"
      placement="top"
      class="error-popup"
      arrow
      open-delay="100"
      close-delay="100"
    >
      <input
        class="w-full rounded-md border border-gray-500"
        :class="{ 'border-red-500': hasError }"
        v-bind="useAttrs()"
        :id="props.id"
        type="text"
        v-model="model"
        @keydown="emits('keydown', $event)"
      />
    </Popper>
  </div>
</template>
