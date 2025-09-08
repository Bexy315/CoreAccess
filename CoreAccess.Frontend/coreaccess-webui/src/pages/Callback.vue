<template>
  <div>Processing login...</div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useRouter, useRoute } from "vue-router";
import { handleCallback } from "../services/auth.ts";

const router = useRouter();
const route = useRoute();

onMounted(async () => {
  const code = route.query.code as string;
  if (code) {
    await handleCallback(code);
    router.push("/dashboard");
  } else {
    router.push("/");
  }
});
</script>
