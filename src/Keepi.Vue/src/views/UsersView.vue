<script setup lang="ts">
import ApiClient, { type IGetAllUsersResponse } from '@/api-client'
import { handleApiError } from '@/router'
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
import { isAdmin } from '@/user-roles'
import { getUserContext } from '@/user-context'
import KeepiEditUserDialog from '@/components/KeepiEditUserDialog.vue'

const apiClient = new ApiClient()
const userContext = getUserContext()

const users = ref<IGetAllUsersResponse['users']>(await getAllUsers(apiClient))

const reloadUsers = async () => {
  users.value = await getAllUsers(apiClient)
}

async function getAllUsers(apiClient: ApiClient) {
  return await apiClient.getAllUsers().match(
    (result) => {
      return result.users
    },
    (error) => {
      handleApiError(error)
      return []
    },
  )
}

const userToEdit = ref<IGetAllUsersResponse['users'][0] | null>(null)
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
          <TableHead class="hidden sm:block">E-mail</TableHead>
          <TableHead>Rol</TableHead>
          <TableHead></TableHead>
        </TableRow>
      </TableHeader>

      <TableBody>
        <TableRow v-for="user in users" :key="user.id">
          <TableCell>{{ user.name }}</TableCell>

          <TableCell class="hidden sm:block">{{ user.emailAddress }}</TableCell>

          <TableCell>
            {{ isAdmin(user) ? 'Beheerder' : 'Gebruiker' }}
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
