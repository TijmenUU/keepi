<script setup lang="ts">
import ApiClient, { type IUpdateAllUserInvoiceItemCustomizationsRequest } from '@/api-client'
import { getSortedInvoiceItems } from '@/invoiceitems'
import { handleApiError } from '@/router'
import { createSwapy, type Swapy } from 'swapy'
import { computed, onMounted, ref, useTemplateRef } from 'vue'
import KeepiAlertDialog from '@/components/KeepiAlertDialog.vue'
import { useRegle } from '@regle/core'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Label } from '@/components/ui/label'
import { useNavigationChangeDialogConfirmation } from '@/dialog'
import { requiredValidator } from '@/regle'
import { FieldDescription, FieldGroup, FieldSet } from '@/components/ui/field'
import { Grip } from 'lucide-vue-next'
import { toast } from 'vue-sonner'

const apiClient = new ApiClient()
const projects = await apiClient.getUserProjects().match(
  (success) => success.projects,
  (error) => {
    handleApiError(error)
    return []
  },
)

const colors = [
  'none',
  '#f97316', // orange
  '#eab308', // yellow
  '#22c55e', // green
  '#3b82f6', // blue
  '#a855f7', // purple
]

const { r$ } = useRegle(
  {
    invoiceItems: getSortedInvoiceItems(projects).map((i) => {
      if (!colors.includes(i.color)) {
        i.color = 'none'
      }

      return i
    }),
  },
  {
    invoiceItems: {
      $each: {
        id: {
          required: requiredValidator,
        },
        color: {
          required: requiredValidator,
        },
      },
    },
  },
)

const isApplyingChanges = ref(false)
const hasSwapped = ref(false)
const hasChanges = computed<boolean>(() => hasSwapped.value || r$.$anyDirty)
const { dialogOpen: unsavedChangesDialogOpen, onAccept: onAcceptUnsavedChanges } =
  useNavigationChangeDialogConfirmation(hasChanges)

const swapyContainer = useTemplateRef('container')
const swapy = ref<Swapy | null>(null)

onMounted(() => {
  if (swapyContainer.value != null) {
    swapy.value = createSwapy(swapyContainer.value)
    swapy.value.onSwapEnd((ev) => {
      if (ev.hasChanged) {
        hasSwapped.value = true
      }
    })
  } else {
    console.error('The swapy container element reference is unexpectedly NULL', swapyContainer)
  }
})

const onPickColor = (color: string, index: number) => {
  if (index < 0 || index >= r$.$value.invoiceItems.length) {
    console.error(`Attempted to set color ${color} to non existing invoice item index ${index}`)
    return
  }

  r$.$value.invoiceItems[index].color = color
  r$.$fields.invoiceItems.$each[index].color.$touch()
}

const onSubmit = async () => {
  if (isApplyingChanges.value) {
    return
  }

  isApplyingChanges.value = true
  try {
    const { valid, data } = await r$.$validate()
    if (!valid) {
      return
    }

    const order = swapy.value?.slotItemMap().asArray
    if (order == null) {
      console.error('Swapy reported order is NULL / undefined')
      return
    }

    if (order.length !== data.invoiceItems?.length) {
      console.error(
        'Form invoice item count does not match the reported invoice item count by Swapy',
      )
      return
    }

    const items: IUpdateAllUserInvoiceItemCustomizationsRequest['invoiceItems'] =
      data.invoiceItems.map((item) => ({
        id: item.id,
        color: item.color === 'none' ? null : item.color,
        ordinal: parseInt(order.find((o) => o.item === item.id.toString())?.slot ?? '0'),
      }))
    const request: IUpdateAllUserInvoiceItemCustomizationsRequest = {
      invoiceItems: items,
    }

    await apiClient.updateAllUserInvoiceItemCustomizations(request).match(
      () => {
        r$.$reset()
        hasSwapped.value = false
        toast.success('De wijzigingen zijn opgeslagen')
      },
      (error) => {
        handleApiError(error)
      },
    )
  } finally {
    isApplyingChanges.value = false
  }
}
</script>

<template>
  <div ref="container">
    <KeepiAlertDialog
      v-model="unsavedChangesDialogOpen"
      @accept="onAcceptUnsavedChanges"
      description="Je gemaakte wijzigingen zullen verloren gaan." />

    <div class="flex flex-col gap-4">
      <div
        v-for="(invoiceItem, index) in r$.$value.invoiceItems"
        :key="invoiceItem.id"
        :data-swapy-slot="index"
        class="rounded-xl bg-gray-200 dark:bg-zinc-950">
        <Card class="w-full" :data-swapy-item="invoiceItem.id">
          <CardHeader>
            <CardTitle>
              {{ invoiceItem.name }}
              <span class="align-super text-xs">({{ invoiceItem.projectName }})</span>
            </CardTitle>
          </CardHeader>

          <CardContent class="flex">
            <FieldGroup>
              <FieldSet class="gap-4">
                <FieldDescription>Invoerscherm accent kleur</FieldDescription>
                <div class="flex flex-wrap gap-4">
                  <Label
                    v-for="color in colors"
                    :key="`${invoiceItem.id}-${color}`"
                    :for="`${invoiceItem.id}-${color}`">
                    <button
                      :id="`${invoiceItem.id}-${color}`"
                      class="h-6 w-6 border"
                      :disabled="invoiceItem.color === color"
                      :class="{
                        'cursor-not-allowed border border-black dark:border-white':
                          invoiceItem.color === color,
                      }"
                      :style="{
                        'background-image': `linear-gradient(45deg,${color},rgba(255,255,255,0) 70%)`,
                      }"
                      :value="color"
                      @click="onPickColor(color, index)"></button>
                  </Label>
                </div>
              </FieldSet>
            </FieldGroup>

            <Grip />
          </CardContent>
        </Card>
      </div>

      <div class="sm:flex sm:justify-end">
        <Button class="w-full sm:w-auto" :disabled="r$.$invalid || !hasChanges" @click="onSubmit">
          Opslaan
        </Button>
      </div>
    </div>
  </div>
</template>

<style lang="css" scoped></style>
