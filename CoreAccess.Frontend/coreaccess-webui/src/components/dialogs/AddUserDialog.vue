<template>
  <Dialog v-model:visible="visible" modal header="Create User">
          <div class="pb-4">
            <span>Credentials:</span>
            <div class="flex flex-row gap-3 mt-2">
              <InputText v-model="user.username" placeholder="Username" />
              <Password v-model="user.password" placeholder="Password" toggleMask />
            </div>
          </div>
    <Button label="Create" icon="pi pi-check" @click="submit()" severity="success" />
  </Dialog>
</template>

<script lang="ts" setup>
import { ref, reactive, watch } from 'vue';
import {type CoreUserCreateRequest, CoreUserStatus} from "../../model/CoreUserModel.ts";
import {type RoleDto } from "../../model/CoreRoleModel.ts";
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

const selectedRoles = ref([] as RoleDto[]);

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
