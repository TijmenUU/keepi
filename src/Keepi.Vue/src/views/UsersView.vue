<script setup lang="ts">
import { useApiClient, type IApiClient } from '@/api-client'
import { handleApiError } from '@/error'
import { ref } from 'vue'
import { Button } from '@/components/ui/button'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { getUserRole, getUserRoleLabel } from '@/user-roles'
import { getUserContext } from '@/user-context'
import KeepiEditUserDialog from '@/components/KeepiEditUserDialog.vue'
import type { KeepiUser } from '@/types'

const apiClient = useApiClient()
const userContext = getUserContext()

const users = ref<KeepiUser[]>(await getAllUsers(apiClient))

const reloadUsers = async () => {
  users.value = await getAllUsers(apiClient)
}

async function getAllUsers(apiClient: IApiClient): Promise<KeepiUser[]> {
  return await apiClient.getAllUsers().match(
    (result) => {
      return result.users.map((u) => ({
        id: u.id,
        name: u.name,
        emailAddress: u.emailAddress,
        role: getUserRole(u),
      }))
    },
    (error) => {
      handleApiError(error)
      return []
    },
  )
}

const userToEdit = ref<KeepiUser | null>(null)
const editDialogOpen = ref(false)

const onEditUser = (id: number) => {
  if (editDialogOpen.value) {
    return
  }

  userToEdit.value = users.value.find((u) => u.id === id) ?? null
  if (userToEdit.value != null) {
    editDialogOpen.value = true
  }
}
</script>

<template>
  <div class="flex flex-col space-y-4">
    <KeepiEditUserDialog
      v-if="userToEdit != null"
      v-model:open="editDialogOpen"
      :editing-user="userToEdit"
      @user-updated="reloadUsers" />

    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Naam</TableHead>
          <TableHead class="hidden sm:table-cell">E-mail</TableHead>
          <TableHead>Rol</TableHead>
          <TableHead></TableHead>
        </TableRow>
      </TableHeader>

      <TableBody>
        <TableRow v-for="user in users" :key="user.id">
          <TableCell>{{ user.name }}</TableCell>

          <TableCell class="hidden sm:table-cell">{{ user.emailAddress }}</TableCell>

          <TableCell>
            {{ getUserRoleLabel(user.role) }}
          </TableCell>

          <TableCell>
            <Button v-if="userContext.id !== user.id" @click="() => onEditUser(user.id)">
              Bewerk
            </Button>
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  </div>
</template>
