<!-- UserDetailDialogWrapper.vue -->
<template>
  <Dialog
      v-model:visible="visible"
      modal
      header="User Details"
      style="width: 500px"
      @hide="closeDialog"
  >
    <template v-if="loading">
      <ProgressSpinner />
    </template>

    <template v-else>
      <div v-if="user">
        <p><strong>ID:</strong> {{ user.id }}</p>
        <p><strong>Username:</strong> {{ user.username }}</p>
        <p><strong>Email:</strong> {{ user.email }}</p>
      </div>
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {fetchUser} from "../../services/UserService.ts";

const route = useRoute()
const router = useRouter()

const visible = ref(true)
const loading = ref(true)
const user = ref<any | null>(null)

const loadUser = async () => {
  loading.value = true
  user.value = await fetchUser(route.params.id as string)
  loading.value = false
}

onMounted(loadUser)

watch(() => route.params.id, loadUser)

const closeDialog = () => {
  router.push({ path: '/users', query: route.query })
}
</script>
