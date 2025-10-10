<template>
  <Dialog
      v-model:visible="visible"
      modal
      header="Create New Application"
      @hide="closeDialog"
      class="w-10/12 md:w-3/4 lg:w-1/2 h-10/12 md:h-3/4 lg:h-2/3"
  >
    <div class="flex flex-col gap-6">
      <Tabs v-model:value="activeTab">
        <TabList>
          <Tab value="0">General</Tab>
          <Tab value="1">Redirect URIs</Tab>
          <Tab value="2">Client Permissions</Tab>
        </TabList>

        <TabPanels>
          <!-- General -->
          <TabPanel value="0">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label class="font-medium">Name</label>
                <InputText v-model="newApplication.displayName" class="w-full" />
              </div>
              <div>
                <label class="font-medium">Client ID</label>
                <InputText v-model="newApplication.clientId" class="w-full" />
              </div>
              <div>
                <label class="font-medium">Client Secret</label>
                <InputText v-model="newApplication.clientSecret" class="w-full" />
              </div>
              <div>
                <label class="font-medium">Application Type</label>
                <InputText v-model="newApplication.applicationType" class="w-full" />
              </div>
              <div>
                <label class="font-medium">Client Type</label>
                <InputText v-model="newApplication.clientType" class="w-full" />
              </div>
              <div>
                <label class="font-medium">Consent Type</label>
                <InputText v-model="newApplication.consentType" class="w-full" />
              </div>
            </div>
          </TabPanel>

          <!-- Redirect URIs -->
          <TabPanel value="1">
            <div class="flex flex-col gap-2">
              <div class="flex gap-2">
                <InputText v-model="newRedirectUri" placeholder="Enter redirect URI" class="flex-grow" />
                <Button label="Add" icon="pi pi-plus" :disabled="!newRedirectUri" @click="addRedirectUri" />
              </div>
              <DataTable :value="redirectUris">
                <Column header="URI">
                  <template #body="{ data }">
                    {{ data.value }}
                  </template>
                </Column>
                <Column header="Actions">
                  <template #body="{ data }">
                    <Button icon="pi pi-trash" severity="danger" rounded @click="removeRedirectUri(data)" />
                  </template>
                </Column>
              </DataTable>
            </div>

            <div class="flex flex-col gap-2 mt-4">
              <label class="font-medium">Post-Logout Redirect URIs</label>
              <div class="flex gap-2">
                <InputText v-model="newPostLogoutRedirectUri" placeholder="Enter URI" class="flex-grow" />
                <Button label="Add" icon="pi pi-plus" :disabled="!newPostLogoutRedirectUri" @click="addPostLogoutRedirectUri" />
              </div>
              <DataTable :value="postLogoutRedirectUris">
                <Column header="URI">
                  <template #body="{ data }">
                    {{ data.value }}
                  </template>
                </Column>
                <Column header="Actions">
                  <template #body="{ data }">
                    <Button icon="pi pi-trash" severity="danger" rounded @click="removePostLogoutRedirectUri(data)" />
                  </template>
                </Column>
              </DataTable>
            </div>
          </TabPanel>

          <!-- Permissions -->
          <TabPanel value="2">
            <div class="flex flex-col gap-2">
              <div class="flex gap-2">
                <AutoComplete
                    v-model="selectedPermission"
                    :suggestions="filteredPermissions"
                    optionLabel="label"
                    placeholder="Search permissions..."
                    @complete="searchPermissions"
                    dropdown
                    class="flex-grow"
                />
                <Button label="Add" icon="pi pi-plus" :disabled="!selectedPermission" @click="addPermission" />
              </div>
              <DataTable :value="permissions">
                <Column header="Permission">
                  <template #body="{ data }">{{ data.value }}</template>
                </Column>
                <Column header="Actions">
                  <template #body="{ data }">
                    <Button icon="pi pi-trash" severity="danger" rounded @click="removePermission(data)" />
                  </template>
                </Column>
              </DataTable>
            </div>
          </TabPanel>
        </TabPanels>
      </Tabs>

      <div class="flex justify-end gap-2 mt-4">
        <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="closeDialog" />
        <Button label="Create" icon="pi pi-check" @click="createApplicationAction" />
      </div>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import {  showSuccess } from "../../../utils/toast";
import { AllOpenIdValues } from "../../../utils/ClientPermissionLists";

const router = useRouter();

const visible = ref(true);
const activeTab = ref("0");

interface ApplicationCreateRequest {
  displayName: string;
  clientId: string;
  clientSecret: string;
  applicationType: string;
  clientType: string;
  consentType: string;
  redirectUris: string[];
  postLogoutRedirectUris: string[];
  permissions: string[];
}

const newApplication = ref<ApplicationCreateRequest>({
  displayName: "",
  clientId: "",
  clientSecret: "",
  applicationType: "",
  clientType: "",
  consentType: "",
  redirectUris: [],
  postLogoutRedirectUris: [],
  permissions: []
});

const redirectUris = ref<{ value: string }[]>([]);
const newRedirectUri = ref("");

const postLogoutRedirectUris = ref<{ value: string }[]>([]);
const newPostLogoutRedirectUri = ref("");

const permissions = ref<{ value: string }[]>([]);
const selectedPermission = ref<{ label: string; value: string } | null>(null);
const allPermissions = ref<{ label: string; value: string }[]>([]);
const filteredPermissions = ref<{ label: string; value: string }[]>([]);

onMounted(() => {
  allPermissions.value = AllOpenIdValues.map(v => ({ label: v, value: v }));
});

function addRedirectUri() {
  if (newRedirectUri.value.trim()) {
    redirectUris.value.push({ value: newRedirectUri.value.trim() });
    newRedirectUri.value = "";
  }
}

function removeRedirectUri(uri: { value: string }) {
  redirectUris.value = redirectUris.value.filter(u => u.value !== uri.value);
}

function addPostLogoutRedirectUri() {
  if (newPostLogoutRedirectUri.value.trim()) {
    postLogoutRedirectUris.value.push({ value: newPostLogoutRedirectUri.value.trim() });
    newPostLogoutRedirectUri.value = "";
  }
}

function removePostLogoutRedirectUri(uri: { value: string }) {
  postLogoutRedirectUris.value = postLogoutRedirectUris.value.filter(u => u.value !== uri.value);
}

function searchPermissions(event: any) {
  const query = event.query.toLowerCase();
  filteredPermissions.value = allPermissions.value.filter(
      p => p.label.toLowerCase().includes(query) && !permissions.value.some(ap => ap.value === p.value)
  );
}

function addPermission() {
  if (selectedPermission.value) {
    permissions.value.push({ value: selectedPermission.value.value });
    selectedPermission.value = null;
    filteredPermissions.value = [];
  }
}

function removePermission(p: { value: string }) {
  permissions.value = permissions.value.filter(x => x.value !== p.value);
}

async function createApplicationAction() {
  newApplication.value.redirectUris = redirectUris.value.map(u => u.value);
  newApplication.value.postLogoutRedirectUris = postLogoutRedirectUris.value.map(u => u.value);
  newApplication.value.permissions = permissions.value.map(p => p.value);

  console.log(newApplication.value);
  showSuccess("Creating application...(Dummy)");
}

function closeDialog() {
  visible.value = false;
  router.push("/applications");
}
</script>
