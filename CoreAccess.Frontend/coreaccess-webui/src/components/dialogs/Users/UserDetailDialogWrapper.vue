<template>
  <Dialog
      v-model:visible="visible"
      modal
      header="User Details"
      @hide="closeDialog"
      class="w-10/12 md:w-3/4 lg:w-1/2 h-10/12 md:h-3/4 lg:h-2/3"
  >
    <template v-if="loading">
      <ProgressSpinner />
    </template>
    <template v-else>
      <div v-if="user">
        <!-- Mit v-model:value binden wir das aktive Tab -->
        <Tabs v-model:value="activeTab">
          <TabList>
            <Tab value="0">General</Tab>
            <Tab value="1">Roles and Permissions</Tab>
            <Tab value="2">Settings</Tab>
          </TabList>
          <TabPanels>
            <TabPanel value="0">
              <div class="flex flex-col gap-6">
                <!-- View Mode -->
                <div v-if="!editingGeneral">
                  <div class="grid grid-cols-2 gap-4">
                    <div><strong>ID:</strong> {{ user.id }}</div>
                    <div><strong>Username:</strong> {{ user.username }}</div>
                    <div><strong>Email:</strong> {{ user.email }}</div>
                    <div><strong>First Name:</strong> {{ user.firstName }}</div>
                    <div><strong>Last Name:</strong> {{ user.lastName }}</div>
                    <div><strong>Created At:</strong> {{ new Date(user.createdAt).toLocaleString() }} <i>(UTC)</i></div>
                    <div><strong>Updated At:</strong> {{ new Date(user.updatedAt).toLocaleString() }} <i>(UTC)</i></div>
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
                      <label class="font-medium">Username</label>
                      <InputText v-model="generalForm.username" class="w-full" />
                    </div>
                    <div>
                      <label class="font-medium">Email</label>
                      <InputText v-model="generalForm.email" class="w-full" />
                    </div>
                    <div>
                      <label class="font-medium">First Name</label>
                      <InputText v-model="generalForm.firstName" class="w-full" />
                    </div>
                    <div>
                      <label class="font-medium">Last Name</label>
                      <InputText v-model="generalForm.lastName" class="w-full" />
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
            <TabPanel value="1">
              <div class="flex flex-col gap-6">
                <!-- Assigned Roles -->
                <div>
                  <h3 class="font-semibold mb-3">Assigned Roles</h3>
                  <DataTable
                      :value="assignedRoles"
                      class="p-datatable-sm"
                      responsiveLayout="scroll"
                  >
                    <Column field="name" header="Role"></Column>
                    <Column header="Actions" bodyStyle="text-align:right">
                      <template #body="slotProps">
                        <Button
                            icon="pi pi-trash"
                            severity="danger"
                            text
                            @click="removeRole(slotProps.data)"
                        />
                      </template>
                    </Column>
                  </DataTable>
                  <p v-if="assignedRoles.length === 0" class="text-gray-500 text-sm mt-2">
                    No roles assigned yet.
                  </p>
                </div>

                <!-- Add Role -->
                <div>
                  <h3 class="font-semibold mb-3">Add Role</h3>
                    <div class="flex gap-2">
                          <AutoComplete
                              v-model="selectedRole"
                              :suggestions="filteredRoles"
                              optionLabel="name"
                              placeholder="Search roles..."
                              @complete="searchRoles"
                              class="w-full"
                              dropdown
                              dropdown-mode="current"
                          />
                          <Button
                              label="Add"
                              icon="pi pi-plus"
                              :disabled="!selectedRole"
                              @click="addRole(selectedRole)"
                          />
                    </div>
                </div>

                <!-- Save/Cancel -->
                <div class="flex justify-end gap-2">
                  <Button
                      label="Cancel"
                      icon="pi pi-times"
                      severity="secondary"
                      :disabled="!dirty"
                      @click="resetRoles"
                  />
                  <Button
                      label="Save Changes"
                      icon="pi pi-save"
                      :disabled="!dirty"
                      @click="saveRoles"
                  />
                </div>
              </div>
            </TabPanel>
            <TabPanel value="2">
              <p class="m-0">
                At vero eos et accusamus et iusto odio dignissimos ...
              </p>
            </TabPanel>
          </TabPanels>
        </Tabs>
      </div>
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {fetchUser, assignRoleToUser, removeRoleFromUser, updateUser} from '../../../services/UserService.ts'
import { type CoreUserUpdateRequest} from '../../../model/CoreUserModel.ts'
import {showError, showSuccess} from '../../../utils/toast.ts'
import type {RoleDto} from "../../../model/CoreRoleModel.ts";
import {getRoles} from "../../../services/RoleService.ts";

