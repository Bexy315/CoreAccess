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
        <Tabs value="0">
          <TabList>
            <Tab value="0">General</Tab>
            <Tab value="1">Roles and Permissions</Tab>
            <Tab value="2">Settings</Tab>
          </TabList>
          <TabPanels>
            <TabPanel value="0">
              <p><strong>ID:</strong> {{ user.id }}</p>
              <p><strong>Username:</strong> {{ user.username }}</p>
              <p><strong>Email:</strong> {{ user.email }}</p>
              <p><strong>Full Name:</strong> {{ user.firstName }} {{ user.lastName }}</p>
              <p><strong>Status:</strong> {{ formatStatus(user.status) }}</p>
              <p><strong>Created At:</strong> {{ new Date(user.createdAt).toLocaleString() }} <i>(UTC)</i></p>
              <p><strong>Updated At:</strong> {{ new Date(user.updatedAt).toLocaleString() }} <i>(UTC)</i></p>
            </TabPanel>
            <TabPanel value="1">
              <div class="flex flex-col gap-4">
                <div>
                  <h3 class="font-semibold mb-2">Assigned Roles</h3>
                  <div class="flex flex-wrap gap-2">
                    <Chip
                        v-for="role in assignedRoles"
                        :key="role.id"
                        :label="role.name"
                        removable
                        @remove="removeRole(role)"
                        :class="{'bg-green-100': role._new}"
                    />
                  </div>
                </div>

                <div>
                  <h3 class="font-semibold mb-2">Add Role</h3>
                  <AutoComplete
                      v-model="selectedRole"
                      :suggestions="filteredRoles"
                      optionLabel="name"
                      placeholder="Search roles..."
                      @complete="searchRoles"
                      @item-select="addRole"
                  />
                </div>

                <div class="flex justify-end">
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
                At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident, similique sunt in culpa
                qui officia deserunt mollitia animi, id est laborum et dolorum fuga. Et harum quidem rerum facilis est et expedita distinctio. Nam libero tempore, cum soluta nobis est eligendi optio cumque nihil impedit quo minus.
              </p>
            </TabPanel>
          </TabPanels>
        </Tabs>
      </div>
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {fetchUser} from "../../services/UserService.ts";
import {CoreUserStatus} from "../../model/CoreUserModel.ts";

const route = useRoute()
const router = useRouter()

const visible = ref(true)
const loading = ref(true)
const user = ref<any | null>(null)

const allRoles = ref([
  { id: '7861f005-9a1c-4083-9bcd-2204aa9c9736', name: 'CoreAccess.Admin' },
  { id: '9812f774-eb7e-2c44-8b1f-943afcaa4459', name: 'User' },
  { id: 'viewer', name: 'Viewer' },
  { id: 'auditor', name: 'Auditor' },
])

const assignedRoles = ref<any[]>([])

const selectedRole = ref<any | null>(null)
const filteredRoles = ref<any[]>([])

// Flag ob Änderungen ungespeichert sind
const dirty = ref(false)

// Suche im Autocomplete
function searchRoles(event: any) {
  const query = event.query.toLowerCase()
  filteredRoles.value = allRoles.value.filter(
      r =>
          r.name.toLowerCase().includes(query) &&
          !assignedRoles.value.some(ar => ar.id === r.id)
  )
}

// Rolle hinzufügen (markiert als neu)
function addRole(event: any) {
  assignedRoles.value.push({ ...event.value, _new: true })
  selectedRole.value = null
  dirty.value = true
}

// Rolle entfernen
function removeRole(role: any) {
  assignedRoles.value = assignedRoles.value.filter(r => r.id !== role.id)
  dirty.value = true
}

watch(user, (u) => {
  if (!u) return
  assignedRoles.value = u.roles || []
})

const loadUser = async () => {
  loading.value = true
  user.value = await fetchUser(route.params.id as string)
  loading.value = false
}

onMounted(loadUser)

watch(() => route.params.id, loadUser)

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

const saveRoles = async () => {
  console.log('Saving roles for user', user.value.id)
  // später: await updateUserRoles(user.value.id, roleIds)
}

const closeDialog = () => {
  router.push({ path: '/users', query: route.query })
}
</script>