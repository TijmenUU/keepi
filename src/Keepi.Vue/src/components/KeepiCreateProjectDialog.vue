<script setup lang="ts">
import type { IGetAllProjectsResponse, IGetAllUsersResponse } from '@/api-client'
import ApiClient from '@/api-client'
import KeepiValidatedInput from '@/components/KeepiValidatedInput.vue'
import { Button } from '@/components/ui/button'
import Checkbox from '@/components/ui/checkbox/Checkbox.vue'
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import {
  Field,
  FieldDescription,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldSet,
  FieldTitle,
} from '@/components/ui/field'
import { Label } from '@/components/ui/label'
import { hasMaxLength, requiredValidator } from '@/regle'
import { handleApiError } from '@/error'
import { useRegle } from '@regle/core'
import { withMessage } from '@regle/rules'
import { Plus, Trash } from 'lucide-vue-next'
import { nextTick, ref } from 'vue'

const props = defineProps<{
  existingProjects: IGetAllProjectsResponse['projects']
}>()

const emits = defineEmits<{
  (e: 'project-created'): void
}>()

const apiClient = new ApiClient()
const open = ref(false)

const users = await apiClient.getAllUsers().match(
  (response) => response.users,
  (error) => {
    handleApiError(error)
    return []
  },
)

const disableUserInteraction = ref(false)
const formValues = ref(getEmptyFormValues(users))
const { r$ } = useRegle(formValues, {
  name: {
    required: requiredValidator,
    maxLength: hasMaxLength(64),
    unique: withMessage((value) => {
      if (value == null || typeof value !== 'string') {
        return true
      }

      return !props.existingProjects.some((p) => p.name === value)
    }, 'Deze naam is al reeds in gebruik'),
  },
  enabled: {
    required: requiredValidator,
  },
  users: {
    $each: {
      id: { required: requiredValidator },
      value: { required: requiredValidator },
    },
  },
  invoiceItems: {
    $each: {
      name: {
        required: requiredValidator,
        maxLength: hasMaxLength(64),
        unique: withMessage((value) => {
          if (value == null || typeof value !== 'string') {
            return true
          }

          return formValues.value.invoiceItems.filter((i) => i.name === value).length <= 1
        }, 'Naam komt meerdere keren voor'),
      },
    },
  },
})

const onOpen = (ev: Event) => {
  ev.preventDefault()

  formValues.value = getEmptyFormValues(users)
  r$.$reset()

  nextTick(() => {
    const element = document.getElementById('name')
    if (element != null) {
      element.focus()
    } else {
      console.error('Failed to focus on first input field')
    }
  })
}

const onAddInvoiceItem = () => {
  const id =
    formValues.value.invoiceItems.reduce<number>(
      (maxId, item) => (maxId < item.id ? item.id : maxId),
      0,
    ) + 1
  formValues.value.invoiceItems.push({ id, name: '' })

  nextTick(() => {
    const element = document.getElementById(`invoiceitem-${id}`)
    if (element != null) {
      element.focus()
    } else {
      console.error('Failed to focus on newly added invoice item')
    }
  })
}

const onDeleteInvoiceItem = (invoiceItemIndex: number) => {
  if (invoiceItemIndex < 0 || invoiceItemIndex >= formValues.value.invoiceItems.length) {
    return
  }

  formValues.value.invoiceItems.splice(invoiceItemIndex, 1)
}

const onSubmit = async () => {
  if (disableUserInteraction.value) {
    return
  }

  disableUserInteraction.value = true

  try {
    const { valid, data } = await r$.$validate()
    if (valid) {
      await apiClient
        .createProject({
          name: data.name,
          enabled: data.enabled,
          userIds: data.users.filter((u) => u.value).map((u) => u.id),
          invoiceItemNames: data.invoiceItems.map((i) => i.name),
        })
        .match(
          () => {
            emits('project-created')
            open.value = false
          },
          (error) => {
            handleApiError(error)
          },
        )
    }
  } finally {
    disableUserInteraction.value = false
  }
}

