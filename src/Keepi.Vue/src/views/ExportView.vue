<script setup lang="ts">
import ApiClient from '@/api-client'
import KeepiDatePicker from '@/components/KeepiDatePicker.vue'
import Button from '@/components/ui/button/Button.vue'
import Label from '@/components/ui/label/Label.vue'
import { getWeekDaysForDate } from '@/date'
import { toShortIsoDate, tryParseIsoDate } from '@/format'
import { requiredValidator } from '@/regle'
import { handleApiError } from '@/router'
import { useRegle } from '@regle/core'
import { withMessage } from '@regle/rules'
import { ref } from 'vue'

const apiClient = new ApiClient()
const disableUserInteraction = ref(false)

const currentWeek = getWeekDaysForDate(new Date())
const formValues = ref({
  from: toShortIsoDate(currentWeek.dates[0]),
  to: toShortIsoDate(currentWeek.dates[currentWeek.dates.length - 1]),
})

const { r$ } = useRegle(formValues, {
  from: {
    required: requiredValidator,
  },
  to: {
    required: requiredValidator,
    dateAfter: withMessage((value) => {
      if (value == null || typeof value !== 'string') {
        return true
      }

      const from = tryParseIsoDate(formValues.value.from)
      if (from == null) {
        return true
      }

      const to = tryParseIsoDate(value)
      // The date validation should prevent this, but if it happens this
      // validation should not care
      if (to == null) {
        return true
      }

      return to >= from
    }, 'Datum tot en met mag niet voor vanaf vallen'),
  },
})

const onSubmit = async () => {
  if (disableUserInteraction.value) {
    return
  }

  disableUserInteraction.value = true

  try {
    const { valid, data } = await r$.$validate()
    if (valid) {
      const from = tryParseIsoDate(data.from)
      const to = tryParseIsoDate(data.to)
      if (from == null || to == null) {
        console.error('Malformed submission data in submit handler', data)
        return
      }

      await apiClient.getUserEntriesExport(from, to).match(
        (result) => {
          const downloadUrl = URL.createObjectURL(result.blob)
          const link: HTMLAnchorElement = document.createElement('a')
          link.href = downloadUrl
          link.download = result.fileName
          link.click()
          URL.revokeObjectURL(downloadUrl)
        },
        (error) => {
          handleApiError(error)
        },
      )
    }
  } finally {
    disableUserInteraction.value = false
  }
}
</script>

<template>
  <div class="inline-block">
    <div class="flex flex-wrap gap-2">
      <Label>
        Van
        <KeepiDatePicker v-model="formValues.from" autofocus />
      </Label>

      <Label>
        tot en met
        <KeepiDatePicker v-model="formValues.to" />
      </Label>
    </div>

    <div class="col-span-2 mt-4 sm:flex sm:justify-end">
      <Button @click="onSubmit" class="w-full sm:w-auto" :disabled="r$.$invalid">
        Exporteer
      </Button>
    </div>
  </div>
</template>
