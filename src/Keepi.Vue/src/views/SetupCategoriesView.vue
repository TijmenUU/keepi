<script setup lang="ts">
import ApiClient, {
  type IGetUserEntryCategoriesResponse,
  type IPutUpdateUserEntryCategoriesRequest as IPutUpdateUserEntryCategoriesRequest,
} from '@/api-client'
import KeepiButton from '@/components/KeepiButton.vue'
import KeepiCheckbox from '@/components/KeepiCheckbox.vue'
import KeepiInput from '@/components/KeepiInput.vue'
import { createSwapy, type Swapy } from 'swapy'
import { computed, nextTick, onMounted, reactive, ref, useTemplateRef } from 'vue'
import { onBeforeRouteLeave, type RouteLocationNormalizedGeneric, useRouter } from 'vue-router'

type CategoryEntry = {
  id: number
  enabled: boolean
  name: string
  isNew: boolean
  isDeleted: boolean
  // TODO implement this functionality
  // activeFrom: string,
  // activeTo: string,
}

const router = useRouter()
const apiClient = new ApiClient()
let nextUserEntryCategoryId = -1

const isSubmitting = ref(false)
const hasSwapped = ref(false)
const ignoreUnsavedChanges = ref(false)
const pendingNavigationChange = ref<RouteLocationNormalizedGeneric | null>(null)

const userEntryCategories = await apiClient.getAllUserEntryCategories().match(
  (result) => result.categories,
  (error) => {
    if (error === 'unauthorized' || error === 'forbidden') {
      location.pathname = '/signin' // TODO add the current url as redirect url
    } else {
      location.pathname = '/error'
    }

    return <IGetUserEntryCategoriesResponse['categories']>[]
  },
)

const values: CategoryEntry[] = reactive(
  userEntryCategories.map((c) => ({
    id: c.id,
    enabled: c.enabled,
    name: c.name,
    isNew: false,
    isDeleted: false,
  })),
)

const swapyContainer = useTemplateRef('container')
const swapy = ref<Swapy | null>(null)

onMounted(() => {
  if (swapyContainer.value != null) {
    swapy.value = createSwapy(swapyContainer.value)
    swapy.value.onSwapEnd(() => {
      // TODO This could be made smarter by comparing the slot item array from the event against the original user entry categories
      hasSwapped.value = true
    })
  } else {
    console.error('The swapy container element reference is unexpectedly NULL', swapyContainer)
  }
})

const onToggleDelete = (id: number): void => {
  const toDeleteIndex = values.findIndex((v) => v.id === id)
  if (toDeleteIndex != 0) {
    const toDelete = values[toDeleteIndex]
    if (toDelete.isNew) {
      values.splice(toDeleteIndex, 1)
      swapy.value?.update()
    } else {
      toDelete.isDeleted = !toDelete.isDeleted
    }
  }
}

const hasUnsavedChanges = computed<boolean>(() => {
  return (
    hasSwapped.value ||
    values.reduce<boolean>(
      (previous, current) => previous || current.isDeleted || current.isNew,
      false,
    )
  )
})

const hasEmptyNames = computed<boolean>(() => {
  return values.reduce<boolean>(
    (previous, current) => previous || current.name == null || current.name.trim() == '',
    false,
  )
})

const hasDuplicateNames = computed<boolean>(() => {
  for (let i = 0; i < values.length; ++i) {
    for (let j = i + 1; j < values.length; ++j) {
      if (values[i].name === values[j].name) {
        return true
      }
    }
  }

  return false
})

const hasInvalidChanges = computed<boolean>(() => {
  return hasEmptyNames.value || hasDuplicateNames.value
})

const submitButtonTitle = computed<string | null>(() => {
  if (!hasUnsavedChanges.value) {
    return 'Er zijn geen openstaande wijzigingen'
  } else if (hasEmptyNames.value) {
    return 'Er zijn categorieën zonder geldige naam'
  } else if (hasDuplicateNames.value) {
    return 'Er zijn meerdere categorieën met dezelfde naam'
  }

  return null
})

const onAddEntryCategory = () => {
  values.push({
    id: nextUserEntryCategoryId--,
    enabled: true,
    name: '',
    isNew: true,
    isDeleted: false,
  })

  nextTick(() => swapy.value?.update())
}

const onSubmit = async () => {
  if (isSubmitting.value) {
    return
  }

  isSubmitting.value = true

  if (values.length < 1) {
    return
  }
  if (values.some((v1) => values.some((v2) => v1.name == v2.name && v1.id !== v2.id))) {
    return
  }

  const swapyReportedOrder = (swapy.value?.slotItemMap().asArray ?? []).map((v) => parseInt(v.item))

  const updateCandidateValues = values.filter((v) => !v.isDeleted)
  const updateRequest: IPutUpdateUserEntryCategoriesRequest = {
    userEntryCategories: [],
  }
  for (let i = 0; i < updateCandidateValues.length; ++i) {
    const updateCandidateValue = updateCandidateValues[i]
    const updateCandidateValueOrdinal = swapyReportedOrder.findIndex(
      (v) => v === updateCandidateValue.id,
    )

    updateRequest.userEntryCategories.push({
      id: updateCandidateValue.isNew ? undefined : updateCandidateValue.id,
      enabled: updateCandidateValue.enabled,
      name: updateCandidateValue.name,
      ordinal: updateCandidateValueOrdinal,
    })
  }

  const result = await apiClient.updateAllUserEntryCategories(updateRequest)
  if (result.isErr()) {
    // TODO report the error to the user
    isSubmitting.value = false
    return
  }

  // Reload the component
  router.push('/')
}

