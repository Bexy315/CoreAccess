<script setup lang="ts">
import {showError, showSuccess} from "../utils/toast.ts";
import {PingBackend} from "../services/DebugService.ts";
import {ref} from "vue";

const isLoading = ref(false);

const pingBackend = async () => {
  isLoading.value = true;
  try {
    return await PingBackend().then(res =>
      showSuccess(`Backend antwortet: ${res}`)
    ).catch(err => showError(err.message));
  } catch (error: any) {
    showError(`Error pinging backend: ${error.message}`);
  } finally {
    isLoading.value = false;
  }
};
</script>

<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-2">Willkommen bei CoreAccess</h1>
    <p>Dashboard seite</p>
    <p>Diese Seite ist noch in Arbeit.</p>
    <Button label="Ping" @click="pingBackend"></Button>
  </div>
</template>

<style scoped>
</style>