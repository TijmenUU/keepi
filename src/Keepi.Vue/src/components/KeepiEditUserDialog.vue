<script setup lang="ts">
import type { IGetAllUsersResponse, IUpdateUserPermissionsRequest } from '@/api-client'
import ApiClient from '@/api-client'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { getUserRole } from '@/user-roles'
import { handleApiError } from '@/router'
import type { UserRole } from '@/types'
import { nextTick, ref } from 'vue'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import Label from '@/components/ui/label/Label.vue'

const props = defineProps<{
  editingUser: IGetAllUsersResponse['users'][0]
}>()

const open = defineModel<boolean>('open', { required: true })

const emits = defineEmits<{
  (e: 'user-updated'): void
}>()

const apiClient = new ApiClient()

const disableUserInteraction = ref(false)
const selectedRole = ref(getUserRole(props.editingUser))
const roleOptions = [
  { value: 'admin', label: 'Beheerder' },
  { value: 'user', label: 'Gebruiker' },
]

const onOpen = (ev: Event) => {
  ev.preventDefault()

  selectedRole.value = getUserRole(props.editingUser)

  nextTick(() => {
    const element = document.getElementById('user-role')
    if (element != null) {
      element.focus()
    } else {
      console.error('Failed to focus on first input field')
    }
  })
}

const onSubmit = async () => {
  if (disableUserInteraction.value) {
    return
  }

  disableUserInteraction.value = true

  try {
    await apiClient
      .updateUserPermissions(props.editingUser.id, getUserPermissionsForRole(selectedRole.value))
      .match(
        () => {
          emits('user-updated')
          open.value = false
        },
        (error) => {
          handleApiError(error)
        },
      )
  } finally {
    disableUserInteraction.value = false
  }
}

function getUserPermissionsForRole(role: UserRole): IUpdateUserPermissionsRequest {
  if (role === 'admin') {
    return {
      entriesPermission: 'readAndModify',
      exportsPermission: 'readAndModify',
      projectsPermission: 'readAndModify',
      usersPermission: 'readAndModify',
    }
  }
  return {
    entriesPermission: 'readAndModify',
    exportsPermission: 'none',
    projectsPermission: 'none',
    usersPermission: 'none',
  }
}
</script>

<template>
  <Dialog v-model:open="open">
    <div>
      <DialogContent
        class="max-h-screen overflow-y-scroll"
        @open-auto-focus="onOpen"
        tabindex="null">
        <DialogHeader>
          <DialogTitle>Gebruikersrol bewerken</DialogTitle>

          <DialogDescription>
            Normale gebruikers hebben alleen toegang tot tijdsregistratie. Beheerders hebben toegang
            tot alle delen van de applicatie: tijdsregistratie, exports, projectenbeheer en
            gebruikersbeheer.
          </DialogDescription>
        </DialogHeader>

        <Label>
          Rol
          <Select v-model="selectedRole">
            <SelectTrigger id="user-role">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem v-for="option in roleOptions" :value="option.value" :key="option.value">
                {{ option.label }}
              </SelectItem>
            </SelectContent>
          </Select>
        </Label>

        <DialogFooter>
          <DialogClose as-child>
            <Button variant="outline">Annuleren</Button>
          </DialogClose>

          <Button @click="onSubmit">Opslaan</Button>
        </DialogFooter>
      </DialogContent>
    </div>
  </Dialog>
</template>
