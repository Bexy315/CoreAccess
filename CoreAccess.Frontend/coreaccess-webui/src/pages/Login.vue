<template>
  <div class="flex items-center justify-center min-h-screen">
    <div class="w-full max-w-md px-4">
      <Card class="shadow-md rounded-md">
        <template #title>Login</template>
        <template #content>
          <form @submit.prevent="onSubmit" class="space-y-5">
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

            <Button type="submit" label="Login" class="w-full" :loading="isLoading"/>
          </form>
        </template>
      </Card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { login } from '../services/AuthService'
import {showError, showSuccess} from "../utils/toast.ts";

const email = ref('')
const password = ref('')
const submitted = ref(false)
const isLoading = ref(false)

const onSubmit = () => {
  submitted.value = true
  if (!email.value || !password.value) return

  isLoading.value = true

  login({ username: email.value, password: password.value })
      .then(() => {
            showSuccess("Login erfolgreich", "Willkommen zurück!")
          }
      )
      .catch((error) => {
        showError("Login fehlgeschlagen", error.message || "Bitte überprüfen Sie Ihre Anmeldedaten.")
      })
  isLoading.value = false
}
</script>

<style scoped>
.p-invalid {
  border-color: lightcoral !important;
}
</style>