function getEmptyFormValues(users: IGetAllUsersResponse['users']) {
  return {
    name: '',
    enabled: true,
    invoiceItems: [{ id: 0, name: '' }],
    users: users.map((u) => ({ id: u.id, name: u.id.toString(), label: u.name, value: false })),
  }
}
</script>

<template>
  <Dialog v-model:open="open">
    <div>
      <DialogTrigger as-child>
        <Button variant="outline">Nieuw project </Button>
      </DialogTrigger>

      <DialogContent
        class="max-h-screen overflow-y-scroll"
        @open-auto-focus="onOpen"
        tabindex="null">
        <DialogHeader>
          <DialogTitle>Nieuw project</DialogTitle>

          <DialogDescription>
            Maak een nieuw project aan om werktijden op te registreren.
          </DialogDescription>
        </DialogHeader>

        <div class="grid gap-4">
          <Label>
            Naam
            <KeepiValidatedInput :field="r$.$fields.name" id="name" v-model="formValues.name" />
          </Label>

          <FieldGroup>
            <FieldSet class="gap-4">
              <FieldLegend>Beschikbaarheid</FieldLegend>

              <FieldDescription>
                Hiermee kan het project wel of niet beschikbaar worden gesteld voor werktijd
                registratie.
              </FieldDescription>

              <Label>
                Beschikbaar
                <Checkbox id="enabled" v-model="formValues.enabled" />
              </Label>
            </FieldSet>
          </FieldGroup>

          <FieldGroup v-if="r$.$value.users.length > 0">
            <FieldSet class="gap-4">
              <FieldLegend>Gebruikers</FieldLegend>

              <FieldDescription>
                Selecteer de gebruikers die voor dit project werktijden mogen registreren
              </FieldDescription>

              <FieldGroup class="flex flex-row flex-wrap gap-2 [--radius:9999rem]">
                <FieldLabel
                  v-for="option in r$.$value.users"
                  :key="option.id"
                  :for="option.name"
                  class="w-fit!">
                  <Field
                    orientation="horizontal"
                    class="gap-1.5 overflow-hidden px-3! py-1.5! transition-all duration-100 ease-linear group-has-data-[state=checked]/field-label:px-2!">
                    <Checkbox
                      :id="option.name"
                      v-model="option.value"
                      class="-ml-6 -translate-x-1 rounded-full transition-all duration-100 ease-linear data-[state=checked]:ml-0 data-[state=checked]:translate-x-0" />

                    <FieldTitle>{{ option.label }}</FieldTitle>
                  </Field>
                </FieldLabel>
              </FieldGroup>
            </FieldSet>
          </FieldGroup>

          <FieldGroup>
            <FieldSet class="gap-4">
              <FieldLegend>Factuurposten</FieldLegend>

              <FieldDescription>
                Op deze posten worden de werktijden geregistreerd
              </FieldDescription>

              <div
                class="flex space-x-2"
                v-for="(invoiceItem, invoiceItemIndex) in r$.$value.invoiceItems"
                :key="invoiceItem.id">
                <KeepiValidatedInput
                  :id="`invoiceitem-${invoiceItem.id}`"
                  :field="r$.$fields.invoiceItems.$each[invoiceItemIndex].$fields.name"
                  v-model="r$.$value.invoiceItems[invoiceItemIndex].name"
                  placeholder="Naam facturatiepost" />

                <Button
                  variant="destructive"
                  :disabled="formValues.invoiceItems.length < 2"
                  @click="() => onDeleteInvoiceItem(invoiceItemIndex)">
                  <Trash />
                </Button>
              </div>

              <Button variant="secondary" @click="onAddInvoiceItem">
                <Plus />
              </Button>
            </FieldSet>
          </FieldGroup>
        </div>

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
