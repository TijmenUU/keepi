<script setup lang="ts">
import { useAttrs } from 'vue'

defineOptions({
  inheritAttrs: false,
})

const props = withDefaults(
  defineProps<{
    name?: string
    type?: 'text' | 'number'
    modelValue: string
    readonly?: boolean
    tabIndex?: 0 | -1
    autofocus?: boolean
  }>(),
  {
    name: '',
    type: 'text',
    readonly: false,
    tabIndex: 0,
    autofocus: false,
  },
)

const emits = defineEmits<{
  (e: 'update:modelValue', value: string): void
  (e: 'keyup', value: KeyboardEvent): void
}>()
</script>

<template>
  <input
    class="rounded border-b border-gray-600 ring-gray-400 focus:ring-2 focus:outline-none"
    :name="props.name"
    :type="props.type"
    :value="props.modelValue"
    :autofocus="props.autofocus"
    v-bind="useAttrs()"
    @input="emits('update:modelValue', ($event.target as HTMLInputElement)?.value ?? '')"
    @keyup="emits('keyup', $event)"
    :readonly="props.readonly"
    :tabindex="props.tabIndex"
  />
</template>
