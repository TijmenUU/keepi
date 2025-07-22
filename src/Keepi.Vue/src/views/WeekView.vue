<script setup lang="ts">
import ApiClient, { type IGetWeekUserEntriesResponse } from '@/api-client'
import KeepiButton from '@/components/KeepiButton.vue'
import WeekEditor from '@/components/WeekEditor.vue'
import { type DateRange, getWeekDaysFor, getWeekNumber } from '@/date'
import { toShortDutchDate } from '@/format'
import { loggableDays } from '@/types'
import { computed, ref } from 'vue'

const apiClient = new ApiClient()
const dateRange = ref<DateRange>(getWeekDaysFor(new Date()))
const currentWeek = getWeekNumber(new Date())

const editorVersion = ref(0)
const isReloading = ref(false)
const weekEntries = ref<IGetWeekUserEntriesResponse>(
  await apiClient.getWeekUserEntries(dateRange.value.year, dateRange.value.weekNumber).match(
    (result) => result,
    (error) =>
      // TODO report this error to the user? Or at least refresh, redirect or retry?
      <IGetWeekUserEntriesResponse>{
        monday: { entries: [] },
        tuesday: { entries: [] },
        wednesday: { entries: [] },
        thursday: { entries: [] },
        friday: { entries: [] },
        saturday: { entries: [] },
        sunday: { entries: [] },
      },
  ),
)

// TODO rework the components to use the ApiClient

const dateRangeDescription = computed<string>(
  () =>
    `${toShortDutchDate(dateRange.value.dates[0])} t/m ${toShortDutchDate(
      dateRange.value.dates[loggableDays.length - 1],
    )}`,
)

const onReload = async () => {
  if (isReloading.value) {
    return
  }

  isReloading.value = true

  weekEntries.value = await getWeekUserEntries()
  editorVersion.value += 1

  isReloading.value = false
}

const getWeekUserEntries = async () => {
  return await apiClient.getWeekUserEntries(dateRange.value.year, dateRange.value.weekNumber).match(
    (result) => result,
    (error) =>
      // TODO report this error to the user? Or at least refresh, redirect or retry?
      <IGetWeekUserEntriesResponse>{
        monday: { entries: [] },
        tuesday: { entries: [] },
        wednesday: { entries: [] },
        thursday: { entries: [] },
        friday: { entries: [] },
        saturday: { entries: [] },
        sunday: { entries: [] },
      },
  )
}

const onPreviousWeek = async () => {
  const lastDatePreviousWeek = new Date(dateRange.value.dates[0])
  lastDatePreviousWeek.setDate(lastDatePreviousWeek.getDate() - 1)
  dateRange.value = getWeekDaysFor(lastDatePreviousWeek)

  await onReload()
}

const onToday = async () => {
  dateRange.value = getWeekDaysFor(new Date())
  await onReload()
}

const onNextWeek = async () => {
  const firstDateNextWeek = new Date(dateRange.value.dates[dateRange.value.dates.length - 1])
  firstDateNextWeek.setDate(firstDateNextWeek.getDate() + 1)
  dateRange.value = getWeekDaysFor(firstDateNextWeek)

  await onReload()
}
</script>

<template>
  <div class="mx-auto flex flex-col items-center py-3 lg:container">
    <h2 class="text-2xl">Week {{ dateRange.weekNumber }}</h2>

    <p class="text-gray-500">{{ dateRangeDescription }}</p>

    <div class="flex space-x-2 py-3">
      <KeepiButton @click="onPreviousWeek">Vorige</KeepiButton>
      <KeepiButton :disabled="currentWeek === dateRange.weekNumber" @click="onToday">
        Vandaag
      </KeepiButton>
      <KeepiButton @click="onNextWeek">Volgende</KeepiButton>
    </div>

    <WeekEditor
      class="mt-3"
      :date-range="dateRange"
      :key="editorVersion"
      :week-entries="weekEntries"
      :disabled="isReloading"
      @reload="onReload"
    />
  </div>
</template>
