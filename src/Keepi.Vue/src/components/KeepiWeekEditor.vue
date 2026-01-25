<script setup lang="ts">
import type {
  IGetUserProjectsResponse,
  IGetWeekUserEntriesResponse,
  IGetWeekUserEntriesResponseDay,
  IPutUpdateWeekUserEntriesRequestDay,
} from '@/api-client'
import { getWeekNumber, type DateRange } from '@/date'
import { toHoursMinutesNotation, toShortDutchDate, tryParseTimeNotation } from '@/format'
import { loggableDays, type InvoiceItem, type TimeTableEntry } from '@/types'
import { computed, onMounted, ref } from 'vue'
import { useRegle } from '@regle/core'
import { withMessage } from '@regle/rules'
import KeepiWeekEditorDayLabel from '@/components/KeepiWeekEditorDayLabel.vue'
import KeepiValidatedInput from '@/components/KeepiValidatedInput.vue'
import Button from '@/components/ui/button/Button.vue'
import { getSortedInvoiceItems } from '@/invoiceitems'
import KeepiAlertDialog from '@/components/KeepiAlertDialog.vue'
import { useNavigationChangeDialogConfirmation } from '@/dialog'
import { StepForward, StepBack } from 'lucide-vue-next'
import ApiClient from '@/api-client'
import { handleApiError } from '@/error'

const props = defineProps<{
  projects: IGetUserProjectsResponse['projects']
  dateRange: DateRange
  weekEntries: IGetWeekUserEntriesResponse
  disabled: boolean
}>()

const emits = defineEmits<{
  (e: 'reload'): void
  (e: 'nextWeek'): void
  (e: 'previousWeek'): void
  (e: 'currentWeek'): void
}>()

onMounted(() => {
  const firstInput = document.getElementById('0')
  if (firstInput != null) {
    firstInput.focus()
  }
})

const apiClient = new ApiClient()
const currentWeek = getWeekNumber(new Date())
const activeInvoiceItems = getSortedInvoiceItems(props.projects.filter((p) => p.enabled)).map(
  (i) => {
    if (i.color !== 'none') {
      const red = parseInt(i.color.substring(1, 3), 16)
      const green = parseInt(i.color.substring(3, 5), 16)
      const blue = parseInt(i.color.substring(5, 7), 16)

      i.color = `rgba(${red},${green},${blue},0.33)`
    }

    return i
  },
)

