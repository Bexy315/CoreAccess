<script setup lang="ts">
import AppLayout from "./layouts/AppLayout.vue";
import {onMounted, ref} from "vue";
import {registerGlobalToast, showError} from './utils/toast';
import {useToast} from "primevue/usetoast";
import {getAppConfig} from "./services/AppConfigService.cs.ts";
import {useAppStateStore} from "./stores/AppStateStore.ts";
import {useRouter} from "vue-router";

const toastRef = ref(useToast());
const loading = ref(true)
const appStateStore = useAppStateStore();
const router = useRouter();


const retryGetAppConfig = async (retries = 3, delay = 1000) => {
  for (let attempt = 0; attempt < retries; attempt++) {
    try {
      return await getAppConfig();
    } catch (error) {
      if (attempt < retries - 1) {
        await new Promise(resolve => setTimeout(resolve, delay));
      } else {
        throw error;
      }
    }
  }
};


onMounted(async () => {
  try {
    if (toastRef.value) {
      registerGlobalToast(toastRef.value);
    }

    const appConfigResponse = await retryGetAppConfig(25);

    if(!appConfigResponse || !appConfigResponse.data) {
      throw new Error('Invalid app config response');
    }

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
    </div>
    <div v-if="!loading">
      <AppLayout />
    </div>
    <Toast ref="toastRef" position="bottom-right" />
  </div>
</template>

<style scoped>
</style>