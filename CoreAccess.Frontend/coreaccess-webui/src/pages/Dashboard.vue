<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { getHealthStatus } from '../services/DashboardService.ts';

interface HealthStatus {
  status: string;
  uptime: string;
  checks: Record<string, string>;
}

const health = ref<HealthStatus | null>(null);
const loading = ref(true);

onMounted(async () => {
  try {
    const result = await getHealthStatus();
    health.value = {
      status: result.status,
      uptime: result.uptime,
      checks: result.checks,
    };
  } catch {
    health.value = null;
  } finally {
    loading.value = false;
  }
});

function formatUptime(uptime: string): string {
  const seconds = parseInt(uptime.replace('s', ''), 10);
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
const totalUsers = 128;
const totalRoles = 12;
const activeSessions = 37;
const failedLoginsToday = 3;
</script>

<template>
  <div class="p-2">
    <h1 class="text-2xl font-bold text-primary mb-2">Willkommen!</h1>
  </div>

  <div class="p-4 grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
    <!-- Quick Stats -->
    <Card class="bg-white shadow rounded-2xl p-4">
      <template #title>Total Users</template>
      <template #content>
        <p class="text-3xl font-bold text-primary">{{ totalUsers }}</p>
      </template>
    </Card>

    <Card class="bg-white shadow rounded-2xl p-4">
      <template #title>Roles</template>
      <template #content>
        <p class="text-3xl font-bold text-primary">{{ totalRoles }}</p>
      </template>
    </Card>

    <Card class="bg-white shadow rounded-2xl p-4">
      <template #title>Active Sessions</template>
      <template #content>
        <p class="text-3xl font-bold text-green-600">{{ activeSessions }}</p>
      </template>
    </Card>

    <Card class="bg-white shadow rounded-2xl p-4">
      <template #title>Failed Logins (Today)</template>
      <template #content>
        <p class="text-3xl font-bold text-red-500">{{ failedLoginsToday }}</p>
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