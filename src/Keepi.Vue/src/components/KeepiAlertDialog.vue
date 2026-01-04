<script setup lang="ts">
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'

const model = defineModel<boolean>({ required: true })

const props = withDefaults(
  defineProps<{
    title?: string
    description?: string
    acceptLabel?: string
  }>(),
  {
    title: 'Weet je het zeker?',
    description: 'Deze actie kan niet ongedaan worden gemaakt.',
    acceptLabel: 'Bevestigen',
  },
)

const emits = defineEmits<{
  (e: 'accept'): void
  (e: 'cancel'): void
}>()
</script>

<template>
  <AlertDialog :open="model" @update:open="(value) => (model = value)">
    <AlertDialogContent>
      <AlertDialogHeader>
        <AlertDialogTitle>{{ props.title }}</AlertDialogTitle>

        <AlertDialogDescription>
          {{ props.description }}
        </AlertDialogDescription>
      </AlertDialogHeader>

      <AlertDialogFooter>
        <AlertDialogCancel @click="emits('cancel')">Annuleren</AlertDialogCancel>

        <AlertDialogAction @click="emits('accept')">{{ props.acceptLabel }}</AlertDialogAction>
      </AlertDialogFooter>
    </AlertDialogContent>
  </AlertDialog>
</template>
