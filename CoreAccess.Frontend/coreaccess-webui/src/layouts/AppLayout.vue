<template>
  <div class="h-screen flex flex-col">
    <div v-if="isLoggedIn">
      <Topbar />
    </div>

    <!-- Main Layout -->
    <div class="flex flex-1">
      <div v-if="isLoggedIn">
        <Sidebar />
      </div>

      <div class="flex-1 border-1 border-gray-400">
        <!-- Page content -->
        <div class="pl-2">
          <RouterView />
        </div>
      </div>
    </div>
    <Toast position="bottom-left"/>
  </div>
</template>

<script setup lang="ts">
import {ref, watch} from 'vue';
import {isAuthenticatedRef} from "../services/auth.ts";

const isLoggedIn = ref(false);

watch(() => isAuthenticatedRef.value, (newValue) => {
  isLoggedIn.value = newValue;
}, { immediate: true });
</script>

<style scoped>
</style>