const { r$ } = useRegle(
  {
    days: mapWeekEntriesToFlatHoursPerDayPerInvoiceItemArray(activeInvoiceItems, props.weekEntries),
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

const isSaving = ref<boolean>(false)
const disableNavigation = computed<boolean>(() => {
  return isSaving.value || props.disabled
})
const disableSave = computed<boolean>(() => {
  return isSaving.value || props.disabled || r$.$invalid || !r$.$anyDirty
})

const dateRangeDescription = computed<string>(
  () =>
    `${toShortDutchDate(props.dateRange.dates[0])} t/m ${toShortDutchDate(
      props.dateRange.dates[loggableDays.length - 1],
    )}`,
)

const { dialogOpen: unsavedChangesDialogOpen, onAccept: onAcceptUnsavedChanges } =
  useNavigationChangeDialogConfirmation(() => r$.$anyDirty)

const invoiceItemTotals = computed<Record<number, number>>(() => {
  const result: Record<string, number> = {}
  activeInvoiceItems.forEach((invoiceItem, index) => {
    const start = 0 + index * loggableDays.length
    const end = start + loggableDays.length
    result[invoiceItem.id] = r$.$value.days
      .slice(start, end)
      .map((v) => tryParseTimeNotation(v.minutes) ?? 0)
      .reduce((previous, current) => previous + current)
  })

  return result
})

const disabledInvoiceItemRows = getDisabledInvoiceItemRows(
  getSortedInvoiceItems(props.projects.filter((p) => !p.enabled)),
  props.weekEntries,
).filter((i) => i.totalMinutes > 0)

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
    monday:
      totalsPerDay[0] +
      disabledInvoiceItemRows.reduce((previous, current) => previous + current.dayMinutes[0], 0),
    tuesday:
      totalsPerDay[1] +
      disabledInvoiceItemRows.reduce((previous, current) => previous + current.dayMinutes[1], 0),
    wednesday:
      totalsPerDay[2] +
      disabledInvoiceItemRows.reduce((previous, current) => previous + current.dayMinutes[2], 0),
    thursday:
      totalsPerDay[3] +
      disabledInvoiceItemRows.reduce((previous, current) => previous + current.dayMinutes[3], 0),
    friday:
      totalsPerDay[4] +
      disabledInvoiceItemRows.reduce((previous, current) => previous + current.dayMinutes[4], 0),
    saturday:
      totalsPerDay[5] +
      disabledInvoiceItemRows.reduce((previous, current) => previous + current.dayMinutes[5], 0),
    sunday:
      totalsPerDay[6] +
      disabledInvoiceItemRows.reduce((previous, current) => previous + current.dayMinutes[6], 0),
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

const onSubmit = async () => {
  if (isSaving.value) {
    return
  }

  isSaving.value = true
  try {
    const { valid, data } = await r$.$validate()
    if (!valid) {
      return
    }

    const result: TimeTableEntry[] = []
    // For some reason days is optional here despite the regle form not being
    // defined as such
    const minutes = data.days?.map((d) => tryParseTimeNotation(d.minutes ?? '') ?? 0) ?? []

    activeInvoiceItems.forEach((_, invoiceItemIndex) =>
      loggableDays.forEach((_, dayIndex) => {
        const formValueIndex = dayIndex + invoiceItemIndex * loggableDays.length
        result.push({
          invoiceItemId: activeInvoiceItems[invoiceItemIndex].id,
          dayIndex: dayIndex,
          minutes: minutes[formValueIndex],
        })
      }),
    )

    await apiClient
      .updateWeekUserEntries(props.dateRange.year, props.dateRange.weekNumber, {
        monday: getUpdateWeekUserEntriesRequestDay(0, result),
        tuesday: getUpdateWeekUserEntriesRequestDay(1, result),
        wednesday: getUpdateWeekUserEntriesRequestDay(2, result),
        thursday: getUpdateWeekUserEntriesRequestDay(3, result),
        friday: getUpdateWeekUserEntriesRequestDay(4, result),
        saturday: getUpdateWeekUserEntriesRequestDay(5, result),
        sunday: getUpdateWeekUserEntriesRequestDay(6, result),
      })
      .match(
        () => emits('reload'),
        (error) => {
          handleApiError(error)
        },
      )
  } finally {
    isSaving.value = false
  }
}

function getUpdateWeekUserEntriesRequestDay(
  dayIndex: number,
  entries: TimeTableEntry[],
): IPutUpdateWeekUserEntriesRequestDay {
  return {
    entries: entries
      .filter((e) => e.dayIndex === dayIndex && e.minutes != 0)
      .map((e) => ({
        invoiceItemId: e.invoiceItemId,
        minutes: e.minutes,
      })),
  }
}

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

function mapWeekEntriesToFlatHoursPerDayPerInvoiceItemArray(
  invoiceItems: InvoiceItem[],
  weekEntries: IGetWeekUserEntriesResponse,
) {
  const entries = [
    ...createDayIndexEntries(0, weekEntries.monday),
    ...createDayIndexEntries(1, weekEntries.tuesday),
    ...createDayIndexEntries(2, weekEntries.wednesday),
    ...createDayIndexEntries(3, weekEntries.thursday),
    ...createDayIndexEntries(4, weekEntries.friday),
    ...createDayIndexEntries(5, weekEntries.saturday),
    ...createDayIndexEntries(6, weekEntries.sunday),
  ]

  const result: { minutes: string }[] = []
  invoiceItems.forEach((invoiceItem) =>
    loggableDays.forEach((_, dayIndex) => {
      const minutes = entries
        .filter((entry) => entry.invoiceItemId === invoiceItem.id && entry.dayIndex === dayIndex)
        .reduce<number>((previous, current) => previous + current.minutes, 0)
      result.push({ minutes: toHoursMinutesNotation(minutes) })
    }),
  )

  return result
}

function createDayIndexEntries(dayIndex: number, dayEntries: IGetWeekUserEntriesResponseDay) {
  return dayEntries.entries.map((entry) => ({
    invoiceItemId: entry.invoiceItemId,
    remark: entry.remark,
    minutes: entry.minutes,
    dayIndex,
  }))
}

function getDisabledInvoiceItemRows(
  invoiceItems: InvoiceItem[],
  weekEntries: IGetWeekUserEntriesResponse,
): { item: InvoiceItem; dayMinutes: number[]; totalMinutes: number }[] {
  return invoiceItems.map((i) => {
    const dayMinutes = [
      sumInvoiceItemMinutes(weekEntries.monday, i.id),
      sumInvoiceItemMinutes(weekEntries.tuesday, i.id),
      sumInvoiceItemMinutes(weekEntries.wednesday, i.id),
      sumInvoiceItemMinutes(weekEntries.thursday, i.id),
      sumInvoiceItemMinutes(weekEntries.friday, i.id),
      sumInvoiceItemMinutes(weekEntries.saturday, i.id),
      sumInvoiceItemMinutes(weekEntries.sunday, i.id),
    ]
    return {
      item: i,
      dayMinutes,
      totalMinutes: dayMinutes.reduce((previous, current) => previous + current, 0),
    }
  })
}

function sumInvoiceItemMinutes(day: IGetWeekUserEntriesResponseDay, invoiceItemId: number) {
  return day.entries
    .filter((e) => e.invoiceItemId === invoiceItemId)
    .reduce((previous, current) => previous + current.minutes, 0)
}
</script>

<template>
  <div class="flex-col items-center space-y-4">
    <Teleport to="#header-bar">
      <Button variant="outline" @click="() => emits('previousWeek')" :disabled="disableNavigation">
        <StepBack />
        <span class="max-sm:sr-only"> Vorige week </span>
      </Button>

      <Button
        variant="outline"
        :disabled="currentWeek === dateRange.weekNumber || disableNavigation"
        @click="() => emits('currentWeek')">
        Toon vandaag
      </Button>

      <Button variant="outline" @click="() => emits('nextWeek')" :disabled="disableNavigation">
        <span class="max-sm:sr-only"> Volgende week </span>
        <StepForward />
      </Button>
    </Teleport>

    <div class="text-center">
      <h2 class="text-2xl">Week {{ dateRange.weekNumber }}</h2>
      <p class="">{{ dateRangeDescription }}</p>
    </div>

    <div class="flex flex-col space-y-4">
      <KeepiAlertDialog
        v-model="unsavedChangesDialogOpen"
        @accept="onAcceptUnsavedChanges"
        description="Je gemaakte wijzigingen zullen verloren gaan." />

      <div class="flex flex-col gap-2 overflow-x-scroll overflow-y-hidden">
        <div class="grid grid-cols-[repeat(10,minmax(4rem,1fr))] gap-2 font-bold">
          <span class="col-span-2 text-end">Facturatie post</span>
          <KeepiWeekEditorDayLabel
            v-for="date in dateRange.dates"
            :key="date.getTime()"
            :date="date" />
          <span>Totalen</span>
        </div>

        <div
          v-for="(invoiceItem, index) in activeInvoiceItems"
          class="grid grid-cols-[repeat(10,minmax(4rem,1fr))] gap-2 p-0.5"
          :key="invoiceItem.id">
          <div
            class="col-span-2 flex items-center justify-end rounded-md px-1"
            :style="{
              'background-image': `linear-gradient(45deg,${invoiceItem.color},rgba(255,255,255,0) 70%)`,
            }">
            <span class="truncate overflow-hidden">
              {{ invoiceItem.name }}
            </span>
          </div>

          <KeepiValidatedInput
            v-model="r$.$value.days[0 + index * loggableDays.length].minutes"
            :field="r$.$fields.days.$each[0 + index * loggableDays.length].$fields.minutes"
            :id="`${0 + index * loggableDays.length}`"
            class="text-center"
            :style="{
              'background-image': `linear-gradient(45deg,${invoiceItem.color},rgba(255,255,255,0) 40%)`,
            }"
            @keydown="onKeyDown" />
          <KeepiValidatedInput
            v-model="r$.$value.days[1 + index * loggableDays.length].minutes"
            :field="r$.$fields.days.$each[1 + index * loggableDays.length].$fields.minutes"
            :id="`${1 + index * loggableDays.length}`"
            class="text-center"
            :style="{
              'background-image': `linear-gradient(45deg,${invoiceItem.color},rgba(255,255,255,0) 40%)`,
            }"
            @keydown="onKeyDown" />
          <KeepiValidatedInput
            v-model="r$.$value.days[2 + index * loggableDays.length].minutes"
            :field="r$.$fields.days.$each[2 + index * loggableDays.length].$fields.minutes"
            :id="`${2 + index * loggableDays.length}`"
            class="text-center"
            :style="{
              'background-image': `linear-gradient(45deg,${invoiceItem.color},rgba(255,255,255,0) 40%)`,
            }"
            @keydown="onKeyDown" />
          <KeepiValidatedInput
            v-model="r$.$value.days[3 + index * loggableDays.length].minutes"
            :field="r$.$fields.days.$each[3 + index * loggableDays.length].$fields.minutes"
            :id="`${3 + index * loggableDays.length}`"
            class="text-center"
            :style="{
              'background-image': `linear-gradient(45deg,${invoiceItem.color},rgba(255,255,255,0) 40%)`,
            }"
            @keydown="onKeyDown" />
          <KeepiValidatedInput
            v-model="r$.$value.days[4 + index * loggableDays.length].minutes"
            :field="r$.$fields.days.$each[4 + index * loggableDays.length].$fields.minutes"
            :id="`${4 + index * loggableDays.length}`"
            class="text-center"
            :style="{
              'background-image': `linear-gradient(45deg,${invoiceItem.color},rgba(255,255,255,0) 40%)`,
            }"
            @keydown="onKeyDown" />
          <KeepiValidatedInput
            v-model="r$.$value.days[5 + index * loggableDays.length].minutes"
            :field="r$.$fields.days.$each[5 + index * loggableDays.length].$fields.minutes"
            :id="`${5 + index * loggableDays.length}`"
            class="text-center"
            :style="{
              'background-image': `linear-gradient(45deg,${invoiceItem.color},rgba(255,255,255,0) 40%)`,
            }"
            @keydown="onKeyDown" />
          <KeepiValidatedInput
            v-model="r$.$value.days[6 + index * loggableDays.length].minutes"
            :field="r$.$fields.days.$each[6 + index * loggableDays.length].$fields.minutes"
            :id="`${6 + index * loggableDays.length}`"
            class="text-center"
            :style="{
              'background-image': `linear-gradient(45deg,${invoiceItem.color},rgba(255,255,255,0) 40%)`,
            }"
            @keydown="onKeyDown" />
          <div
            class="flex items-center justify-center rounded-md"
            :style="{
              'background-image': `linear-gradient(45deg,${invoiceItem.color},rgba(255,255,255,0) 40%)`,
            }">
            <span>{{ toHoursMinutesNotation(invoiceItemTotals[invoiceItem.id]) }}</span>
          </div>
        </div>

        <div
          class="grid grid-cols-[repeat(10,minmax(4rem,1fr))] gap-2 text-center text-gray-400 dark:text-gray-600"
          v-for="disabledRow in disabledInvoiceItemRows"
          :key="disabledRow.item.id">
          <span class="col-span-2 truncate overflow-hidden text-left">
            {{ disabledRow.item.name }}
          </span>
          <span>{{ toHoursMinutesNotation(disabledRow.dayMinutes[0]) }}</span>
          <span>{{ toHoursMinutesNotation(disabledRow.dayMinutes[1]) }}</span>
          <span>{{ toHoursMinutesNotation(disabledRow.dayMinutes[2]) }}</span>
          <span>{{ toHoursMinutesNotation(disabledRow.dayMinutes[3]) }}</span>
          <span>{{ toHoursMinutesNotation(disabledRow.dayMinutes[4]) }}</span>
          <span>{{ toHoursMinutesNotation(disabledRow.dayMinutes[5]) }}</span>
          <span>{{ toHoursMinutesNotation(disabledRow.dayMinutes[6]) }}</span>
          <span>
            {{ toHoursMinutesNotation(disabledRow.totalMinutes) }}
          </span>
        </div>

        <div class="grid min-h-lh grid-cols-[repeat(10,minmax(4rem,1fr))] gap-2 text-center">
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

      <div class="sm:flex sm:justify-end">
        <Button class="w-full sm:w-auto" :disabled="disableSave" @click="onSubmit">
          Opslaan
        </Button>
      </div>
    </div>
  </div>
</template>
