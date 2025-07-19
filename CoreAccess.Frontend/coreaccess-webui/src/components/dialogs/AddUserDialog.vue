<template>
  <Dialog v-model:visible="visible" modal header="Create User" style="width: 80%; height: 100%">
    <Stepper value="1">
      <StepList>
        <Step value="1">Basic</Step>
        <Step value="4">Overview</Step>
      </StepList>

      <StepPanels>
        <!-- Step 1: Basic -->
        <StepPanel v-slot="{ activateCallback }" value="1">
          <div>
            <span class="mb-2">Credentials:</span>
            <div class="flex flex-row gap-3 mt-2">
              <InputText v-model="user.username" placeholder="Username" />
              <Password v-model="user.password" placeholder="Password" toggleMask />
            </div>
          </div>
          <div class="mt-2 flex flex-col gap-3">
            <span class="mt-2 mb-2">Optional:</span>
            <InputText v-model="user.email" placeholder="Email" />
            <InputText v-model="user.firstName" placeholder="First Name" />
            <InputText v-model="user.lastName" placeholder="Last Name" />
            <InputText v-model="user.phone" placeholder="Phone" />
            <InputText v-model="user.address" placeholder="Address" />
            <InputText v-model="user.city" placeholder="City" />
            <InputText v-model="user.state" placeholder="State" />
            <InputText v-model="user.zip" placeholder="ZIP Code" />
            <InputText v-model="user.country" placeholder="Country" />
          </div>
          <div class="flex justify-end pt-4 absolute bottom-2 pr-12 w-full">
            <Button label="Next" icon="pi pi-arrow-right" @click="activateCallback('4')" />
          </div>
        </StepPanel>

        <!-- Step 4: Overview -->
        <StepPanel v-slot="{ activateCallback }" value="4">
          <div class="space-y-2">
            <p><b>Username:</b> {{ user.username }}</p>
            <p><b>Email:</b> {{ user.email }}</p>
            <p><b>First Name:</b> {{ user.firstName }}</p>
            <p><b>Last Name:</b> {{ user.lastName }}</p>
            <p><b>Phone:</b> {{ user.phone }}</p>
            <p><b>Address:</b> {{ user.address }}</p>
            <p><b>City:</b> {{ user.city }}</p>
            <p><b>State:</b> {{ user.state }}</p>
            <p><b>ZIP Code:</b> {{ user.zip }}</p>
            <p><b>Country:</b> {{ user.country }}</p>
          </div>
          <div class="flex justify-end pt-4 absolute bottom-2 pr-12 w-full">
            <Button class="mr-2" label="Back" icon="pi pi-arrow-left" @click="activateCallback('1')" />
            <Button label="Create" icon="pi pi-check" @click="submit()" severity="success" />
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

const selectedStatus =ref({ label: 'Aktiv', value: CoreUserStatus.Active });

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
