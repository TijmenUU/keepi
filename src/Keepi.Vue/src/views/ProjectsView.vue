<script setup lang="ts">
import ApiClient, { type IGetAllProjectsResponse } from '@/api-client'
import KeepiCreateProjectDialog from '@/components/KeepiCreateProjectDialog.vue'
import KeepiEditProjectDialog from '@/components/KeepiEditProjectDialog.vue'
import KeepiAlertDialog from '@/components/KeepiAlertDialog.vue'
import { handleApiError } from '@/router'
import { ref } from 'vue'
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Label } from '@/components/ui/label'
import { Button } from '@/components/ui/button'

const apiClient = new ApiClient()
const isApplyingChanges = ref(false)
const deleteDialogOpen = ref(false)
const editDialogOpen = ref(false)
const projects = ref<IGetAllProjectsResponse['projects']>(await getAllProjects(apiClient))
const projectToEdit = ref<IGetAllProjectsResponse['projects'][0] | null>(null)

const reloadProjects = async () => {
  projects.value = await getAllProjects(apiClient)
}

async function getAllProjects(apiClient: ApiClient) {
  return await apiClient.getAllProjects().match(
    (result) => {
      return result.projects
    },
    (error) => {
      handleApiError(error)
      return []
    },
  )
}

let confirmDeleteProject = async () => {}

const onDeleteProject = (id: number) => {
  confirmDeleteProject = async () => {
    if (isApplyingChanges.value) {
      return
    }

    isApplyingChanges.value = true
    try {
      await apiClient.deleteProject(id).match(reloadProjects, (error) => {
        handleApiError(error)
      })
    } finally {
      isApplyingChanges.value = false
    }
  }
  deleteDialogOpen.value = true
}

const onEditProject = (id: number) => {
  if (editDialogOpen.value) {
    return
  }

  projectToEdit.value = projects.value.find((p) => p.id === id) ?? null
  if (projectToEdit.value != null) {
    editDialogOpen.value = true
  }
}
</script>

<template>
  <div>
    <Teleport to="#header-bar">
      <KeepiCreateProjectDialog :existing-projects="projects" @project-created="reloadProjects" />
      <KeepiEditProjectDialog
        v-model:open="editDialogOpen"
        :existing-projects="projects"
        :editing-project="projectToEdit"
        @project-updated="reloadProjects"
        v-if="projectToEdit != null" />
    </Teleport>

    <KeepiAlertDialog v-model="deleteDialogOpen" @accept="confirmDeleteProject" />

    <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
      <Card v-for="project in projects" :key="project.id" :project="project">
        <CardHeader>
          <CardTitle>{{ project.name }}</CardTitle>
        </CardHeader>

        <CardContent class="flex h-full flex-col gap-4">
          <Label v-if="project.enabled">Beschikbaar voor tijdsregistratie</Label>

          <Label v-else> Niet beschikbaar voor tijdsregistratie</Label>

          <div>
            <Label>Gebruikers</Label>
            <ul class="list-inside" v-if="project.users.length > 0">
              <li class="list-disc" v-for="user in project.users" :key="user.id">
                {{ user.name }}
              </li>
            </ul>
            <p v-else class="italic">Geen</p>
          </div>

          <div>
            <Label>Facturatieposten</Label>
            <ul class="list-inside" v-if="project.invoiceItems.length > 0">
              <li class="list-disc" v-for="item in project.invoiceItems" :key="item.id">
                {{ item.name }}
              </li>
            </ul>
            <p v-else class="italic">Geen</p>
          </div>
        </CardContent>

        <CardFooter class="flex-col gap-2 sm:grid sm:grid-cols-3">
          <Button
            :disabled="isApplyingChanges"
            class="w-full sm:col-span-2"
            @click="() => onEditProject(project.id)">
            Bewerk
          </Button>

          <Button
            :disabled="isApplyingChanges"
            class="w-full"
            variant="destructive"
            @click="() => onDeleteProject(project.id)">
            Verwijder
          </Button>
        </CardFooter>
      </Card>
    </div>
  </div>
</template>
