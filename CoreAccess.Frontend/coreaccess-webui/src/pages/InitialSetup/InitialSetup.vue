<script setup lang="ts">
import { ref, computed } from 'vue';
import SetupReviewMatrix from './SetupReviewMatrix.vue';

const generalInitialSettings = ref({
  baseUri: '',
  systemLogLevel: 'Information',
  disableRegistration: 'false'
})

const jwtInitialSettings = ref({
  jwtSecret: '',
  issuer: '',
  audience: '',
  expiresIn: '3600'
})

const userInitialSettings = ref({
  admin: {
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
    status: 1
  }
})

const disableRegistrationBool = computed({
  get: () => generalInitialSettings.value.disableRegistration === 'true',
  set: (val: boolean) => {
    generalInitialSettings.value.disableRegistration = val.toString()
  }
})

const submit = () => {
  const payload = {
    generalInitialSettings: generalInitialSettings.value,
    jwtInitialSettings: jwtInitialSettings.value,
    userInitialSettings: userInitialSettings.value
  }

  console.log('Submitted payload:', payload)
}

const isMissing = (val: unknown) => {
  return !val || (typeof val === 'string' && val.trim() === '')
}

const hasMissingRequired = computed(() => {
  return isMissing(generalInitialSettings.value.baseUri)
      || isMissing(generalInitialSettings.value.systemLogLevel)
      || isMissing(jwtInitialSettings.value.jwtSecret)
      || isMissing(jwtInitialSettings.value.issuer)
      || isMissing(jwtInitialSettings.value.audience)
      || isMissing(jwtInitialSettings.value.expiresIn)
      || isMissing(userInitialSettings.value.admin.username)
      || isMissing(userInitialSettings.value.admin.password)
})

</script>

