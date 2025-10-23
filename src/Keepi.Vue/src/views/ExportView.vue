<script setup lang="ts">
import ApiClient from '@/api-client'
import KeepiButton from '@/components/KeepiButton.vue'
import KeepiValidatedInput from '@/components/KeepiValidatedInput.vue'
import { getWeekDaysFor } from '@/date'
import { toShortDutchDate, tryParseDutchDate } from '@/format'
import { useRegle } from '@regle/core'
import { required, withMessage } from '@regle/rules'
import { ref } from 'vue'

const apiClient = new ApiClient()
const disableUserInteraction = ref(false)
const hasSubmitted = ref(false)

const currentWeek = getWeekDaysFor(new Date())
const formValues = ref({
  from: toShortDutchDate(currentWeek.dates[0]),
  to: toShortDutchDate(currentWeek.dates[currentWeek.dates.length - 1]),
})

const { r$ } = useRegle(formValues, {
  from: {
    date: withMessage(isValidDate, 'Geen geldige datum'),
    required: withMessage(required, 'Datum vanaf is verplicht'),
  },
  to: {
    date: withMessage(isValidDate, 'Geen geldige datum'),
    required: withMessage(required, 'Datum tot en met is verplicht'),
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

function isValidDate(value: unknown): boolean {
  if (value == null || typeof value !== 'string') {
    return true
  }

  return tryParseDutchDate(value) != null
}

const onSubmit = async () => {
  hasSubmitted.value = true

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
        console.debug('Malformed submission data', data)
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
        // TODO notify the user of this error?
        () => {},
      )
    }
  } finally {
    disableUserInteraction.value = false
  }
}
</script>

<template>
  <div class="container mx-auto flex max-w-screen-lg flex-col items-center gap-y-3 px-2 py-3">
    <label>
      Vanaf
      <!--
        TODO Make this input date compatible so the UX from the frontend
        is more enjoyable. See the other date input below as well.
      -->
      <KeepiValidatedInput
        :force-show-error="hasSubmitted"
        :field="r$.$fields.from"
        id="from"
        v-model="formValues.from"
        autofocus
      />
    </label>

    <label>
      Tot en met
      <KeepiValidatedInput
        :force-show-error="hasSubmitted"
        :field="r$.$fields.to"
        id="to"
        v-model="formValues.to"
      />
    </label>

    <KeepiButton @click="onSubmit"> Exporteer </KeepiButton>
  </div>
</template>
