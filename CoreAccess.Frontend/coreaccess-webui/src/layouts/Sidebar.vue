<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();
const isCollapsed = ref(false);
const expandedItems = ref<string[]>([]);

const menuItems = [
  { title: 'Dashboard', icon: 'pi pi-home', href: '/' },
  {
    title: 'Users',
    icon: 'pi pi-users',
    href: '/users',
    children: [
      { title: 'Active Users', icon: 'pi pi-user-plus', href: '/users' },
      { title: 'Inactive Users', icon: 'pi pi-user-minus', href: '/users' },
    ],
  },
  { title: 'Roles', icon: 'pi pi-shield', href: '/roles' },
  { title: 'Permissions', icon: 'pi pi-lock', href: '/permissions' },
  { title: 'Clients', icon: 'pi pi-sitemap', href: '/clients' },
  { title: 'Metrics', icon: 'pi pi-gauge', href: '/metrics',
    children: [
      { title: 'System Logs', icon: 'pi pi-file', href: '/metrics/logs/system' },
      { title: 'Audit Logs', icon: 'pi pi-user', href: '/metrics/logs/audit' },
    ],
  },
  { title: 'App Settings', icon: 'pi pi-cog', href: '/settings' },
];

const navigateTo = (href: string) => {
  router.push(href);
};

const toggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value;
};

const toggleExpand = (title: string) => {
  if (expandedItems.value.includes(title)) {
    expandedItems.value = expandedItems.value.filter(item => item !== title);
  } else {
    expandedItems.value.push(title);
  }
};
</script>

<template>
  <div
      :class="[
      'flex flex-col bg-gray-800 text-white h-full',
      isCollapsed ? 'w-12' : 'w-60'
    ]"
      style="transition: width 0.3s ease"
  >
    <nav class="flex flex-col flex-grow gap-1 p-2">
      <div v-for="item in menuItems" :key="item.title">
        <!-- Parent Menu Item -->
        <div class="flex items-center">
          <button
              @click="navigateTo(item.href)"
              class="flex items-center gap-3 rounded-md p-2 hover:bg-gray-700 transition-colors duration-200 flex-grow"
              :class="['h-12', 'cursor-pointer']"
          >
            <i :class="[item.icon, 'text-lg flex-shrink-0']"></i>
            <span v-if="!isCollapsed" class="truncate">{{ item.title }}</span>
          </button>
          <button
              v-if="item.children"
              @click.stop="toggleExpand(item.title)"
              class="p-2 hover:bg-gray-700 transition-colors duration-200 rounded-md"
          >
            <i
                :class="[
                'pi',
                expandedItems.includes(item.title) ? 'pi-angle-down' : 'pi-angle-right',
                'text-lg'
              ]"
            ></i>
          </button>
        </div>

        <!-- Child Menu Items -->
        <div
            v-if="item.children && expandedItems.includes(item.title)"
            class="pl-6"
        >
          <button
              v-for="child in item.children"
              :key="child.title"
              @click="navigateTo(child.href)"
              class="flex items-center gap-3 rounded-md p-2 hover:bg-gray-700 w-full transition-colors duration-200"
              :class="['h-10', 'cursor-pointer']"
          >
            <i :class="[child.icon, 'text-sm flex-shrink-0']"></i>
            <span v-if="!isCollapsed" class="truncate">{{ child.title }}</span>
          </button>
        </div>
      </div>
    </nav>

    <button
        @click="toggleSidebar"
        class="mt-auto p-3 hover:bg-gray-700 transition-colors duration-200 rounded-md flex justify-center cursor-pointer"
        aria-label="Toggle Sidebar"
    >
      <i :class="isCollapsed ? 'pi pi-angle-double-right' : 'pi pi-angle-double-left'"></i>
    </button>
  </div>
</template>