<template>
  <div class="max-w-3xl mx-auto mt-12 px-4">
    <h1 class="text-3xl font-bold text-primary text-center mb-10">CoreAccess Setup</h1>
    <div class="card">
      <Stepper value="1">
        <StepList>
          <Step value="1">General Settings</Step>
          <Step value="2">JWT Settings</Step>
          <Step value="3">Create Admin User</Step>
          <Step value="4">Confirm</Step>
        </StepList>
        <StepPanels>
          <!-- General Settings -->
          <StepPanel v-slot="{ activateCallback }" value="1">
            <div class="flex flex-col gap-4">
              <h2 class="text-xl font-semibold">General Settings</h2>
              <p class="text-gray-500 text-sm">
                Define the basic configuration of your CoreAccess instance, including the base URI and logging level.
              </p>
              <div>
                <label class="block mb-1 font-medium">Base URI</label>
                <InputText v-model="generalInitialSettings.baseUri" placeholder="https://yourapp.com" class="w-full" />
                <small class="text-gray-500">Used for email links, redirects etc.</small>
              </div>

              <div>
                <label class="block mb-1 font-medium">System Log Level</label>
                <InputText v-model="generalInitialSettings.systemLogLevel" placeholder="Information / Debug / Error" class="w-full" />
              </div>

              <div class="flex items-center">
                <Checkbox v-model="disableRegistrationBool" inputId="disableReg" :binary="true" />
                <label for="disableReg" class="ml-2">Disable User Registration</label>
              </div>
            </div>

            <div class="flex justify-end pt-6">
              <Button label="Next" icon="pi pi-arrow-right" iconPos="right" @click="activateCallback('2')" />
            </div>
          </StepPanel>

          <!-- JWT Settings -->
          <StepPanel v-slot="{ activateCallback }" value="2">
            <div class="flex flex-col gap-4">
              <h2 class="text-xl font-semibold">JWT Settings</h2>
              <p class="text-gray-500 text-sm">
                Configure how authentication tokens are generated, including secrets and token expiration.
              </p>
                <div>
                  <label class="block mb-1 font-medium">JWT Secret</label>
                  <InputText v-model="jwtInitialSettings.jwtSecret" class="w-full" />
                </div>
                <div>
                  <label class="block mb-1 font-medium">Issuer</label>
                  <InputText v-model="jwtInitialSettings.issuer" class="w-full" />
                </div>
                <div>
                  <label class="block mb-1 font-medium">Audience</label>
                  <InputText v-model="jwtInitialSettings.audience" class="w-full" />
                </div>
                <div>
                  <label class="block mb-1 font-medium">Token Expiry (seconds)</label>
                  <InputText v-model="jwtInitialSettings.expiresIn" class="w-full" />
                </div>
            </div>

            <div class="flex justify-between pt-6">
              <Button label="Back" severity="secondary" icon="pi pi-arrow-left" @click="activateCallback('1')" />
              <Button label="Next" icon="pi pi-arrow-right" iconPos="right" @click="activateCallback('3')" />
            </div>
          </StepPanel>

          <!-- Admin User -->
          <StepPanel v-slot="{ activateCallback }" value="3">
            <div class="flex flex-col gap-6">
              <h2 class="text-xl font-semibold">Admin User</h2>
              <p class="text-gray-500 text-sm">
                Create the initial administrator account that will manage users, roles and settings.
              </p>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block mb-1 font-medium">Username *</label>
                  <InputText v-model="userInitialSettings.admin.username" class="w-full" />
                </div>

                <div>
                  <label class="block mb-1 font-medium">Password *</label>
                  <InputText type="password" v-model="userInitialSettings.admin.password" class="w-full" />
                </div>
              </div>

              <div class="border p-4 rounded-lg bg-surface-100 dark:bg-surface-800">
                <p class="text-sm mb-4 text-gray-500">The following fields are optional:</p>

                <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <InputText v-model="userInitialSettings.admin.email" placeholder="Email" class="w-full" />
                  <InputText v-model="userInitialSettings.admin.firstName" placeholder="First Name" class="w-full" />
                  <InputText v-model="userInitialSettings.admin.lastName" placeholder="Last Name" class="w-full" />
                  <InputText v-model="userInitialSettings.admin.phone" placeholder="Phone" class="w-full" />
                  <InputText v-model="userInitialSettings.admin.address" placeholder="Address" class="w-full" />
                  <InputText v-model="userInitialSettings.admin.city" placeholder="City" class="w-full" />
                  <InputText v-model="userInitialSettings.admin.state" placeholder="State" class="w-full" />
                  <InputText v-model="userInitialSettings.admin.zip" placeholder="ZIP Code" class="w-full" />
                  <InputText v-model="userInitialSettings.admin.country" placeholder="Country" class="w-full" />
                </div>
              </div>
            </div>

            <div class="flex justify-between pt-6">
              <Button label="Back" severity="secondary" icon="pi pi-arrow-left" @click="activateCallback('2')" />
              <Button label="Next" icon="pi pi-arrow-right" iconPos="right" @click="activateCallback('4')" />
            </div>
          </StepPanel>

          <!-- Confirm -->
          <StepPanel v-slot="{ activateCallback }" value="4">
            <div class="flex flex-col gap-4">
              <h2 class="text-xl font-semibold">Confirm Setup</h2>
              <p class="text-gray-600">Please review your settings and click "Finish" to complete the setup.</p>

              <SetupReviewMatrix
                  :general="generalInitialSettings"
                  :jwt="jwtInitialSettings"
                  :admin="userInitialSettings.admin"
              />

            </div>
            <div class="flex justify-between pt-6">
              <Button label="Back" severity="secondary" icon="pi pi-arrow-left" @click="activateCallback('3')" />
              <div v-if="!hasMissingRequired">
              <Button label="Finish" icon="pi pi-check" severity="success" iconPos="right" @click="submit" :disabled="hasMissingRequired"  />
              </div>
              <div v-else-if="hasMissingRequired">
                <Button label="Finish" icon="pi pi-check" severity="success" iconPos="right" @click="submit" :disabled="hasMissingRequired" v-tooltip="'Please fill out all required fields'" />
              </div>
            </div>
          </StepPanel>
        </StepPanels>
      </Stepper>
    </div>
  </div>
</template>

<style scoped>

</style>
