<template>
  <div class="flex flex-col gap-4 items-start p-4 text-white max-w-sm mx-auto">
    <h2 class="text-xl font-semibold mb-4">Login</h2>
    <InputText v-model="username" placeholder="Benutzername" class="w-full" />
    <Password v-model="password" placeholder="Passwort" class="w-full" toggleMask />
    <Button label="Login" class="w-full" @click="onLogin" />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import InputText from 'primevue/inputtext';
import Password from 'primevue/password';
import Button from 'primevue/button';

const username = ref('');
const password = ref('');

function onLogin() {
  console.log('Login attempt', username.value, password.value);
}
</script>


<style scoped>

</style>