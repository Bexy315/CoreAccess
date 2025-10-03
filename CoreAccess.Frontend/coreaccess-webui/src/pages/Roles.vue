<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import { useConfirm } from "primevue";
import { router } from "../router";
import { useRoute } from "vue-router";
import {deleteRole, getRoles} from "../services/RoleService.ts";
import {showError} from "../utils/toast.ts";

// RoleDto model
type RoleDto = {
  id: string;
  name: string;
  description?: string;
};

const route = useRoute();

const search = ref<string>(String(route.query.search || ""));
const page = ref<number>(Number(route.query.page || 1));
const pageSize = ref<number>(Number(route.query.pageSize || 10));

const roles = ref<RoleDto[]>([]);
const selectedRoles = ref<RoleDto[]>([]);
const rowsPerPageOptions = ref([5, 10, 20, 50]);
const totalRecords = ref(0);
const loading = ref(false);
const confirm = useConfirm();

// --- Dummy Service ---
async function fetchRoles(opts: { page: number; pageSize: number; search?: string }) {
  return await getRoles({
    page: opts.page,
    pageSize: opts.pageSize,
    search: opts.search,
  });
}
// --- Load data ---
const loadRoles = async (page = 0, pageSize = 10) => {
  loading.value = true;
  try {
    const response = await fetchRoles({
      page: page,
      pageSize: pageSize,
      search: search.value || undefined,
    });
    roles.value = response.items;
    totalRecords.value = response.totalCount;
  } catch (err) {
    console.error("Failed to load roles", err);
  } finally {
    loading.value = false;
  }
};

watch(
    () => route.query,
    (q) => {
      search.value = String(q.search || "");
      page.value = Number(q.page || 1);
      pageSize.value = Number(q.pageSize || 10);
      loadRoles();
    }
);

onMounted(async () => {
  await loadRoles(page.value, pageSize.value);
});

function updateQuery(params: Record<string, any> = {}) {
  router.replace({
    query: {
      search: search.value || undefined,
      page: page.value.toString(),
      pageSize: pageSize.value.toString(),
      ...params,
    },
  });
}

function onSearchChange() {
  page.value = 1;
  updateQuery({});
}

/** function onPageChange(event: any) {
  page.value = event.page + 1;
  pageSize.value = event.rows;
  updateQuery({});
} **/

const confirmDelete = () => {
  if (selectedRoles.value.length !== 1) {
    console.warn("Please select exactly one role to delete.");
    return;
  }

  confirm.require({
    message: "Do you want to delete this role?",
    header: "Danger Zone",
    icon: "pi pi-info-circle",
    rejectLabel: "Cancel",
    rejectProps: {
      label: "Cancel",
      severity: "secondary",
      outlined: true,
    },
    acceptProps: {
      label: "Delete",
      severity: "danger",
    },
    accept: async () => {
      try {
        await deleteRole(selectedRoles.value[0].id);
        selectedRoles.value = [];
        await loadRoles(page.value - 1, pageSize.value);
      } catch (err: any) {
        showError(err.response.data, "Failed to delete role");
        console.error("Error deleting role", err);
      }
    },
    reject: () => {
    },
  });
};

function openDetailsDialog(role: RoleDto) {
  router.push({
    path: `/roles/${role.id}`,
    query: route.query,
  });
}

const addRoleDialogVisible = ref(false);
function addedRole(){
  addRoleDialogVisible.value = false;
  loadRoles(page.value / pageSize.value, pageSize.value);
}
</script>

<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-4">Role Management</h1>
    <p class="mb-4">Here you can manage roles.</p>

    <DataTable
        :value="roles"
        :lazy="true"
        v-model:selection="selectedRoles"
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
        <Toolbar class="!bg-white">
          <template #start>
            <div class="flex gap-2">
              <!-- New Role -->
              <Button
                  label="New"
                  icon="pi pi-plus"
                  @click="addRoleDialogVisible = true"
                  variant="text"
                  severity="secondary"
              />

              <!-- Delete -->
              <Button
                  v-if="selectedRoles.length === 1"
                  label="Delete"
                  icon="pi pi-trash"
                  severity="danger"
                  @click="confirmDelete"
              />
            </div>
          </template>

          <template #end>
            <div class="flex gap-2 items-center">
              <!-- Search -->
              <span class="p-input-icon-left">
                <InputText
                    v-model="search"
                    placeholder="Search roles..."
                    @input="onSearchChange"
                />
              </span>
            </div>
          </template>
        </Toolbar>
      </template>

      <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
      <Column field="name" header="Name" />
      <Column field="description" header="Description" />
      <Column class="w-24 !text-end" header="Actions">
        <template #body="{ data }">
          <Button
              icon="pi pi-search"
              @click="openDetailsDialog(data)"
              severity="secondary"
              rounded
          />
        </template>
      </Column>
    </DataTable>


    <div class="flex justify-center items-center">
      <AddRoleDialog
          v-model:visible="addRoleDialogVisible"
          v-on:submit="addedRole"
      />
    </div>
    <router-view />
  </div>
  <ConfirmDialog></ConfirmDialog>
</template>

<style scoped></style>
