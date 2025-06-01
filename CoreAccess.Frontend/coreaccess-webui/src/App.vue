<script setup lang="ts">
import AppLayout from "./layouts/AppLayout.vue";
import {onMounted, ref} from "vue";
import {restoreAuth} from "./services/AuthService.ts";
import { registerGlobalToast } from './utils/toast';
import {useToast} from "primevue/usetoast";

const toastRef = ref(useToast());
const loading = ref(true)

onMounted(() => {
  if (toastRef.value) {
    registerGlobalToast(toastRef.value);
  }
  restoreAuth()
  loading.value = false
})
</script>

<template>
  <div>
    <div v-if="loading" class="flex items-center justify-center h-screen">
      <ProgressSpinner />
    </div>
    <div v-if="!loading">
      <AppLayout />
    </div>
  </div>
</template>

<style scoped>
</style>
