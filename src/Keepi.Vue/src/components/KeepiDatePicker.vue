<script setup lang="ts">
import {
  CalendarDate,
  DateFormatter,
  getLocalTimeZone,
  parseDate,
  today,
  type DateValue,
} from '@internationalized/date'

import { CalendarIcon } from 'lucide-vue-next'
import { cn } from '@/lib/utils'
import { Button } from '@/components/ui/button'
import { Calendar } from '@/components/ui/calendar'
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover'
import { computed } from 'vue'

const props = defineProps<{
  // Expects an ISO 8601 date string (yyyy-mm-dd)
  modelValue?: string
}>()

const emits = defineEmits<{
  (e: 'update:modelValue', value?: string): void
}>()

defineOptions({
  inheritAttrs: false,
})

const defaultPlaceholder = today(getLocalTimeZone())
const date = computed<CalendarDate | undefined>(() => {
  if (props.modelValue != null) {
    return parseDate(props.modelValue)
  }

  return undefined
})

const onModelValueUpdate = (value: DateValue | undefined, popoverCallback: () => void) => {
  emits('update:modelValue', value?.toString())
  popoverCallback()
}

const df = new DateFormatter('nl-NL', {
  dateStyle: 'long',
})
</script>

<template>
  <Popover v-slot="{ close }">
    <PopoverTrigger as-child>
      <Button
        v-bind="$attrs"
        variant="outline"
        :class="cn('w-60 justify-start text-left font-normal', !date && 'text-muted-foreground')">
        <CalendarIcon />
        {{ date ? df.format(date.toDate(getLocalTimeZone())) : 'Kies een datum' }}
      </Button>
    </PopoverTrigger>

    <PopoverContent class="w-auto p-0" align="start">
      <Calendar
        :model-value="date"
        :default-placeholder="defaultPlaceholder"
        layout="month-and-year"
        initial-focus
        @update:model-value="(value) => onModelValueUpdate(value, close)" />
    </PopoverContent>
  </Popover>
</template>
