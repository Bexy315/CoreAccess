<script setup lang="ts">
import { onMounted, ref } from 'vue';
import {getDashboardMetrics, getHealthStatus, type HealthStatus} from '../services/DashboardService.ts';

const health = ref<HealthStatus | null>(null);
const loading = ref(true);

onMounted(async () => {
  try {
    loading.value = true;
    await getHealthStatus().then(res =>
        health.value = {
      status: res.status,
      uptime: res.uptime,
      checks: res.checks,
    });

    await getDashboardMetrics().then(res => {
      totalUsers.value = res.totalUsers;
      totalRoles.value = res.totalRoles;
      totalPermissions.value = res.totalPermissions;
    });
    loading.value = false;
  } catch {
    health.value = null;
  } finally {
    loading.value = false;
  }
});

function formatUptime(seconds: number): string {
  if (isNaN(seconds)) return 'Invalid uptime';

  const weeks = Math.floor(seconds / (7 * 24 * 60 * 60));
  const days = Math.floor((seconds % (7 * 24 * 60 * 60)) / (24 * 60 * 60));
  const hours = Math.floor((seconds % (24 * 60 * 60)) / (60 * 60));
  const minutes = Math.floor((seconds % (60 * 60)) / 60);
  const remainingSeconds = seconds % 60;

  let formatted = '';
  if (weeks > 0) formatted += `${weeks}w `;
  if (days > 0) formatted += `${days}d `;
  if (hours > 0) formatted += `${hours}h `;
  if (minutes > 0) formatted += `${minutes}m `;
  if (remainingSeconds > 0 || formatted === '') formatted += `${remainingSeconds}s`;

  return formatted.trim();
}

// Dummy stats
const totalUsers = ref(0);
const totalRoles = ref(0);
const totalPermissions = ref(0)
</script>

<template>
  <div class="p-2">
    <h1 class="text-2xl font-bold text-primary mb-2">Willkommen!</h1>
  </div>

  <div class="p-4 grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
    <!-- Quick Stats -->
    <Card class="bg-white shadow rounded-2xl p-4 hover:cursor-pointer hover:border-1 hover:border-gray-300" @click="() => $router.push('/users')">
      <template #title>Total Users</template>
      <template #content>
        <div v-if="loading">
          <Skeleton height="2rem" class="mb-2" />
        </div>
        <div v-else>
        <p class="text-3xl font-bold text-primary">{{ totalUsers }}</p>
        </div>
      </template>
    </Card>

    <Card class="bg-white shadow rounded-2xl p-4 hover:cursor-pointer hover:border-1 hover:border-gray-300" @click="() => $router.push('/roles')">
      <template #title>Roles</template>
      <template #content>
        <div v-if="loading">
          <Skeleton height="2rem" class="mb-2" />
        </div>
        <div v-else>
        <p class="text-3xl font-bold text-primary">{{ totalRoles }}</p>
        </div>
      </template>
    </Card>

    <Card class="bg-white shadow rounded-2xl p-4 hover:cursor-pointer hover:border-1 hover:border-gray-300" @click="() => $router.push('/permissions')">
      <template #title>Permissions</template>
      <template #content>
        <div v-if="loading">
          <Skeleton height="2rem" class="mb-2" />
        </div>
        <div v-else>
          <p class="text-3xl font-bold text-primary">{{ totalPermissions }}</p>
        </div>
      </template>
    </Card>
  </div>

  <!-- Healthcheck Status -->
  <div class="p-4 mt-6">
    <Card class="shadow rounded-2xl">
      <template #title>System Health</template>
      <template #content>
        <div v-if="loading">
          <Skeleton height="2rem" class="mb-2" />
          <Skeleton height="2rem" class="mb-2" />
          <Skeleton height="2rem" />
        </div>

        <div v-else-if="health">
          <p class="text-lg font-medium mb-2">
            Overall Status:
            <span :class="health.status === 'Healthy' ? 'text-green-600' : 'text-red-500'">
              {{ health.status }}
            </span>
          </p>
          <p class="text-lg font-medium mb-2">
            Uptime:
            <span class="text-gray-600">
              {{ formatUptime(health.uptime) }}
            </span>
          </p>
          <ul class="space-y-1">
            <li v-for="(value, key) in health.checks" :key="key">
              <span class="font-medium">{{ key }}: </span>
              <span :class="value === 'OK' ? 'text-green-600' : 'text-red-500 ml-1'">{{ value }}</span>
            </li>
          </ul>
        </div>

        <div v-else class="text-red-500">Failed to load health status.</div>
      </template>
    </Card>
  </div>
</template>

<style scoped>
/* Optional custom styles */
</style>