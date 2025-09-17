<template>
  <div>Processing login...</div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { handleCallback } from "../services/auth.ts";
import {showError} from "../utils/toast.ts";

const router = useRouter();
const route = useRoute();

onMounted(async () => {
  const code = route.query.code as string;
  const state = route.query.state as string;
  if (code) {
    await handleCallback(code);
    router.push(state??"/");
  } else {
    showError("No code found in the callback URL");
    router.push("/");
  }
});
</script>
