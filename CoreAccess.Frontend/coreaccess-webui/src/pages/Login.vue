<template>
  <div class="flex items-center justify-center min-h-screen bg-gray-100 px-4">
    <div class="w-full max-w-md">
      <Card class="shadow-lg rounded-xl">
        <template #title>
          <div class="text-center text-2xl font-semibold text-gray-800">Login</div>
        </template>

        <template #content>
          <form @submit.prevent="onSubmit" class="space-y-6">
            <!-- Fehlermeldung -->
            <div v-if="errorMessage" class="border border-red-400 bg-red-50 text-red-700 px-4 py-3 rounded-lg text-sm">
              {{ errorMessage }}
            </div>

            <!-- E-Mail -->
            <div>
              <label for="email" class="block mb-1 font-medium text-gray-700">E-Mail</label>
              <InputText
                  id="email"
                  v-model="email"
                  class="w-full"
                  :invalid="submitted && !email"
                  placeholder="you@example.com"
                  autocomplete="username"
              />
              <small v-if="submitted && !email" class="text-red-500">E-Mail ist erforderlich.</small>
            </div>

            <!-- Passwort -->
            <div>
              <label for="password" class="block mb-1 font-medium text-gray-700">Passwort</label>
              <Password
                  id="password"
                  v-model="password"
                  toggleMask
                  :feedback="false"
                  class="w-full"
                  inputClass="w-full"
                  :inputProps="{ class: submitted && !password ? 'p-invalid' : '', autocomplete: 'current-password' }"
                  placeholder="********"
              />
              <small v-if="submitted && !password" class="text-red-500">Passwort ist erforderlich.</small>
            </div>

            <!-- Submit -->
            <Button
                type="submit"
                label="Login"
                class="w-full bg-blue-600 hover:bg-blue-700 border-0"
                :loading="isLoading"
            />
          </form>
        </template>
      </Card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { login } from '../services/AuthService'

const email = ref('')
const password = ref('')
const submitted = ref(false)
const isLoading = ref(false)
const errorMessage = ref<string | null>(null)

const onSubmit = async () => {
  submitted.value = true
  errorMessage.value = null

  if (!email.value || !password.value) return

  isLoading.value = true

  try {
    await login({ username: email.value, password: password.value })
  } catch (error: any) {
    console.error(error)
    errorMessage.value = "Bitte überprüfen Sie Ihre Anmeldedaten."
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.p-invalid {
  border-color: lightcoral !important;
}
</style>
