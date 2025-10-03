<template>
  <Dialog
      v-model:visible="visible"
      modal
      header="Role Details"
      @hide="closeDialog"
      class="w-10/12 md:w-3/4 lg:w-1/2 h-10/12 md:h-3/4 lg:h-2/3"
  >
    <template v-if="loading">
      <ProgressSpinner />
    </template>
    <template v-else>
      <div v-if="role">
        <Tabs v-model:value="activeTab">
          <TabList>
            <Tab value="0">General</Tab>
            <Tab value="1">Permissions</Tab>
            <Tab value="2">Users</Tab>
          </TabList>
          <TabPanels>
            <!-- General Tab -->
            <TabPanel value="0">
              <div class="flex flex-col gap-6">
                <!-- View Mode -->
                <div v-if="!editingGeneral">
                  <div class="grid grid-cols-2 gap-4">
                    <div><strong>ID:</strong> {{ role.id }}</div>
                    <div><strong>Name:</strong> {{ role.name }}</div>
                    <div><strong>Description:</strong> {{ role.description }}</div>
                    <div><strong>Created At:</strong> {{ new Date(role.createdAt).toLocaleString() }} <i>(UTC)</i></div>
                    <div><strong>Updated At:</strong> {{ new Date(role.updatedAt).toLocaleString() }} <i>(UTC)</i></div>
                    <div><strong>System Role:</strong> {{ role.isSystem ? 'Yes' : 'No' }}</div>
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
                      <InputTextarea v-model="generalForm.description" rows="3" class="w-full" />
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

            <!-- Permissions Tab -->
            <TabPanel value="1">
              <div class="flex flex-col gap-6">
                <div>
                  <h3 class="font-semibold mb-3">Assigned Permissions</h3>
                  <DataTable :value="permissions" class="p-datatable-sm" responsiveLayout="scroll">
                    <Column field="name" header="Permission"></Column>
                    <Column header="Actions" bodyStyle="text-align:right">
                      <template #body="slotProps">
                        <Button
                            icon="pi pi-trash"
                            severity="danger"
                            text
                            @click="removePermission(slotProps.data)"
                        />
                      </template>
                    </Column>
                  </DataTable>
                  <p v-if="permissions.length === 0" class="text-gray-500 text-sm mt-2">
                    No permissions assigned yet.
                  </p>
                </div>

                <!-- Add Permission -->
                <div>
                  <h3 class="font-semibold mb-3">Add Permission</h3>
                  <div class="flex gap-2">
                    <AutoComplete
                        v-model="selectedPermission"
                        :suggestions="filteredPermissions"
                        optionLabel="name"
                        placeholder="Search permissions..."
                        @complete="searchPermissions"
                        class="w-full"
                        dropdown
                        dropdown-mode="current"
                    />
                    <Button
                        label="Add"
                        icon="pi pi-plus"
                        :disabled="!selectedPermission"
                        @click="addPermission()"
                    />
                  </div>
                </div>

                <div class="flex justify-end gap-2">
                  <Button
                      label="Cancel"
                      icon="pi pi-times"
                      severity="secondary"
                      :disabled="!dirtyPermissions"
                      @click="resetPermissions"
                  />
                  <Button
                      label="Save Changes"
                      icon="pi pi-save"
                      :disabled="!dirtyPermissions"
                      @click="savePermissions"
                  />
                </div>
              </div>
            </TabPanel>

            <!-- Users Tab -->
            <TabPanel value="2">
              <div class="mb-4 flex justify-between items-center">
                <h3 class="font-semibold">Assigned Users</h3>
                <p class="text-gray-500">Count: {{users.length}}</p>
              </div>
              <div class="flex flex-col gap-6">
                <DataTable :value="users" class="p-datatable-sm" responsiveLayout="scroll">
                  <Column field="username" header="Username"></Column>
                  <Column field="email" header="Email"></Column>
                </DataTable>
                <p v-if="users.length === 0" class="text-gray-500 text-sm mt-2">
                  No users assigned yet.
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
import {assignPermissionToRole, fetchRole, removePermissionFromRole} from "../../../services/RoleService.ts";
import type {RoleDetailDto} from "../../../model/CoreRoleModel.ts";
import type {PermissionDto} from "../../../model/CorePermissionModel.ts";
import type {UserDto} from "../../../model/CoreUserModel.ts";
import {getPermissions} from "../../../services/PermissionService.ts";
import {showError} from "../../../utils/toast.ts";

