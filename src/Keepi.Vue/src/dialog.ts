import { router } from '@/router'
import { ref, toValue, type MaybeRefOrGetter } from 'vue'
import {
  onBeforeRouteLeave,
  onBeforeRouteUpdate,
  type NavigationGuard,
  type RouteLocationNormalizedGeneric,
} from 'vue-router'

export function useNavigationChangeDialogConfirmation(showDialog: MaybeRefOrGetter<boolean>) {
  const ignoreUnsavedChanges = ref<boolean>(false)
  const dialogOpen = ref<boolean>(false)
  const pendingNavigationChange = ref<RouteLocationNormalizedGeneric | null>(null)

  const onBeforeHandler: NavigationGuard = (to) => {
    if (ignoreUnsavedChanges.value) {
      return true
    }

    if (toValue(showDialog)) {
      dialogOpen.value = true
      pendingNavigationChange.value = to
      return false
    }
  }

  onBeforeRouteUpdate(onBeforeHandler) // Different URL, same component
  onBeforeRouteLeave(onBeforeHandler) // Different URL, different component

  const onAccept = async () => {
    if (pendingNavigationChange.value == null) {
      return
    }

    ignoreUnsavedChanges.value = true
    await router.push(pendingNavigationChange.value.fullPath)
  }

  return {
    dialogOpen,
    onAccept,
  }
}
