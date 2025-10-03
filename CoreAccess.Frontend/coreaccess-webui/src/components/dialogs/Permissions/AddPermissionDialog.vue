<script setup lang="ts">
import {showError} from "../../../utils/toast.ts";
import {createRole} from "../../../services/RoleService.ts";
import {ref, watch} from "vue";
import {createPermission} from "../../../services/PermissionService.ts";

const visible = defineModel<boolean>();

const emit = defineEmits(['submit']);

const name = ref<string>('');
const description = ref<string>('');

watch(visible, (v) => {
  if (v) {
    name.value = '';
    description.value = '';
  }
});

const submitDisabled = ref(true);

watch(name, () => {
  submitDisabled.value = !name.value || !description.value;
}, { deep: true });

watch(description, () => {
  submitDisabled.value = !name.value || !description.value;
}, { deep: true });

async function submit() {
  await createPermission(name.value, description.value).catch((error) => {
    console.error('Error creating role:', error);
    showError('Fehler beim Erstellen der Rolle. Bitte überprüfen Sie die Eingaben und versuchen Sie es erneut.');
  });

  emit('submit')
}
</script>

<template>
  <Dialog v-model:visible="visible" modal header="Create Permission">
    <div class="pb-4">
      <span>Details:</span>
      <div class="flex flex-row gap-3 mt-2">
        <InputText v-model="name" placeholder="Name" />
        <InputText v-model="description" placeholder="Description" />
      </div>
    </div>
    <Button label="Create" icon="pi pi-check" @click="submit()" severity="success" :disabled="submitDisabled" />
  </Dialog>
</template>

<style scoped>

</style>