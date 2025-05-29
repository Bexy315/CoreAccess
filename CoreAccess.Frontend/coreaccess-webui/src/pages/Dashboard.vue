<script setup lang="ts">
import {logout} from "../services/AuthService.ts";
</script>

<template>
  <div class="p-4 text-white">
    <h1 class="text-2xl font-bold mb-2">Willkommen bei CoreAccess</h1>
    <p>Dashboard oder Landingpage</p>
    <Button @click="logout"></Button>
  </div>
</template>

<style scoped>

</style>