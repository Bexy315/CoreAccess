<script setup lang="ts">
import AppLayout from "./layouts/AppLayout.vue";
import {onMounted, ref} from "vue";
import {restoreAuth} from "./services/AuthService.ts";
import { registerGlobalToast } from './utils/toast';
import {useToast} from "primevue/usetoast";
import {getAppConfig} from "./services/AppConfigService.cs.ts";
import {useAppStateStore} from "./stores/AppStateStore.ts";
import {useRouter} from "vue-router";

const toastRef = ref(useToast());
const loading = ref(true)
const appStateStore = useAppStateStore();
const router = useRouter();

onMounted(async () => {
  if (toastRef.value) {
    registerGlobalToast(toastRef.value);
  }
  restoreAuth();
  const appConfigResponse = await getAppConfig();
  appStateStore.setInitiated(appConfigResponse.data.isSetupComplete);

  if (!appStateStore.isInitiated && router.currentRoute.value.path !== '/initial-setup') {
    router.push('/initial-setup');
  }else if(appStateStore.isInitiated && router.currentRoute.value.path === '/initial-setup') {
    router.push('/');
  }

  loading.value = false;
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
  </div>
</template>

<style scoped>
</style>