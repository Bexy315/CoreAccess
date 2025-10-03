<script setup lang="ts">
import {onMounted, ref, watch} from "vue";
import {useConfirm} from "primevue";
import {router} from "../router";
import {useRoute} from "vue-router";
import {showError} from "../utils/toast.ts";
import {deletePermission, getPermissions} from "../services/PermissionService.ts";

// ---- Typen ----
interface PermissionDto {
  id: string;
  name: string;
  description?: string;
}

// ---- Dummy Services ----
async function fetchPermissions(opts: { page: number; pageSize: number; search?: string }) {
  console.log("Fetching permissions...", opts);
  return await getPermissions({
    page: opts.page,
    pageSize: opts.pageSize,
    search: opts.search
  }).then((res => {
    return res;
  })).catch((err => {
    console.error("Error fetching permissions", err);
    showError(err.data, "Error fetching permissions");
  }))
}

const route = useRoute();
const confirm = useConfirm();

// ---- Table State ----
const search = ref<string>(String(route.query.search || ""));
const page = ref<number>(Number(route.query.page || 1));
const pageSize = ref<number>(Number(route.query.pageSize || 10));
const rowsPerPageOptions = ref([5, 10, 20, 50]);

const permissions = ref<PermissionDto[]>([]);
const selectedPermissions = ref<PermissionDto[]>([]);
const totalRecords = ref(0);
const loading = ref(false);

// ---- Data Loading ----
const loadPermissions = async (page = 0, pageSize = 10) => {
  loading.value = true;
  try {
    const response = await fetchPermissions({
      page: page,
      pageSize: pageSize,
      search: search.value || undefined,
    }).then(res => {
      return res;
    });

    if(!response) {
      permissions.value = [];
      totalRecords.value = 0;
      return;
    }
    permissions.value = response.items;
    totalRecords.value = response.totalCount;
  } catch (err) {
    console.error("Failed to load permissions", err);
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
      loadPermissions();
    }
);

onMounted(async () => {
  await loadPermissions(page.value, pageSize.value);
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

// ---- Actions ----
const confirmDelete = () => {
  if (selectedPermissions.value.length !== 1) {
    console.warn("Please select exactly one permission to delete.");
    return;
  }

  confirm.require({
    message: "Do you want to delete this permission?",
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
        await deletePermission(selectedPermissions.value[0].id);
        selectedPermissions.value = [];
        await loadPermissions(page.value - 1, pageSize.value);
      } catch (err: any) {
        console.error("Error deleting permission", err);
        showError(err.data || "Error deleting permission", "Error");
      }
    },
    reject: () => {},
  });
};

function openDetailsDialog(permission: PermissionDto) {
  router.push({
    path: `/permissions/${permission.id}`,
    query: route.query,
  });
}

const addPermissionDialogVisible = ref(false);
function addedPermission() {
  console.log("Permission created successfully.");
  addPermissionDialogVisible.value = false;
  loadPermissions(page.value / pageSize.value, pageSize.value);
}
</script>

<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-4">Permission Management</h1>
    <p class="mb-4">Here you can manage permissions.</p>

    <DataTable
        :value="permissions"
        :lazy="true"
        v-model:selection="selectedPermissions"
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
              <!-- New Permission -->
              <Button
                  label="New"
                  icon="pi pi-plus"
                  @click="addPermissionDialogVisible = true"
                  variant="text"
                  severity="secondary"
              />

              <!-- Delete -->
              <Button
                  v-if="selectedPermissions.length === 1"
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
                    placeholder="Search permissions..."
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
      <AddPermissionDialog
          v-model:visible="addPermissionDialogVisible"
          v-on:submit="addedPermission"
      />
    </div>

    <router-view />
  </div>
  <ConfirmDialog></ConfirmDialog>
</template>

<style scoped></style>
