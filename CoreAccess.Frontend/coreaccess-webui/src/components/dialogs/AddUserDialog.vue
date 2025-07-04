<template>
  <Dialog v-model:visible="visible" modal header="Benutzer erstellen" style="width: 50rem">
    <Stepper value="1">
      <StepList>
        <Step value="1">Basis</Step>
        <Step value="2">Rollen</Step>
        <Step value="3">Custom-Felder</Step>
        <Step value="4">Übersicht</Step>
      </StepList>

      <StepPanels>
        <!-- Step 1: Basis -->
        <StepPanel v-slot="{ activateCallback }" value="1">
          <div class="flex flex-col gap-3">
            <InputText v-model="user.username" placeholder="Benutzername" />
            <InputText v-model="user.email" placeholder="E-Mail" />
          </div>
          <div class="flex justify-end pt-4">
            <Button label="Weiter" icon="pi pi-arrow-right" @click="activateCallback('2')" />
          </div>
        </StepPanel>

        <!-- Step 2: Rollen -->
        <StepPanel v-slot="{ activateCallback }" value="2">
          <div>
            <MultiSelect
                v-model="user.roles"
                :options="availableRoles"
                optionLabel="name"
                placeholder="Rollen auswählen"
            />
            <div class="mt-2">
              <InputSwitch v-model="user.isActive" />
              <span class="ml-2">Aktiv</span>
            </div>
          </div>
          <div class="flex justify-between pt-4">
            <Button label="Zurück" icon="pi pi-arrow-left" @click="activateCallback('1')" />
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
          <div class="flex justify-between pt-4">
            <Button label="Zurück" icon="pi pi-arrow-left" @click="activateCallback('2')" />
            <Button label="Weiter" icon="pi pi-arrow-right" @click="activateCallback('4')" />
          </div>
        </StepPanel>

        <!-- Step 4: Übersicht -->
        <StepPanel v-slot="{ activateCallback }" value="4">
          <div class="space-y-2">
            <p><b>Username:</b> {{ user.username }}</p>
            <p><b>E-Mail:</b> {{ user.email }}</p>
            <p><b>Rollen:</b> {{ user.roles.map(r => r.name).join(', ') }}</p>
            <div class="mt-2">
              <b>Custom-Felder:</b>
              <ul class="list-disc pl-4">
                <li v-for="f in customFields" :key="f.key">
                  {{ f.name }}: {{ customValues[f.key] }}
                </li>
              </ul>
            </div>
          </div>
          <div class="flex justify-between pt-4">
            <Button label="Zurück" icon="pi pi-arrow-left" @click="activateCallback('3')" />
            <Button label="Erstellen" icon="pi pi-check" @click="submit()" />
          </div>
        </StepPanel>
      </StepPanels>
    </Stepper>
  </Dialog>
</template>

<script lang="ts" setup>
import { ref, reactive, watch } from 'vue';

const visible = defineModel<boolean>();

const user = reactive({
  username: '',
  email: '',
  roles: [] as Array<{ id: string; name: string }>,
  isActive: true
});

const availableRoles = ref([
  { id: 'admin', name: 'Administrator' },
  { id: 'user', name: 'Benutzer' }
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

function submit() {
  const payload = {
    ...user,
    customFieldValues: customFields.value.map((f) => ({
      fieldDefinitionKey: f.key,
      value: String(customValues[f.key] ?? '')
    }))
  };
  console.log('Erstellen Payload:', payload);
  visible.value = false;
}
</script>