const route = useRoute()
const router = useRouter()

const visible = ref(true)
const loading = ref(true)
const user = ref<any | null>(null)

const allRoles = ref<RoleDto[] | null>(null)

const assignedRoles = ref<any[]>([])
const initialRoles = ref<any[]>([]) // für Vergleich + Reset
const selectedRole = ref<any | null>(null)
const filteredRoles = ref<any[]>([])
const dirty = ref(false)

const activeTab = ref<string>((route.query.tab as string) ?? '0')

const loadUser = async () => {
  loading.value = true
  user.value = await fetchUser(route.params.id as string)
  loading.value = false

  initialRoles.value = [...(user.value?.roles || [])]
  assignedRoles.value = [...(user.value?.roles || [])]
  dirty.value = false
}

onMounted(loadUser)
onMounted(loadRoles)

function loadRoles() {
  getRoles({ page: 1, pageSize: 1000 }).then(roles => {
    allRoles.value = roles.items
  }).catch(error => {
    showError(error, 'Failed to load roles. Role management might not work properly.')
  })
}

watch(() => route.params.id, loadUser)

// Synchronisation: wenn activeTab sich ändert → Query aktualisieren
watch(activeTab, (val) => {
  if (route.query.tab === val) return
  router.replace({
    path: route.path,
    query: {
      ...route.query,
      tab: val
    }
  }).catch(() => {})
})

// Synchronisation: wenn sich Query.tab ändert
watch(
    () => route.query.tab,
    (val) => {
      if (val != null && val !== activeTab.value) {
        activeTab.value = val as string
      }
    }
)

// Rollenverwaltung
function searchRoles(event: any) {
  const query = event.query.toLowerCase()
  filteredRoles.value = allRoles.value?.filter(
      r =>
          r.name.toLowerCase().includes(query) &&
          !assignedRoles.value.some(ar => ar.id === r.id)
  ) ?? []
}

function addRole(role: any) {
  if (!role) return
  assignedRoles.value.push(role)
  selectedRole.value = null
  dirty.value = true
}

function removeRole(role: any) {
  assignedRoles.value = assignedRoles.value.filter(r => r.id !== role.id)
  dirty.value = true
}

function resetRoles() {
  assignedRoles.value = [...initialRoles.value]
  dirty.value = false
}

async function saveRoles() {
  const added = assignedRoles.value.filter(
      r => !initialRoles.value.some(ir => ir.id === r.id)
  )
  const removed = initialRoles.value.filter(
      ir => !assignedRoles.value.some(r => r.id === ir.id)
  )

  for (const role of added) {
    await assignRoleToUser(user.value.id, role.id).catch(error => {
      showError(error, 'Failed to assign role. Please try again.')
    })
  }

  for (const role of removed) {
    await removeRoleFromUser(user.value.id, role.id).catch(error => {
      showError(error, 'Failed to remove role. Please try again.')
    })
  }

  await loadUser()
}

// General Tab Edit-State
const editingGeneral = ref(false)
const generalForm = ref({
  username: '',
  email: '',
  firstName: '',
  lastName: '',
} as CoreUserUpdateRequest)
const dirtyGeneral = ref(false)

const resetGeneralForm = () => {
  generalForm.value = {
    username: user.value?.username ?? '',
    email: user.value?.email ?? '',
    firstName: user.value?.firstName ?? '',
    lastName: user.value?.lastName ?? '',
  }
  editingGeneral.value = false
  dirtyGeneral.value = false
}

watch(generalForm, () => {
  dirtyGeneral.value = true
}, { deep: true })

const saveGeneral = async () => {
  await updateUser(user.value.id, generalForm.value).catch(e => {
    showError(e, 'Failed to update user details. Please try again.')
  })
  user.value = { ...user.value, ...generalForm.value }
  showSuccess('User details updated successfully.')
  resetGeneralForm()
}

const cancelGeneralEdit = () => {
  resetGeneralForm()
}

// beim Laden auch Formular initialisieren
watch(user, () => {
  if (user.value) resetGeneralForm()
})


const closeDialog = () => {
  const { tab, ...rest } = route.query
  router.push({
    path: '/users',
    query: rest
  })
}
</script>

