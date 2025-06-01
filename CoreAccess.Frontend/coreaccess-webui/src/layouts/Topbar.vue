<script setup lang="ts">
import {ref} from "vue";
import {logout} from "../services/AuthService.ts";
import {useRouter} from "vue-router";

const userMenuRef = ref();
const router = useRouter();

const toggleUserMenu = (event: MouseEvent) => {
  userMenuRef.value?.toggle(event);
};

const userMenu = [
  { label: 'Profil', icon: 'pi pi-user', command: () => router.push('/profile') },
  { separator: true },
  { label: 'Logout', icon: 'pi pi-sign-out', command: () => logout() },
];

</script>

<template>
  <Menubar :model="[]" class="border-0! rounded-none! shadow-none! bg-gray-800">
    <template #start>
      <router-link to="/" class="font-bold text-lg ml-2 text-white">CoreAccess</router-link>
    </template>
    <template #end>
      <div class="flex items-center gap-2">
        <Avatar
            icon="pi pi-user"
            shape="circle"
            style="background-color: #6366f1; color: white"
            class="cursor-pointer"
            @click="toggleUserMenu"
        />
        <Menu ref="userMenuRef" :model="userMenu" :popup="true" />
      </div>
    </template>
  </Menubar>
</template>

<style scoped>

</style>