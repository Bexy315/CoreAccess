<template>
  <Dialog v-model:visible="visible" modal header="Benutzer erstellen" style="width: 80%; height: 100%">
    <Stepper value="1">
      <StepList>
        <Step value="1">Basis</Step>
        <Step value="2">Metadaten</Step>
        <Step value="3">Custom-Felder</Step>
        <Step value="4">Übersicht</Step>
      </StepList>

      <StepPanels>
        <!-- Step 1: Basis -->
        <StepPanel v-slot="{ activateCallback }" value="1">
          <div>
            <span class="mb-2">Credentials:</span>
            <div class="flex flex-row gap-3 mt-2">
              <InputText v-model="user.username" placeholder="Benutzername" />
              <Password v-model="user.password" placeholder="Password" toggleMask />
            </div>
          </div>
          <div class="mt-2 flex flex-col gap-3">
            <span class="mt-2 mb-2">Optional:</span>
            <InputText v-model="user.email" placeholder="E-Mail" />
            <InputText v-model="user.firstName" placeholder="Vorname" />
            <InputText v-model="user.lastName" placeholder="Nachname" />
            <InputText v-model="user.phone" placeholder="Telefon" />
            <InputText v-model="user.address" placeholder="Adresse" />
            <InputText v-model="user.city" placeholder="Stadt" />
            <InputText v-model="user.state" placeholder="Bundesland" />
            <InputText v-model="user.zip" placeholder="Postleitzahl" />
            <InputText v-model="user.country" placeholder="Land" />
          </div>
          <div class="flex justify-end pt-4 absolute bottom-2 pr-12 w-full">
            <Button label="Weiter" icon="pi pi-arrow-right" @click="activateCallback('2')" />
          </div>
        </StepPanel>

        <!-- Step 2: Metadaten -->
        <StepPanel v-slot="{ activateCallback }" value="2">
          <div>
            <MultiSelect
                v-model="selectedRoles"
                :options="availableRoles"
                optionLabel="name"
                placeholder="Rollen auswählen"
            />
            <div class="mt-4">
              <Select
                  v-model="selectedStatus"
                  :options="availableStatus"
                  optionLabel="label"
                  placeholder="Status auswählen"
              />
            </div>
          </div>
          <div class="flex justify-end pt-4 absolute bottom-2 pr-12 w-full">
            <Button class="mr-2" label="Zurück" icon="pi pi-arrow-left" @click="activateCallback('1')" />
            <Button label="Weiter" icon="pi pi-arrow-right" @click="activateCallback('3')" />
          </div>
        </StepPanel>

        <!-- Step 3: Custom-Felder -->
        <StepPanel v-slot="{ activateCallback }" value="3">
          <div class="space-y-3">
            <div v-for="f in customFields" :key="f.key">
              <label>{{ f.name }}</label>
              <InputText v-if="f.type === 'Text' || f.type === 'Guid'" v-model="customValues[f.key]" />
              <InputNumber v-else-if="f.type === 'Number'" v-model="customValues[f.key]" />
              <div v-else-if="f.type === 'Boolean'" class="flex items-center">
                <InputSwitch v-model="customValues[f.key]" />
                <span class="ml-2">{{ f.name }}</span>
              </div>
            </div>
          </div>
          <div class="flex justify-end pt-4 absolute bottom-2 pr-12 w-full">
            <Button class="mr-2" label="Zurück" icon="pi pi-arrow-left" @click="activateCallback('2')" />
            <Button label="Weiter" icon="pi pi-arrow-right" @click="activateCallback('4')" />
          </div>
        </StepPanel>

        <!-- Step 4: Übersicht -->
        <StepPanel v-slot="{ activateCallback }" value="4">
          <div class="space-y-2">
            <p><b>Username:</b> {{ user.username }}</p>
            <p><b>E-Mail:</b> {{ user.email }}</p>
            <p><b>Rollen:</b> {{ selectedRoles.map(r => r.name).join(', ') }}</p>
            <div class="mt-2">
              <b>Custom-Felder:</b>
              <ul class="list-disc pl-4">
                <li v-for="f in customFields" :key="f.key">
                  {{ f.name }}: {{ customValues[f.key] }}
                </li>
              </ul>
            </div>
          </div>
          <div class="flex justify-end pt-4 absolute bottom-2 pr-12 w-full">
            <Button class="mr-2" label="Zurück" icon="pi pi-arrow-left" @click="activateCallback('3')" />
            <Button label="Erstellen" icon="pi pi-check" @click="submit()" severity="success" />
          </div>
        </StepPanel>
      </StepPanels>
    </Stepper>
  </Dialog>
