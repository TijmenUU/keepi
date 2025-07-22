<script setup lang="ts">
import type {
  IGetUserEntryCategoriesResponse,
  IGetWeekUserEntriesResponse,
  IGetWeekUserEntriesResponseDay,
  IPutUpdateWeekUserEntriesRequestDay,
} from '@/api-client'
import ApiClient from '@/api-client'
import KeepiButton from '@/components/KeepiButton.vue'
import KeepiInput from '@/components/KeepiInput.vue'
import type { DateRange } from '@/date'
import { toHoursMinutesNotation, tryParseTimeNotation, toShortDutchDate } from '@/format'
import { type LoggableDay, loggableDays, type TimeTableEntry } from '@/types'
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'

const apiClient = new ApiClient()
const router = useRouter()
const isSaving = ref(false)

const props = defineProps<{
  dateRange: DateRange
  weekEntries: IGetWeekUserEntriesResponse
  disabled: boolean
}>()

const emits = defineEmits<{
  (e: 'reload'): void
}>()

// TODO consider sharing this piece of error handling logic between components instead of duplicating it
const userEntryCategories = await apiClient.getAllUserEntryCategories().match(
  (result) => {
    const categories = result.categories
    categories.sort((a, b) => a.ordinal - b.ordinal)
    return categories
  },
  (error) => {
    if (error === 'unauthorized' || error === 'forbidden') {
      location.pathname = '/signin' // TODO add the current url as redirect url
    } else {
      location.pathname = '/error'
    }

    return <IGetUserEntryCategoriesResponse['categories']>[]
  },
)
const timeTableEntries = ref<TimeTableEntry[]>([
  ...getTimeTableEntriesForDay(0, props.weekEntries.monday),
  ...getTimeTableEntriesForDay(1, props.weekEntries.tuesday),
  ...getTimeTableEntriesForDay(2, props.weekEntries.wednesday),
  ...getTimeTableEntriesForDay(3, props.weekEntries.thursday),
  ...getTimeTableEntriesForDay(4, props.weekEntries.friday),
  ...getTimeTableEntriesForDay(5, props.weekEntries.saturday),
  ...getTimeTableEntriesForDay(6, props.weekEntries.sunday),
])

function getTimeTableEntriesForDay(dayIndex: number, dayEntries: IGetWeekUserEntriesResponseDay) {
  return userEntryCategories.map<TimeTableEntry>((category) => {
    const initialMinutes = dayEntries.entries
      .filter((e) => e.entryCategoryId == category.id)
      .reduce((previous, current) => previous + current.minutes, 0)
    return {
      userEntryCategoryId: category.id,
      dayIndex: dayIndex,
      initialMinutes: initialMinutes,
      inputMinutes: initialMinutes,
    }
  })
}

const inputValues = ref<string[]>(
  timeTableEntries.value.map((v) => toHoursMinutesNotation(v.inputMinutes)),
)
const inputValidationMode = ref<'optional-time' | undefined>(undefined)

const onDayInput = (value: string, index: number) => {
  if (index < 0 || index > inputValues.value.length) {
    console.error(`Input index ${index} is out of range`)
    return
  }

  inputValues.value[index] = value

  const parsedValue = tryParseTimeNotation(value) ?? 0
  timeTableEntries.value[index].inputMinutes = parsedValue
}

const daySummaries = computed<string[]>(() => {
  const minutesPerDay: number[] = []
  for (let i = 0; i < loggableDays.length; ++i) {
    const inputeMatches = timeTableEntries.value.filter((v) => v.dayIndex === i)
    minutesPerDay.push(
      inputeMatches.reduce<number>((previous, current) => previous + current.inputMinutes, 0),
    )
  }

  return minutesPerDay.map((m) => toHoursMinutesNotation(m))
})

const categorySummaries = computed<string[]>(() => {
  const minutesPerProject: number[] = []
  for (let i = 0; i < userEntryCategories.length; ++i) {
    const category = userEntryCategories[i]
    const matches = timeTableEntries.value.filter((v) => v.userEntryCategoryId === category.id)
    minutesPerProject.push(
      matches.reduce<number>((previous, current) => previous + current.inputMinutes, 0),
    )
  }

  return minutesPerProject.map((m) => toHoursMinutesNotation(m))
})

const grandTotal = computed<string>(() => {
  const totalMinutes = timeTableEntries.value.reduce<number>(
    (previous, current) => previous + current.inputMinutes,
    0,
  )
  return toHoursMinutesNotation(totalMinutes)
})

const today = computed<LoggableDay | null>(() => {
  const today = new Date()
  const dateIndex = props.dateRange.dates.findIndex(
    (d) =>
      d.getUTCFullYear() === today.getUTCFullYear() &&
      d.getUTCMonth() === today.getUTCMonth() &&
      d.getUTCDate() === today.getUTCDate(),
  )
  if (dateIndex < 0) {
    return null
  }
  return loggableDays[dateIndex]
})

