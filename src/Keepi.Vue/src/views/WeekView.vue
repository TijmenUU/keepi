<script setup lang="ts">
import ApiClient, {
  type IGetWeekUserEntriesResponse,
  type IPutUpdateWeekUserEntriesRequestDay,
} from '@/api-client'
import KeepiButton from '@/components/KeepiButton.vue'
import WeekEditor from '@/components/WeekEditor.vue'
import { type DateRange, getWeekDaysFor, getWeekNumber } from '@/date'
import { toShortDutchDate } from '@/format'
import { loggableDays, type TimeTableEntry } from '@/types'
import { computed, ref } from 'vue'

const apiClient = new ApiClient()
const dateRange = ref<DateRange>(getWeekDaysFor(new Date()))
const currentWeek = getWeekNumber(new Date())

const editorVersion = ref(0)
const disableUserInteraction = ref(false)
const weekEntries = ref<IGetWeekUserEntriesResponse>(
  await apiClient.getWeekUserEntries(dateRange.value.year, dateRange.value.weekNumber).match(
    (result) => result,
    () =>
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

const onReload = async (skipDisableUserInteractionCheck?: boolean) => {
  if (!skipDisableUserInteractionCheck) {
    if (disableUserInteraction.value) {
      return
    }
    disableUserInteraction.value = true
  }

  weekEntries.value = await getWeekUserEntries()
  editorVersion.value += 1

  disableUserInteraction.value = false
}

const onSubmit = async (entries: TimeTableEntry[]) => {
  if (disableUserInteraction.value) {
    return
  }

  disableUserInteraction.value = true

  await apiClient
    .updateWeekUserEntries(dateRange.value.year, dateRange.value.weekNumber, {
      monday: getUpdateWeekUserEntriesRequestDay(0, entries),
      tuesday: getUpdateWeekUserEntriesRequestDay(1, entries),
      wednesday: getUpdateWeekUserEntriesRequestDay(2, entries),
      thursday: getUpdateWeekUserEntriesRequestDay(3, entries),
      friday: getUpdateWeekUserEntriesRequestDay(4, entries),
      saturday: getUpdateWeekUserEntriesRequestDay(5, entries),
      sunday: getUpdateWeekUserEntriesRequestDay(6, entries),
    })
    .match(
      () => {},
      () => {
        // TODO handle this gracefully without losing the user input preferably
      },
    )

  await onReload(true)
}

function getUpdateWeekUserEntriesRequestDay(dayIndex: number, entries: TimeTableEntry[]) {
  return <IPutUpdateWeekUserEntriesRequestDay>{
    entries: entries
      .filter((e) => e.dayIndex === dayIndex && e.minutes != 0)
      .map((e) => ({
        entryCategoryId: e.userEntryCategoryId,
        minutes: e.minutes,
      })),
  }
}

const getWeekUserEntries = async () => {
  disableUserInteraction.value = true

  const result = await apiClient
    .getWeekUserEntries(dateRange.value.year, dateRange.value.weekNumber)
    .match(
      (result) => result,
      () =>
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

  disableUserInteraction.value = false

  return result
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
  <div class="mx-auto max-w-full p-4">
    <div
      class="relative max-w-3xl flex-col items-center space-y-4 transition duration-200"
      :class="{ 'blur-xs': disableUserInteraction }"
    >
      <div
        class="absolute top-0 left-0 z-10 h-full w-full cursor-not-allowed"
        v-if="disableUserInteraction"
      ></div>

      <div class="flex justify-center space-x-2">
        <KeepiButton @click="onPreviousWeek">Vorige week</KeepiButton>
        <KeepiButton :disabled="currentWeek === dateRange.weekNumber" @click="onToday">
          Toon vandaag
        </KeepiButton>
        <KeepiButton @click="onNextWeek">Volgende week</KeepiButton>
      </div>

      <div class="text-center">
        <h2 class="text-2xl">Week {{ dateRange.weekNumber }}</h2>
        <p class="text-gray-500">{{ dateRangeDescription }}</p>
      </div>

      <WeekEditor
        :date-range="dateRange"
        :key="editorVersion"
        :week-entries="weekEntries"
        :disabled="disableUserInteraction"
        @submit="onSubmit"
      />
    </div>
  </div>
</template>
