<script setup lang="ts">
import ApiClient from '@/api-client'
import KeepiValidatedInput from '@/components/KeepiValidatedInput.vue'
import Button from '@/components/ui/button/Button.vue'
import Label from '@/components/ui/label/Label.vue'
import { getWeekDaysForDate } from '@/date'
import { toShortDutchDate, tryParseDutchDate } from '@/format'
import { dateValidator, requiredValidator } from '@/regle'
import { handleApiError } from '@/router'
import { useRegle } from '@regle/core'
import { withMessage } from '@regle/rules'
import { ref } from 'vue'

const apiClient = new ApiClient()
const disableUserInteraction = ref(false)

const currentWeek = getWeekDaysForDate(new Date())
const formValues = ref({
  from: toShortDutchDate(currentWeek.dates[0]),
  to: toShortDutchDate(currentWeek.dates[currentWeek.dates.length - 1]),
})

const { r$ } = useRegle(formValues, {
  from: {
    date: dateValidator,
    required: requiredValidator,
  },
  to: {
    date: dateValidator,
    required: requiredValidator,
    dateAfter: withMessage((value) => {
      if (value == null || typeof value !== 'string') {
        return true
      }

      const from = tryParseDutchDate(formValues.value.from)
      if (from == null) {
        return true
      }

      const to = tryParseDutchDate(value)
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
      const from = tryParseDutchDate(data.from)
      const to = tryParseDutchDate(data.to)
      if (from == null || to == null) {
        throw new Error('Malformed submission data in submit handler')
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
    <div class="flex space-x-2">
      <Label>
        Van
        <!--
        TODO Make this input date compatible so the UX from the frontend
        is more enjoyable. See the other date input below as well.
      -->
        <KeepiValidatedInput
          :field="r$.$fields.from"
          class="max-w-28"
          id="from"
          v-model="formValues.from"
          placeholder="dd-mm-jjjj"
          autofocus />
      </Label>

      <Label>
        tot en met
        <KeepiValidatedInput
          :field="r$.$fields.to"
          class="max-w-28"
          id="to"
          v-model="formValues.to"
          placeholder="dd-mm-jjjj" />
      </Label>
    </div>

    <div class="col-span-2 mt-4 sm:flex sm:justify-end">
      <Button @click="onSubmit" class="w-full sm:w-auto" :disabled="r$.$invalid">
        Exporteer
      </Button>
    </div>
  </div>
</template>