const onKey = (direction: 'up' | 'down') => {
  if (!(document.activeElement instanceof HTMLInputElement)) {
    return
  }

  const currentInputName = document.activeElement.name
  if (!currentInputName || currentInputName.match(/^dayinput-[0-9]+$/) == null) {
    console.debug(`Ignoring input key event on unexpected input with name ${currentInputName}`)
    return
  }

  const currentEntryIndex = parseInt(currentInputName.split('-')[1])
  let newIndex = currentEntryIndex
  switch (direction) {
    case 'up':
      newIndex -= loggableDays.length
      if (newIndex < 0) {
        newIndex += inputValues.value.length
      }
      break

    case 'down':
      newIndex = (newIndex + loggableDays.length) % inputValues.value.length
      break
  }

  tryFocusOn(`dayinput-${newIndex}`)
}

function tryFocusOn(name: string) {
  const hits = document.getElementsByName(name)
  if (hits.length > 0) {
    hits[0].focus()
  } else {
    console.debug(`Attempted focus on ${name} yielded no elements`)
  }
}

const getInputIndex = (categoryIndex: number, dayIndex: number): number => {
  if (categoryIndex < 0 || categoryIndex > userEntryCategories.length) {
    throw new Error(`Category index value ${categoryIndex} is out of range`)
  }
  if (dayIndex < 0 || dayIndex > loggableDays.length) {
    throw new Error(`Day index value ${dayIndex} is out of range`)
  }

  return categoryIndex * loggableDays.length + dayIndex
}

const onSubmit = async () => {
  if (isSaving.value) {
    return
  }

  isSaving.value = true

  await apiClient
    .updateWeekUserEntries(props.dateRange.year, props.dateRange.weekNumber, {
      monday: getUpdateWeekUserEntriesRequestDay(0, timeTableEntries.value),
      tuesday: getUpdateWeekUserEntriesRequestDay(1, timeTableEntries.value),
      wednesday: getUpdateWeekUserEntriesRequestDay(2, timeTableEntries.value),
      thursday: getUpdateWeekUserEntriesRequestDay(3, timeTableEntries.value),
      friday: getUpdateWeekUserEntriesRequestDay(4, timeTableEntries.value),
      saturday: getUpdateWeekUserEntriesRequestDay(5, timeTableEntries.value),
      sunday: getUpdateWeekUserEntriesRequestDay(6, timeTableEntries.value),
    })
    .match(
      (ok) => {},
      (error) => {
        // TODO handle this gracefully without losing the user input preferably
      },
    )

  isSaving.value = false
  emits('reload')
}

function getUpdateWeekUserEntriesRequestDay(dayIndex: number, entries: TimeTableEntry[]) {
  return <IPutUpdateWeekUserEntriesRequestDay>{
    entries: entries
      .filter((e) => e.dayIndex === 0)
      .map((e) => ({
        entryCategoryId: e.userEntryCategoryId,
        minutes: e.inputMinutes,
      })),
  }
}
</script>

<template>
  <div class="relative transition duration-200" :class="{ 'blur-sm': isSaving || props.disabled }">
    <div
      class="absolute top-0 left-0 z-10 h-full w-full cursor-not-allowed"
      v-if="isSaving || props.disabled"
    ></div>

    <div>
      <div>
        <div class="flex justify-center" style="max-width: 100vw">
          <table class="table-auto">
            <tr>
              <th></th>
              <th v-for="(day, index) in loggableDays" :key="day">
                <span :title="toShortDutchDate(dateRange.dates[index])">{{
                  day.substring(0, 2)
                }}</span>
                <span
                  class="text-blue-500"
                  :class="{ invisible: day !== today }"
                  :title="`Het is vandaag ${day}`"
                  >*</span
                >
              </th>
              <th></th>
            </tr>

            <tr v-for="(category, categoryIndex) in userEntryCategories" :key="category.id">
              <td>
                <span class="pr-1" :class="{ 'text-gray-500': !category.enabled }">{{
                  category.name
                }}</span>
              </td>

              <td v-for="(_, dayIndex) in loggableDays" :key="`${category.id}-${dayIndex}`">
                <KeepiInput
                  :name="`dayinput-${getInputIndex(categoryIndex, dayIndex)}`"
                  :model-value="inputValues[getInputIndex(categoryIndex, dayIndex)]"
                  @update:model-value="onDayInput($event, getInputIndex(categoryIndex, dayIndex))"
                  :readonly="!category.enabled"
                  :tabindex="category.enabled ? 0 : -1"
                  :input-validation="inputValidationMode"
                  class="text-center"
                  :class="{ 'text-gray-500': !category.enabled }"
                  style="width: 65px"
                  @keyup.up="onKey('up')"
                  @keyup.down="onKey('down')"
                />
              </td>

              <td class="text-center text-gray-500">
                <div style="min-width: 65px">
                  <span class="pl-1">
                    {{ categorySummaries[categoryIndex] }}
                  </span>
                </div>
              </td>
            </tr>

            <tr>
              <td></td>

              <td v-for="(_, index) in loggableDays" class="text-center text-gray-500">
                <div class="min-h-6">
                  <span>
                    {{ daySummaries[index] }}
                  </span>
                </div>
              </td>

              <td class="text-center text-gray-500">
                <div class="min-h-6" style="min-width: 65px">
                  <span>
                    {{ grandTotal }}
                  </span>
                </div>
              </td>
            </tr>
          </table>
        </div>

        <div class="grid w-full grid-cols-3 space-x-2 p-3">
          <div></div>

          <div></div>

          <div class="flex justify-end">
            <KeepiButton class="self-end" @click="onSubmit" variant="green"> Opslaan </KeepiButton>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
