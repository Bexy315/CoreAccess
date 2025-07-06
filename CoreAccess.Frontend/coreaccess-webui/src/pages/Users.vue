<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { fetchUsers } from '../services/UserService';
import type { CoreUserDto } from "../model/CoreUserModel.ts";
import { CoreUserStatus } from "../model/CoreUserModel.ts";
import AddUserDialog from "../components/dialogs/AddUserDialog.vue";

const users = ref<CoreUserDto[]>([]);
const selectedUsers = ref<CoreUserDto[]>([]);
const rowsPerPageOptions = ref([5, 10, 20, 50]);
const rows = ref(10);
const first = ref(0)
const totalRecords = ref(0);
const loading = ref(false);

const addUserDialogVisible = ref(false);

const menuItems = ref([
  {
    label: 'New',
    icon: 'pi pi-plus',
    command: () => {
      addUserDialogVisible.value = true;
    }
  },
  {
    label: 'Delete',
    icon: 'pi pi-trash',
    command: () => {
      console.log("Print command executed");
    }
  }
]);

onMounted(async () => {
  await loadUsers(0, rows.value);
});

const loadUsers = async (page = 0, pageSize = 10) => {
  loading.value = true
  try {
    const response = await fetchUsers({
      page: page + 1,
      pageSize: pageSize
    })
    users.value = response.items
    totalRecords.value = response.totalCount
  } catch (err) {
    console.error('Failed to load users', err)
  } finally {
    loading.value = false
  }
}

const onPage = (event: any) => {
  first.value = event.first
  rows.value = event.rows
  const page = event.page
  loadUsers(page, event.rows)
}

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

function addedUser(){
  addUserDialogVisible.value = false;
  loadUsers(first.value / rows.value, rows.value);
}
</script>

<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-4">Benutzerverwaltung</h1>
    <p class="mb-4">Hier können Sie Benutzer verwalten.</p>

    <DataTable :value="users" :lazy="true" v-model:selection="selectedUsers" :first="first" @page="onPage" :totalRecords="totalRecords" paginator :rows="rows" :rowsPerPageOptions="rowsPerPageOptions" :loading="loading" stripedRows responsiveLayout="scroll">
      <template #header>
        <Menubar :model="menuItems" class="!bg-white"></Menubar>
      </template>
      <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
      <Column field="username" header="Benutzername" />
      <Column header="Name" >
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

    <div class="flex justify-center items-center">
      <AddUserDialog
          v-model:visible="addUserDialogVisible"
          v-on:submit="addedUser"
      />
    </div>
  </div>
</template>

<style scoped></style>