onBeforeRouteLeave((to, _) => {
  if (isSubmitting.value || ignoreUnsavedChanges.value) {
    return true
  }
  if (hasUnsavedChanges.value) {
    pendingNavigationChange.value = to
    return false
  }
})

const onIgnoreUnsavedChanges = async () => {
  if (pendingNavigationChange.value == null) {
    return
  }

  ignoreUnsavedChanges.value = true
  await router.push(pendingNavigationChange.value.fullPath)
}

const onAbortNavigation = () => {
  pendingNavigationChange.value = null
}

const getRandomModalTitle = () => {
  const titles = ['Woeps!', 'Ojee!', 'Wow there cowboy!', 'Uhh...', 'Zeg makker']
  return titles[Math.floor(Math.random() * titles.length)]
}
</script>

<template>
  <div class="container mx-auto flex max-w-screen-lg flex-col items-center px-2 py-3">
    <div>
      <div
        class="flex flex-col items-center gap-2"
        :class="{ 'blur-sm': isSubmitting }"
        ref="container"
      >
        <div class="grid w-full grid-cols-[1fr_3fr_1fr] gap-2 px-2 font-bold">
          <div>Beschikbaar</div>
          <div>Naam</div>
          <div>Verwijderen</div>
        </div>

        <div
          v-for="(value, index) in values"
          :key="value.id"
          :data-swapy-slot="index"
          class="rounded-m flex w-full flex-col"
        >
          <div
            class="grid w-full cursor-move grid-cols-[1fr_3fr_1fr] gap-2 rounded-md border border-gray-600 p-3 drop-shadow-md"
            :class="{ 'bg-red-900': value.isDeleted }"
            :data-swapy-item="value.id"
          >
            <KeepiCheckbox v-model="value.enabled" />

            <KeepiInput v-model="value.name" />

            <div>
              <button
                @click="onToggleDelete(value.id)"
                :disabled="isSubmitting"
                class="text-red-600 transition-colors duration-200 hover:text-red-400 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke-width="1.5"
                  stroke="currentColor"
                  class="h-6 w-6"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="m14.74 9-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 0 1-2.244 2.077H8.084a2.25 2.25 0 0 1-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 0 0-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 0 1 3.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 0 0-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 0 0-7.5 0"
                  />
                </svg>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="mt-3 flex w-full justify-end gap-3">
      <KeepiButton class="grow py-0" @click="onAddEntryCategory()" :disabled="isSubmitting">
        Toevoegen
      </KeepiButton>

      <KeepiButton
        @click="onSubmit"
        variant="green"
        :disabled="isSubmitting || values.length < 1 || !hasUnsavedChanges || hasInvalidChanges"
        :title="submitButtonTitle"
      >
        Opslaan
      </KeepiButton>
    </div>

    <Transition name="fade" mode="out-in" appear>
      <div
        v-if="pendingNavigationChange"
        class="pd-overlay fixed top-0 left-0 z-10 h-full w-full overflow-x-hidden overflow-y-auto"
      >
        <div class="m-5 sm:mx-auto sm:w-full sm:max-w-lg">
          <div class="flex flex-col rounded-lg bg-gray-700">
            <div
              class="flex items-center justify-between rounded-t border-b border-gray-600 p-4 md:p-5"
            >
              <h3 class="text-xl font-semibold text-white">
                {{ getRandomModalTitle() }}
              </h3>

              <button
                type="button"
                class="ms-auto inline-flex h-8 w-8 items-center justify-center rounded-lg bg-transparent text-sm text-gray-400 hover:bg-gray-600 hover:text-white"
                @click="onAbortNavigation"
              >
                <svg
                  class="h-3 w-3"
                  aria-hidden="true"
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  viewBox="0 0 14 14"
                >
                  <path
                    stroke="currentColor"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="m1 1 6 6m0 0 6 6M7 7l6-6M7 7l-6 6"
                  />
                </svg>
                <span class="sr-only">Close modal</span>
              </button>
            </div>

            <div class="space-y-4 p-4 md:p-5">
              <p class="text-base leading-relaxed text-gray-400">
                Er zijn nog onopgeslagen wijzigingen, weet je het zeker dat je deze wilt negeren?
              </p>
            </div>

            <div
              class="flex items-center justify-end gap-3 rounded-b border-t border-gray-600 p-4 md:p-5"
            >
              <KeepiButton class="text-sm" variant="outline" @click="onAbortNavigation">
                Terug
              </KeepiButton>

              <KeepiButton variant="red" @click="onIgnoreUnsavedChanges">
                Negeer wijzigingen
              </KeepiButton>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>
