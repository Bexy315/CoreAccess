<template>
  <Dialog
      v-model:visible="visible"
      modal
      header="User Details"
      @hide="closeDialog"
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
                <PickList
                    v-model="picklistValue"
                    dataKey="id"
                    listStyle="height:300px"
                >
                  <template #sourceheader>Available Roles</template>
                  <template #targetheader>Assigned Roles</template>

                  <template #item="slotProps">
                    <div class="flex items-center">
                      <i class="pi pi-users mr-2"></i>
                      <span>{{ slotProps.item.name }}</span>
                    </div>
                  </template>
                </PickList>

                <div class="flex justify-end gap-2">
                  <Button label="Cancel" severity="secondary" @click="closeDialog" />
                  <Button label="Save" icon="pi pi-save" @click="saveRoles" />
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
import { ref, onMounted, watch, computed } from 'vue'
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

const availableRoles = computed(() => {
  if (!user.value) return allRoles.value
  return allRoles.value.filter(
      r => !assignedRoles.value.some((ar: any) => ar.id === r.id)
  )
})

const picklistValue = ref<[any[], any[]]>([[], []])

watch(user, (u) => {
  if (!u) return
  assignedRoles.value = u.roles || []
  picklistValue.value = [availableRoles.value, assignedRoles.value]
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
  const roleIds = picklistValue.value[1].map(r => r.id)
  console.log('Saving roles for user', user.value.id, roleIds)
  // später: await updateUserRoles(user.value.id, roleIds)
}

const closeDialog = () => {
  router.push({ path: '/users', query: route.query })
}
</script>