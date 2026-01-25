<script setup lang="ts">
import ApiClient, { type IGetWeekUserEntriesResponse } from '@/api-client'
import KeepiWeekEditor from '@/components/KeepiWeekEditor.vue'
import { type DateRange, getDateOfIsoWeek, getWeekDaysForDate, getWeekNumber } from '@/date'
import { router } from '@/router'
import { computed, ref, watch } from 'vue'
import { handleApiError } from '@/error'

const props = defineProps<{
  year: string | null | undefined
  weekNumber: string | null | undefined
}>()

const apiClient = new ApiClient()
const dateRange = computed<DateRange>(() => {
  if (props.year == null || props.weekNumber == null) {
    console.error(`Year weeknumber combination is not valid ${props.weekNumber} ${props.year}`)
    replaceRouteWithToday()
    return getWeekDaysForDate(new Date())
  }

  const parsedYear = parseInt(props.year ?? '0')
  const parsedWeekNumber = parseInt(props.weekNumber ?? '0')
  if (
    isNaN(parsedYear) ||
    isNaN(parsedWeekNumber) ||
    parsedYear < 0 ||
    parsedWeekNumber < 1 ||
    parsedWeekNumber > 53
  ) {
    console.error(`Year weeknumber combination is not valid ${props.weekNumber} ${props.year}`)
    replaceRouteWithToday()
    return getWeekDaysForDate(new Date())
  }

  try {
    return getWeekDaysForDate(getDateOfIsoWeek(parsedWeekNumber, parsedYear))
  } catch (error) {
    console.error(
      `Year weeknumber combination is not valid ${props.weekNumber} ${props.year}`,
      error,
    )
    replaceRouteWithToday()
    return getWeekDaysForDate(new Date())
  }
})

async function replaceRouteWithToday() {
  const today = new Date()
  const newWeekNumber = getWeekNumber(today)
  await router.replace(`/input/year/${today.getFullYear()}/week/${newWeekNumber}`)
}

const userProjects = await apiClient.getUserProjects().match(
  (result) => result.projects,
  (error) => {
    handleApiError(error)
    return []
  },
)

const editorVersion = ref(0)
const isReloading = ref(false)
const weekEntries = ref<IGetWeekUserEntriesResponse>(
  await apiClient.getWeekUserEntries(dateRange.value.year, dateRange.value.weekNumber).match(
    (result) => result,
    (error) => {
      handleApiError(error)

      return <IGetWeekUserEntriesResponse>{
        monday: { entries: [] },
        tuesday: { entries: [] },
        wednesday: { entries: [] },
        thursday: { entries: [] },
        friday: { entries: [] },
        saturday: { entries: [] },
        sunday: { entries: [] },
      }
    },
  ),
)

const onReload = async () => {
  if (isReloading.value) {
    return
  }
  isReloading.value = true

  try {
    weekEntries.value = await getWeekUserEntries()
    editorVersion.value += 1
  } finally {
    isReloading.value = false
  }
}

const getWeekUserEntries = async () => {
  isReloading.value = true

  const result = await apiClient
    .getWeekUserEntries(dateRange.value.year, dateRange.value.weekNumber)
    .match(
      (result) => result,
      (error) => {
        handleApiError(error)

        return <IGetWeekUserEntriesResponse>{
          monday: { entries: [] },
          tuesday: { entries: [] },
          wednesday: { entries: [] },
          thursday: { entries: [] },
          friday: { entries: [] },
          saturday: { entries: [] },
          sunday: { entries: [] },
        }
      },
    )

  isReloading.value = false

  return result
}

// When navigating to a different week whilst already viewing a week Vue router
// will re-use the current component and only change the props.
// Thus by watching the prop changes through dateRange we can ensure the changed
// week is fetched.
watch(dateRange, onReload)

const gotoWeek = async (weekNumber: number, year: number) => {
  await router.push(`/input/year/${year}/week/${weekNumber}`)
}

const onPreviousWeek = async () => {
  const firstDayOfPreviousWeek = new Date(dateRange.value.dates[0])
  firstDayOfPreviousWeek.setDate(firstDayOfPreviousWeek.getDate() - 1)
  const newWeekNumber = getWeekNumber(firstDayOfPreviousWeek)
  await gotoWeek(newWeekNumber, firstDayOfPreviousWeek.getFullYear())
}

const onToday = async () => {
  const today = new Date()
  const newWeekNumber = getWeekNumber(today)
  await gotoWeek(newWeekNumber, today.getFullYear())
}

const onNextWeek = async () => {
  const firstDayOfNextWeek = new Date(dateRange.value.dates[6])
  firstDayOfNextWeek.setDate(firstDayOfNextWeek.getDate() + 1)
  const newWeekNumber = getWeekNumber(firstDayOfNextWeek)
  await gotoWeek(newWeekNumber, firstDayOfNextWeek.getFullYear())
}
</script>

<template>
  <KeepiWeekEditor
    :projects="userProjects"
    :date-range="dateRange"
    :key="editorVersion"
    :week-entries="weekEntries"
    :disabled="isReloading"
    @reload="onReload"
    @current-week="onToday"
    @previous-week="onPreviousWeek"
    @next-week="onNextWeek" />
</template>
