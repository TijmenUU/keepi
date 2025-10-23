<script setup lang="ts">
import type {
  IGetUserEntryCategoriesResponse,
  IGetWeekUserEntriesResponse,
  IGetWeekUserEntriesResponseDay,
} from '@/api-client'
import ApiClient from '@/api-client'
import KeepiButton from '@/components/KeepiButton.vue'
import type { DateRange } from '@/date'
import { toHoursMinutesNotation, tryParseTimeNotation } from '@/format'
import { loggableDays, type TimeTableEntry } from '@/types'
import { computed, onMounted } from 'vue'
import { useCustomSubmit } from '@/regle'
import { useRegle } from '@regle/core'
import { withMessage } from '@regle/rules'
import WeekEditorDayLabel from '@/components/WeekEditorDayLabel.vue'
import WeekEditorInput from '@/components/WeekEditorInput.vue'

const apiClient = new ApiClient()

const props = defineProps<{
  dateRange: DateRange
  weekEntries: IGetWeekUserEntriesResponse
}>()

const emits = defineEmits<{
  (e: 'submit', value: TimeTableEntry[]): void
}>()

onMounted(() => {
  const firstInput = document.getElementById('0')
  if (firstInput != null) {
    firstInput.focus()
  }
})

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

const mapWeekEntriesToFlatHoursPerDayPerCategoryArray = () => {
  const entries = [
    ...createDayIndexEntries(0, props.weekEntries.monday),
    ...createDayIndexEntries(1, props.weekEntries.tuesday),
    ...createDayIndexEntries(2, props.weekEntries.wednesday),
    ...createDayIndexEntries(3, props.weekEntries.thursday),
    ...createDayIndexEntries(4, props.weekEntries.friday),
    ...createDayIndexEntries(5, props.weekEntries.saturday),
    ...createDayIndexEntries(6, props.weekEntries.sunday),
  ]

  const result: { minutes: string }[] = []
  userEntryCategories.forEach((category) =>
    loggableDays.forEach((_, dayIndex) => {
      const minutes = entries
        .filter((entry) => entry.entryCategoryId === category.id && entry.dayIndex === dayIndex)
        .reduce<number>((previous, current) => previous + current.minutes, 0)
      result.push({ minutes: toHoursMinutesNotation(minutes) })
    }),
  )

  return result
}

function createDayIndexEntries(dayIndex: number, dayEntries: IGetWeekUserEntriesResponseDay) {
  return dayEntries.entries.map((entry) => ({
    entryCategoryId: entry.entryCategoryId,
    remark: entry.remark,
    minutes: entry.minutes,
    dayIndex,
  }))
}

const { r$ } = useRegle(
  {
    days: mapWeekEntriesToFlatHoursPerDayPerCategoryArray(),
  },
  {
    days: {
      $each: {
        minutes: {
          isValidNotation: withMessage((value) => {
            return (
              value == null ||
              typeof value !== 'string' ||
              value === '' ||
              tryParseTimeNotation(value) != null
            )
          }, 'Geen geldige tijd'),
        },
      },
    },
  },
)

const categoryTotals = computed<Record<number, number>>(() => {
  const result: Record<string, number> = {}
  userEntryCategories.forEach((category, index) => {
    const start = 0 + index * loggableDays.length
    const end = start + loggableDays.length
    result[category.id] = r$.$value.days
      .slice(start, end)
      .map((v) => tryParseTimeNotation(v.minutes) ?? 0)
      .reduce((previous, current) => previous + current)
  })

  return result
})

const dayTotals = computed<{
  monday: number
  tuesday: number
  wednesday: number
  thursday: number
  friday: number
  saturday: number
  sunday: number
}>(() => {
  const totalsPerDay = loggableDays.map(() => 0)
  r$.$value.days.forEach((value, index) => {
    const minutes = tryParseTimeNotation(value.minutes)
    if (minutes != null) {
      totalsPerDay[index % loggableDays.length] += minutes
    }
  })
  return {
    monday: totalsPerDay[0],
    tuesday: totalsPerDay[1],
    wednesday: totalsPerDay[2],
    thursday: totalsPerDay[3],
    friday: totalsPerDay[4],
    saturday: totalsPerDay[5],
    sunday: totalsPerDay[6],
  }
})

const grandTotal = computed<number>(() => {
  return (
    dayTotals.value.monday +
    dayTotals.value.tuesday +
    dayTotals.value.wednesday +
    dayTotals.value.thursday +
    dayTotals.value.friday +
    dayTotals.value.saturday +
    dayTotals.value.sunday
  )
})

const { onSubmit, forceShowError } = useCustomSubmit({
  submitCallback: () => {
    const result: TimeTableEntry[] = []

    userEntryCategories.forEach((_, categoryIndex) =>
      loggableDays.forEach((_, dayIndex) => {
        const formValueIndex = dayIndex + categoryIndex * loggableDays.length
        result.push({
          userEntryCategoryId: userEntryCategories[categoryIndex].id,
          dayIndex: dayIndex,
          minutes: tryParseTimeNotation(r$.$value.days[formValueIndex].minutes) ?? 0,
        })
      }),
    )

    emits('submit', result)
  },
})

