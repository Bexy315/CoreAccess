<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { fetchUsers } from '../services/UserService';
import type { CoreUserDto } from "../model/CoreUserModel.ts";
import { CoreUserStatus } from "../model/CoreUserModel.ts";

const users = ref<CoreUserDto[]>([]);
const loading = ref(false);

onMounted(async () => {
  loading.value = true;
  try {
    const result = await fetchUsers({ page: 1, pageSize: 20 });
    users.value = result.items;
  } finally {
    loading.value = false;
  }
});

function formatStatus(status: CoreUserStatus): string {
  switch (status) {
    case CoreUserStatus.Active:
      return 'Aktiv';
    case CoreUserStatus.Inactive:
      return 'Inaktiv';
    case CoreUserStatus.Locked:
      return 'Gesperrt';
    default:
      return 'Unbekannt';
  }
}
</script>

<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-4">Benutzerverwaltung</h1>
    <p class="mb-4">Hier können Sie Benutzer verwalten.</p>

    <DataTable :value="users" :loading="loading" stripedRows responsiveLayout="scroll">
      <Column field="username" header="Benutzername" />
      <Column header="Name">
        <template #body="{ data }">
          {{ data.firstName }} {{ data.lastName }}
        </template>
      </Column>
      <Column field="email" header="E-Mail" />
      <Column header="Status">
        <template #body="{ data }">
          {{ formatStatus(data.status) }}
        </template>
      </Column>
      <Column header="Rollen">
        <template #body="{ data }">
          <div class="flex flex-wrap gap-2">
            <Tag v-for="role in data.roles" :key="role.id" :value="role.name" severity="info" />
          </div>
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<style scoped></style>