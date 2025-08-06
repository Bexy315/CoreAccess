<template>
  <Dialog v-model:visible="visible" modal header="Edit User" style="width: 80%; height: 100%">
    <Stepper value="1">
      <StepList>
        <Step value="1">Basic</Step>
        <Step value="4">Overview</Step>
      </StepList>

      <StepPanels>
        <!-- Step 1: Basic -->
        <StepPanel v-slot="{ activateCallback }" value="1">
          <div>
            <span class="mb-2">Basic Info:</span>
            <div class="flex flex-row gap-3 mt-2">
              <InputText v-model="editUser.username" placeholder="Username" />
              <InputText v-model="editUser.email" placeholder="Email" />
            </div>
          </div>
          <div class="mt-2 flex flex-col gap-3">
            <InputText v-model="editUser.firstName" placeholder="First Name" />
            <InputText v-model="editUser.lastName" placeholder="Last Name" />
            <InputText v-model="editUser.phone" placeholder="Phone" />
            <InputText v-model="editUser.address" placeholder="Address" />
            <InputText v-model="editUser.city" placeholder="City" />
            <InputText v-model="editUser.state" placeholder="State" />
            <InputText v-model="editUser.zip" placeholder="ZIP Code" />
            <InputText v-model="editUser.country" placeholder="Country" />
          </div>

          <!-- Optional: Rollen & Custom Fields -->
          <!--
          <MultiSelect
            v-model="selectedRoles"
            :options="availableRoles"
            optionLabel="name"
            placeholder="Rollen wählen"
            class="mt-4"
          />

          <div v-for="field in customFields" :key="field.key" class="mt-2">
            <InputText
              v-if="field.type === 'Text'"
              v-model="customValues[field.key]"
              :placeholder="field.name"
            />
            <InputNumber
              v-else-if="field.type === 'Number'"
              v-model="customValues[field.key]"
              :placeholder="field.name"
            />
            <Checkbox
              v-else-if="field.type === 'Boolean'"
              v-model="customValues[field.key]"
              :binary="true"
            />
          </div>
          -->

          <div class="flex justify-end pt-4 absolute bottom-2 pr-12 w-full">
            <Button label="Next" icon="pi pi-arrow-right" @click="activateCallback('4')" />
          </div>
        </StepPanel>

        <!-- Step 4: Overview -->
        <StepPanel v-slot="{ activateCallback }" value="4">
          <div class="space-y-2">
            <p><b>Username:</b> {{ editUser.username }}</p>
            <p><b>Email:</b> {{ editUser.email }}</p>
            <p><b>First Name:</b> {{ editUser.firstName }}</p>
            <p><b>Last Name:</b> {{ editUser.lastName }}</p>
            <p><b>Phone:</b> {{ editUser.phone }}</p>
            <p><b>Address:</b> {{ editUser.address }}</p>
            <p><b>City:</b> {{ editUser.city }}</p>
            <p><b>State:</b> {{ editUser.state }}</p>
            <p><b>ZIP Code:</b> {{ editUser.zip }}</p>
            <p><b>Country:</b> {{ editUser.country }}</p>
          </div>
          <div class="flex justify-end pt-4 absolute bottom-2 pr-12 w-full">
            <Button class="mr-2" label="Back" icon="pi pi-arrow-left" @click="activateCallback('1')" />
            <Button label="Update" icon="pi pi-check" @click="submit()" severity="success" />
          </div>
        </StepPanel>
      </StepPanels>
    </Stepper>
  </Dialog>
</template>

<script lang="ts" setup>
import { reactive, watch } from 'vue';
import { type CoreUserUpdateRequest } from '../../model/CoreUserModel.ts';
import { updateUser } from '../../services/UserService.ts';
import { showError } from '../../utils/toast.ts';

const visible = defineModel<boolean>();
const props = defineProps<{
  user: CoreUserUpdateRequest;
}>();
const emit = defineEmits(['submit']);

const editUser = reactive<CoreUserUpdateRequest>({ ...props.user });

watch(
    () => props.user,
    (newUser) => Object.assign(editUser, newUser),
    { deep: true }
);

// const selectedRoles = ref([] as CoreRole[]);
// const availableRoles = ref<CoreRole[]>([]); // Könnte über API geladen werden

// const customFields = ref<Array<{ key: string; name: string; type: string }>>([]);
// const customValues = reactive<Record<string, any>>({});

async function submit() {
  try {
    console.log('Updating user:', editUser);
    await updateUser("", editUser); // Muss natürlich die ID enthalten
    emit('submit');
  } catch (error) {
    console.error('Error updating user:', error);
    showError('Fehler beim Aktualisieren des Benutzers.');
  }
}
</script>