const onKeyDown = (event: KeyboardEvent) => {
  if (event.target == null || !(event.target instanceof HTMLInputElement)) {
    console.error('Unexpected element as target', event.target)
    return
  }

  const inputId = parseInt(event.target.id)
  if (Number.isNaN(inputId) || inputId < 0) {
    console.error('Input element is missing a numeric ID attribute', event.target)
    return
  }

  let focusOnIndex = inputId
  if (event.code === 'ArrowDown') {
    focusOnIndex = (inputId + loggableDays.length) % r$.$value.days.length
  } else if (event.code === 'ArrowUp') {
    focusOnIndex = inputId - loggableDays.length
    if (focusOnIndex < 0) {
      focusOnIndex += r$.$value.days.length
    }
  } else {
    return
  }

  const candidateElement = document.getElementById(focusOnIndex.toString())
  if (candidateElement == null) {
    console.error(`Cannot focus element with ID ${focusOnIndex} because it does not exist`)
    return
  }

  candidateElement.focus()
}
</script>

<template>
  <div class="flex flex-col space-y-4">
    <div class="flex flex-col gap-2 overflow-x-scroll overflow-y-hidden">
      <div class="grid grid-cols-[repeat(10,minmax(4rem,1fr))] gap-2 font-bold">
        <span class="col-span-2">Categorie</span>
        <WeekEditorDayLabel v-for="date in dateRange.dates" :key="date.getTime()" :date="date" />
        <span>Totalen</span>
      </div>

      <div
        class="grid grid-cols-[repeat(10,minmax(4rem,1fr))] gap-2"
        v-for="(category, index) in userEntryCategories"
        :key="category.id"
      >
        <span class="col-span-2 truncate overflow-hidden text-gray-500">
          {{ category.name }}
        </span>
        <WeekEditorInput
          v-model="r$.$value.days[0 + index * loggableDays.length].minutes"
          :field="r$.$fields.days.$each[0 + index * loggableDays.length].$fields.minutes"
          :force-show-error="forceShowError"
          :id="`${0 + index * loggableDays.length}`"
          @keydown="onKeyDown"
        />
        <WeekEditorInput
          v-model="r$.$value.days[1 + index * loggableDays.length].minutes"
          :field="r$.$fields.days.$each[1 + index * loggableDays.length].$fields.minutes"
          :force-show-error="forceShowError"
          :id="`${1 + index * loggableDays.length}`"
          @keydown="onKeyDown"
        />
        <WeekEditorInput
          v-model="r$.$value.days[2 + index * loggableDays.length].minutes"
          :field="r$.$fields.days.$each[2 + index * loggableDays.length].$fields.minutes"
          :force-show-error="forceShowError"
          :id="`${2 + index * loggableDays.length}`"
          @keydown="onKeyDown"
        />
        <WeekEditorInput
          v-model="r$.$value.days[3 + index * loggableDays.length].minutes"
          :field="r$.$fields.days.$each[3 + index * loggableDays.length].$fields.minutes"
          :force-show-error="forceShowError"
          :id="`${3 + index * loggableDays.length}`"
          @keydown="onKeyDown"
        />
        <WeekEditorInput
          v-model="r$.$value.days[4 + index * loggableDays.length].minutes"
          :field="r$.$fields.days.$each[4 + index * loggableDays.length].$fields.minutes"
          :force-show-error="forceShowError"
          :id="`${4 + index * loggableDays.length}`"
          @keydown="onKeyDown"
        />
        <WeekEditorInput
          v-model="r$.$value.days[5 + index * loggableDays.length].minutes"
          :field="r$.$fields.days.$each[5 + index * loggableDays.length].$fields.minutes"
          :force-show-error="forceShowError"
          :id="`${5 + index * loggableDays.length}`"
          @keydown="onKeyDown"
        />
        <WeekEditorInput
          v-model="r$.$value.days[6 + index * loggableDays.length].minutes"
          :field="r$.$fields.days.$each[6 + index * loggableDays.length].$fields.minutes"
          :force-show-error="forceShowError"
          :id="`${6 + index * loggableDays.length}`"
          @keydown="onKeyDown"
        />
        <span class="text-center text-gray-500">
          {{ toHoursMinutesNotation(categoryTotals[category.id]) }}
        </span>
      </div>

      <div
        class="grid min-h-6 grid-cols-[repeat(10,minmax(4rem,1fr))] gap-2 text-center text-gray-500"
      >
        <span class="col-span-2"></span>
        <span>{{ toHoursMinutesNotation(dayTotals.monday) }}</span>
        <span>{{ toHoursMinutesNotation(dayTotals.tuesday) }}</span>
        <span>{{ toHoursMinutesNotation(dayTotals.wednesday) }}</span>
        <span>{{ toHoursMinutesNotation(dayTotals.thursday) }}</span>
        <span>{{ toHoursMinutesNotation(dayTotals.friday) }}</span>
        <span>{{ toHoursMinutesNotation(dayTotals.saturday) }}</span>
        <span>{{ toHoursMinutesNotation(dayTotals.sunday) }}</span>
        <span>
          {{ toHoursMinutesNotation(grandTotal) }}
        </span>
      </div>
    </div>

    <div class="text-right">
      <KeepiButton :disabled="r$.$invalid" variant="green" @click="onSubmit">Opslaan</KeepiButton>
    </div>
  </div>
</template>