// --- Dummy model types ---

const route = useRoute();
const router = useRouter();

const visible = ref(true);
const loading = ref(true);
const role = ref<RoleDetailDto | null>(null);

const activeTab = ref<string>((route.query.tab as string) ?? "0");

// --- dummy lists ---
const permissions = ref<PermissionDto[]>([]);
const initialPermissions = ref<PermissionDto[]>([]);
const users = ref<UserDto[]>([]);
const allPermissions = ref<PermissionDto[]>([]);

const selectedPermission = ref<PermissionDto | null>(null);
const filteredPermissions = ref<PermissionDto[]>([]);
const dirtyPermissions = ref(false);

// --- general form ---
const editingGeneral = ref(false);
const generalForm = ref({ name: "", description: "" });
const dirtyGeneral = ref(false);

// --- Lifecycle ---
onMounted(async () => {
  loading.value = true;
  await loadRole().then(() => {
    loading.value = false;
  }).catch(error => {
    showError(error, 'Failed to load role details. Please try again.')
    closeDialog();
  });
});

async function loadRole(){
  role.value = await fetchRole(route.params.id as string, true, true);
  permissions.value = [...role.value.permissions];
  initialPermissions.value = [...role.value.permissions];
  users.value = [...role.value.users];
  generalForm.value = { name: role.value.name, description: role.value.description ?? "" };
  await fetchAllPermissions();
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

async function fetchAllPermissions(){
  await getPermissions({  page: 0, pageSize: 1000, }
  ).then((data) => {
    allPermissions.value = data.items;
  });
}
function saveGeneral() {
  role.value = { ...role.value!, ...generalForm.value, updatedAt: new Date().toISOString() };
  editingGeneral.value = false;
  dirtyGeneral.value = false;
}

function cancelGeneralEdit() {
  generalForm.value = { name: role.value?.name ?? "", description: role.value?.description ?? "" };
  editingGeneral.value = false;
  dirtyGeneral.value = false;
}

// --- Permissions ---
function searchPermissions(event: any) {
  const query = event.query.toLowerCase();
  filteredPermissions.value =
      allPermissions.value?.filter(
          (p) =>
              p.name.toLowerCase().includes(query) &&
              !permissions.value.some((ap) => ap.id === p.id)
      ) ?? [];
}

function addPermission() {
  const permission = selectedPermission.value;
  if (!permission) return;
  permissions.value.push(permission);
  selectedPermission.value = null;
  dirtyPermissions.value = true;
}

function removePermission(permission: PermissionDto) {
  permissions.value = permissions.value.filter((p) => p.id !== permission.id);
  dirtyPermissions.value = true;
}

function resetPermissions() {
  permissions.value = [...(role.value?.permissions || [])];
  dirtyPermissions.value = false;
}

async function savePermissions() {
  const added = permissions.value.filter(
      r => !initialPermissions.value.some(ir => ir.id === r.id)
  )
  const removed = initialPermissions.value.filter(
      ir => !permissions.value.some(r => r.id === ir.id)
  )

  for (const permission of added) {
    await assignPermissionToRole(route.params.id as string, permission.id).catch(error => {
      showError(error, 'Failed to assign role. Please try again.')
    })
  }

  for (const permission of removed) {
    await removePermissionFromRole(route.params.id as string, permission.id).catch(error => {
      showError(error, 'Failed to remove role. Please try again.')
    })
  }

  await loadRole()
  dirtyPermissions.value = false;
}

// --- Close ---
function closeDialog() {
  const { tab, ...rest } = route.query;
  router.push({
    path: "/roles",
    query: rest,
  });
}
</script>
