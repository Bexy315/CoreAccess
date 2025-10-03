<template>
  <Dialog
      v-model:visible="visible"
      modal
      header="Permission Details"
      @hide="closeDialog"
      class="w-10/12 md:w-3/4 lg:w-1/2 h-10/12 md:h-3/4 lg:h-2/3"
  >
    <template v-if="loading">
      <ProgressSpinner />
    </template>
    <template v-else>
      <div v-if="permission">
        <Tabs v-model:value="activeTab">
          <TabList>
            <Tab value="0">General</Tab>
            <Tab value="1">Roles</Tab>
          </TabList>

          <TabPanels>
            <!-- General Tab -->
            <TabPanel value="0">
              <div class="flex flex-col gap-6">
                <!-- View Mode -->
                <div v-if="!editingGeneral">
                  <div class="grid grid-cols-2 gap-4">
                    <div><strong>ID:</strong> {{ permission.id }}</div>
                    <div><strong>Name:</strong> {{ permission.name }}</div>
                    <div><strong>Description:</strong> {{ permission.description }}</div>
                    <div><strong>Created At:</strong> {{ new Date(permission.createdAt).toLocaleString() }} <i>(UTC)</i></div>
                    <div><strong>Updated At:</strong> {{ new Date(permission.updatedAt).toLocaleString() }} <i>(UTC)</i></div>
                    <div><strong>System Permission:</strong> {{ permission.isSystem ? 'Yes' : 'No' }}</div>
                  </div>
                  <div class="flex justify-end mt-4">
                    <Button
                        label="Edit"
                        icon="pi pi-pencil"
                        @click="editingGeneral = true"
                    />
                  </div>
                </div>

                <!-- Edit Mode -->
                <div v-else>
                  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label class="font-medium">Name</label>
                      <InputText v-model="generalForm.name" class="w-full" />
                    </div>
                    <div class="md:col-span-2">
                      <label class="font-medium">Description</label>
                      <InputText v-model="generalForm.description" rows="3" class="w-full" />
                    </div>
                  </div>

                  <!-- Save/Cancel -->
                  <div class="flex justify-end gap-2 mt-6">
                    <Button
                        label="Cancel"
                        icon="pi pi-times"
                        severity="secondary"
                        @click="cancelGeneralEdit"
                    />
                    <Button
                        label="Save Changes"
                        icon="pi pi-save"
                        :disabled="!dirtyGeneral"
                        @click="saveGeneral"
                    />
                  </div>
                </div>
              </div>
            </TabPanel>

            <!-- Roles Tab -->
            <TabPanel value="1">
              <div class="mb-4 flex justify-between items-center">
                <h3 class="font-semibold">Assigned Roles</h3>
                <p class="text-gray-500">Count: {{ roles.length }}</p>
              </div>
              <div class="flex flex-col gap-6">
                <DataTable :value="roles" class="p-datatable-sm" responsiveLayout="scroll">
                  <Column field="name" header="Role Name"></Column>
                  <Column field="description" header="Description"></Column>
                </DataTable>
                <p v-if="roles.length === 0" class="text-gray-500 text-sm mt-2">
                  No roles assigned yet.
                </p>
              </div>
            </TabPanel>
          </TabPanels>
        </Tabs>
      </div>
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import type { PermissionDetailDto } from "../../../model/CorePermissionModel.ts";
import type { RoleDto } from "../../../model/CoreRoleModel.ts";
import { fetchPermission, updatePermission } from "../../../services/PermissionService.ts";
import { showError } from "../../../utils/toast.ts";

// --- setup ---
const route = useRoute();
const router = useRouter();

const visible = ref(true);
const loading = ref(true);
const permission = ref<PermissionDetailDto | null>(null);

const activeTab = ref<string>((route.query.tab as string) ?? "0");

// --- dummy lists ---
const roles = ref<RoleDto[]>([]);

// --- general form ---
const editingGeneral = ref(false);
const generalForm = ref({ name: "", description: "" });
const dirtyGeneral = ref(false);

// --- Lifecycle ---
onMounted(async () => {
  loading.value = true;
  await loadPermission().then(() => {
    loading.value = false;
  }).catch(error => {
    showError(error, 'Failed to load permission details. Please try again.')
    closeDialog();
  });
});

async function loadPermission() {
  permission.value = await fetchPermission(route.params.id as string, true);

  if(permission.value == null) {
    throw new Error("Permission not found");
  }

  roles.value = [...permission.value.roles];
  generalForm.value = { name: permission.value.name, description: permission.value.description ?? "" };
}

// --- Tab sync ---
watch(activeTab, (val) => {
  if (route.query.tab === val) return;
  router.replace({
    path: route.path,
    query: { ...route.query, tab: val },
  }).catch(() => {});
});
watch(
    () => route.query.tab,
    (val) => {
      if (val != null && val !== activeTab.value) {
        activeTab.value = val as string;
      }
    }
);

// --- General Edit ---
watch(generalForm, () => {
  dirtyGeneral.value = true;
}, { deep: true });

async function saveGeneral() {
  permission.value = await updatePermission(route.params.id as string, generalForm.value.name, generalForm.value.description).catch(error => {
    showError(error, 'Failed to update permission. Please try again.')
    return permission.value;
  });

  editingGeneral.value = false;
  dirtyGeneral.value = false;
}

function cancelGeneralEdit() {
  generalForm.value = { name: permission.value?.name ?? "", description: permission.value?.description ?? "" };
  editingGeneral.value = false;
  dirtyGeneral.value = false;
}

// --- Close ---
function closeDialog() {
  const { tab, ...rest } = route.query;
  router.push({
    path: "/permissions",
    query: rest,
  });
}
</script>
