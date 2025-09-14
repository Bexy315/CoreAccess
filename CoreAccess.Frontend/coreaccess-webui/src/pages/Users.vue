<script setup lang="ts">
import {onMounted, ref, watch} from 'vue';
import {deleteUser, fetchUsers} from '../services/UserService';
import type { CoreUserDto } from "../model/CoreUserModel.ts";
import { CoreUserStatus } from "../model/CoreUserModel.ts";
import AddUserDialog from "../components/dialogs/AddUserDialog.vue";
import {showError, showSuccess} from "../utils/toast.ts";
import {useConfirm} from "primevue";
import {router} from "../router";
import {useRoute} from "vue-router";

const route = useRoute()

const search = ref<string>(String(route.query.search || ''))
const statuses = ref<string[]>(Array.isArray(route.query.status) ? route.query.status as string[] : route.query.status ? [String(route.query.status)] : [])
const page = ref<number>(Number(route.query.page || 1))
const pageSize = ref<number>(Number(route.query.pageSize || 10))

const users = ref<CoreUserDto[]>([]);
const selectedUsers = ref<CoreUserDto[]>([]);
const rowsPerPageOptions = ref([5, 10, 20, 50]);
const rows = ref(10);
const first = ref(0)
const totalRecords = ref(0);
const loading = ref(false);
const confirm = useConfirm();

const addUserDialogVisible = ref(false);

const loadUsers = async (page = 0, pageSize = 10) => {
  loading.value = true
  try {
    const response = await fetchUsers({
      page: page,
      pageSize: pageSize,
      search: search.value || undefined,
      status: statuses.value
    })
    users.value = response.items
    totalRecords.value = response.totalCount
  } catch (err) {
    console.error('Failed to load users', err)
  } finally {
    loading.value = false
  }
}

const menuDeleteItem = ref({
  label: 'Delete',
  icon: 'pi pi-trash',
  command: async () => {
    confirmDelete();
  }
})

const menuItems = ref([
  {
    label: 'New',
    icon: 'pi pi-plus',
    command: () => {
      addUserDialogVisible.value = true;
    }
  }
]);

watch(
    () => route.query,
    (q) => {
      search.value = String(q.search || '')
      page.value = Number(q.page || 1)
      pageSize.value = Number(q.pageSize || 10)

      if (Array.isArray(q.status)) {
        statuses.value = q.status as string[]
      } else if (q.status) {
        statuses.value = [String(q.status)]
      } else {
        statuses.value = []
      }

      loadUsers()
    }
)

watch(selectedUsers, () => {
  if(selectedUsers.value.length == 1) {
    menuItems.value.push(menuDeleteItem.value);
  } else {
    if(menuItems.value.includes(menuDeleteItem.value)) {
      menuItems.value = menuItems.value.filter(item => (item !== menuDeleteItem.value));
    }
  }
});

const deleteSelectedUsers = async () => {
  if (selectedUsers.value.length === 0) {
    console.warn('No users selected for deletion');
    return;
  }

  if(selectedUsers.value.length > 1) {
    console.warn('Multiple users selected, please delete only one at a time');
    return;
  }

  try {
    await deleteUser(selectedUsers.value[0].id);
    selectedUsers.value = [];
    await loadUsers(first.value / rows.value, rows.value);
    showSuccess('Benutzer erfolgreich gelöscht');
  } catch (error) {
    console.error('Failed to delete users', error);
    showError('Fehler beim Löschen des Benutzers. Bitte versuchen Sie es später erneut.');
  }
}

onMounted(async () => {
  await loadUsers(page.value, pageSize.value);
});

function updateQuery(params: Record<string, any> = {}) {
  router.replace({
    query: {
      search: search.value || undefined,
      status: statuses.value.length > 0 ? statuses.value : undefined,
      page: page.value.toString(),
      pageSize: pageSize.value.toString(),
      ...params
    }
  })
}

function onSearchChange() {
  page.value = 1
  updateQuery({})
}

function onStatusChange() {
  page.value = 1
  updateQuery({})
}

function onPageChange(event: any) {
  page.value = event.page + 1
  pageSize.value = event.rows
  updateQuery({})
}

const statusOptions = [
  { label: 'Aktiv', value: String(CoreUserStatus.Active) },
  { label: 'Inaktiv', value: String(CoreUserStatus.Inactive) },
  { label: 'Gesperrt', value: String(CoreUserStatus.Suspended) }
]

function formatStatus(status: CoreUserStatus): string {
  switch (status) {
    case CoreUserStatus.Active:
      return 'Aktiv';
    case CoreUserStatus.Inactive:
      return 'Inaktiv';
    case CoreUserStatus.Suspended:
      return 'Gesperrt';
    default:
      return 'Unbekannt';
  }
}

function openDetailsDialog(user: CoreUserDto) {
  router.push({
    path: `/users/${user.id}`,
    query: route.query
  })
}

function addedUser(){
  addUserDialogVisible.value = false;
  loadUsers(first.value / rows.value, rows.value);
}

const confirmDelete = () => {
  confirm.require({
    message: 'Do you want to delete this record?',
    header: 'Danger Zone',
    icon: 'pi pi-info-circle',
    rejectLabel: 'Cancel',
    rejectProps: {
      label: 'Cancel',
      severity: 'secondary',
      outlined: true
    },
    acceptProps: {
      label: 'Delete',
      severity: 'danger'
    },
    accept: () => {
      deleteSelectedUsers();
    },
    reject: () => {
      console.log("rejected");
    }
  });
};
</script>

<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-4">User Management</h1>
    <p class="mb-4">Here you can manage users.</p>

    <DataTable :value="users"
               :lazy="true"
               v-model:selection="selectedUsers"
               :first="first"
               @page="onPageChange"
               :totalRecords="totalRecords"
               paginator
               :rows="pageSize"
               :rowsPerPageOptions="rowsPerPageOptions"
               :loading="loading"
               stripedRows
               responsiveLayout="scroll"
               removableSort
    >

      <template #header>
        <Menubar :model="menuItems" class="!bg-white">
          <template #start>
            <span class="font-semibold">Users</span>
          </template>

          <template #end>
            <div class="flex gap-2 items-center">
              <span class="p-input-icon-left">
          <InputText
              v-model="search"
              placeholder="Search users..."
              @input="onSearchChange"
          />
        </span>

              <!-- Status Filter -->
              <MultiSelect
                  v-model="statuses"
                  :options="statusOptions"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Filter status"
                  display="chip"
                  class="w-56"
                  @change="onStatusChange"
              />
            </div>
          </template>
        </Menubar>
      </template>
      <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
      <Column field="username" header="Username" />
      <Column header="Name" >
        <template #body="{ data }">
          {{ data.firstName }} {{ data.lastName }}
        </template>
      </Column>
      <Column field="email" header="Email" />
      <Column header="Status">
        <template #body="{ data }">
          {{ formatStatus(data.status) }}
        </template>
      </Column>
      <Column class="w-24 !text-end" header="Actions">
        <template #body="{ data }">
          <Button icon="pi pi-search" @click="openDetailsDialog(data)" severity="secondary" rounded></Button>
        </template>
      </Column>
    </DataTable>

    <div class="flex justify-center items-center">
      <AddUserDialog
          v-model:visible="addUserDialogVisible"
          v-on:submit="addedUser"
      />
    </div>
    <router-view />
  </div>
  <ConfirmDialog></ConfirmDialog>
</template>

<style scoped></style>