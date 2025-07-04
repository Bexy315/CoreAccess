<script setup lang="ts">
import AppLayout from "./layouts/AppLayout.vue";
import {onMounted, ref} from "vue";
import {restoreAuth} from "./services/AuthService.ts";
import {registerGlobalToast, showError} from './utils/toast';
import {useToast} from "primevue/usetoast";
import {getAppConfig} from "./services/AppConfigService.cs.ts";
import {useAppStateStore} from "./stores/AppStateStore.ts";
import {useRouter} from "vue-router";

const toastRef = ref(useToast());
const loading = ref(true)
const appStateStore = useAppStateStore();
const router = useRouter();

onMounted(async () => {
  try {
    if (toastRef.value) {
      registerGlobalToast(toastRef.value);
    }
    restoreAuth();
    const appConfigResponse = await getAppConfig().catch((error => {
      throw error;
    }));
    appStateStore.setInitiated(appConfigResponse.data.isSetupComplete);

    if (!appStateStore.isInitiated && router.currentRoute.value.path !== '/initial-setup') {
      router.push('/initial-setup');
    } else if (appStateStore.isInitiated && router.currentRoute.value.path === '/initial-setup') {
      router.push('/');
    }
    loading.value = false;
  }catch (error) {
    console.error('Error during app initialization:', error);
    showError('Failed to initialize the application. Please try again later.');
  }
});
</script>

<template>
  <div>
    <div v-if="loading" class="flex items-center justify-center h-screen">
      <ProgressSpinner />
      <Toast ref="toastRef" position="bottom-left" />
    </div>
    <div v-if="!loading">
      <AppLayout />
    </div>
  </div>
</template>

<style scoped>
</style>