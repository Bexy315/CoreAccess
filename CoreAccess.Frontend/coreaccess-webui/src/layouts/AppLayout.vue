<template>
  <div class="h-screen flex flex-col">
    <!-- Topbar -->
    <Menubar :model="[]" class="border-0 rounded-none shadow-none bg-gray-900 text-white">
      <template #start>
        <span class="font-bold text-lg ml-2">CoreAccess</span>
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

    <!-- Main Layout -->
    <div class="flex flex-1 overflow-hidden bg-gray-900 text-white">
      <!-- Sidebar -->
      <aside class="w-64 !bg-gray-800 p-2 overflow-y-auto">
        <PanelMenu :model="menuItems" class="w-full text-white" />
      </aside>

      <!-- Page content -->
      <main class="flex-1 bg-gray-800 overflow-y-auto p-6">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';

import Menubar from 'primevue/menubar';
import PanelMenu from 'primevue/panelmenu';
import Avatar from 'primevue/avatar';
import Menu from 'primevue/menu';

const router = useRouter();
const userMenuRef = ref();

const toggleUserMenu = (event: MouseEvent) => {
  userMenuRef.value?.toggle(event);
};

const menuItems = [
  {
    label: 'Verwaltung',
    items: [
      { label: 'Benutzer', icon: 'pi pi-users', command: () => router.push('/users') },
      { label: 'Rollen', icon: 'pi pi-lock', command: () => router.push('/roles') },
      { label: 'Logs', icon: 'pi pi-list', command: () => router.push('/logs') },
      { label: 'Instanz-Einstellungen', icon: 'pi pi-cog', command: () => router.push('/instance-settings') },
    ],
  },
];

const userMenu = [
  { label: 'Profil', icon: 'pi pi-user', command: () => router.push('/profile') },
  { separator: true },
  { label: 'Logout', icon: 'pi pi-sign-out', command: () => console.log('Logout') },
];
</script>

<style scoped>
:deep(.p-menubar) {
  border: none;
}
:deep(.p-menuitem-text), :deep(.p-panelmenu-header), :deep(.p-menuitem-icon) {
  color: white;
}
</style>