</template>

<script lang="ts" setup>
import { ref, reactive, watch } from 'vue';
import {type CoreUserCreateRequest, CoreUserStatus} from "../../model/CoreUserModel.ts";
import {type CoreRole } from "../../model/CoreRoleModel.ts";
import {createUser} from "../../services/UserService.ts";
import {showError} from "../../utils/toast.ts";

const visible = defineModel<boolean>();

const emit = defineEmits(['submit']);

const user = reactive<CoreUserCreateRequest>({
  username: '',
  password: '',
  email: '',
  firstName: '',
  lastName: '',
  phone: '',
  address: '',
  city: '',
  state: '',
  zip: '',
  country: '',
  status: CoreUserStatus.Active
});

const selectedRoles = ref([] as CoreRole[]);

const availableRoles = ref([
  { id: '1', name: 'Admin', description: 'Administrator role', createdAt: '2023-01-01T00:00:00Z', updatedAt: '2023-01-01T00:00:00Z', isSystem: true, permissions: [] },
  { id: '2', name: 'Editor', description: 'Editor role', createdAt: '2023-01-01T00:00:00Z', updatedAt: '2023-01-01T00:00:00Z', isSystem: false, permissions: [] },
  { id: '3', name: 'Viewer', description: 'Viewer role', createdAt: '2023-01-01T00:00:00Z', updatedAt: '2023-01-01T00:00:00Z', isSystem: false, permissions: [] }
] as CoreRole[]);

const selectedStatus =ref({ label: 'Aktiv', value: CoreUserStatus.Active });

const availableStatus = ref([
  { label: 'Aktiv', value: CoreUserStatus.Active },
  { label: 'Inaktiv', value: CoreUserStatus.Inactive },
  { label: 'Gesperrt', value: CoreUserStatus.Locked }
]);

const customFields = ref<Array<{ key: string; name: string; type: string }>>([
  { key: 'department', name: 'Abteilung', type: 'Text' },
  { key: 'maxLogins', name: 'Max Logins', type: 'Number' },
  { key: 'isExternal', name: 'Extern', type: 'Boolean' }
]);

const customValues = reactive<Record<string, any>>({});

watch(visible, (v) => {
  if (v) {
    customFields.value.forEach((f) => {
      customValues[f.key] = f.type === 'Boolean' ? false : '';
    });
    Object.assign(user, {
      username: '',
      email: '',
      roles: [],
      isActive: true
    });
  }
});

async function submit() {
  const coreUserCreateRequest: CoreUserCreateRequest = {
    ...user,
    status: selectedStatus.value.value
  };
  const roles = selectedRoles.value.map(role => role.id);
  const customFieldsData = Object.entries(customValues).map(([key, value]) => ({ key, value }));
  console.log('CoreUserCreateRequest:', coreUserCreateRequest);
  console.log('Selected Roles:', roles);
  console.log('Custom Fields:', customFieldsData);

  await createUser(coreUserCreateRequest).catch((error) => {
    console.error('Error creating user:', error);
    showError('Fehler beim Erstellen des Benutzers. Bitte überprüfen Sie die Eingaben und versuchen Sie es erneut.');
  });

  emit('submit')
}
</script>
