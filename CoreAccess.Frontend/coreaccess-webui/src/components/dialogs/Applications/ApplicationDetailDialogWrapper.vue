<template>
  <Dialog
      v-model:visible="visible"
      modal
      header="Application Details"
      @hide="closeDialog"
      class="w-10/12 md:w-3/4 lg:w-1/2 h-10/12 md:h-3/4 lg:h-2/3"
  >
    <template v-if="loading">
      <ProgressSpinner />
    </template>

    <template v-else>
      <div v-if="application">
        <Tabs v-model:value="activeTab">
          <TabList>
            <Tab value="0">General</Tab>
            <Tab value="1">Redirect URIs</Tab>
            <Tab value="2">Client Permissions</Tab>
          </TabList>

          <TabPanels>
            <!-- General -->
            <TabPanel value="0">
              <div class="flex flex-col gap-6">
                <div v-if="!editingGeneral">
                  <div class="grid grid-cols-2 gap-4">
                    <div><strong>ID:</strong> {{ application.id }}</div>
                    <div><strong>Name:</strong> {{ application.displayName }}</div>
                    <div><strong>Client ID:</strong> {{ application.clientId }}</div>
                    <div><strong>Application Type:</strong> {{ application.applicationType }}</div>
                    <div><strong>Client Type:</strong> {{ application.clientType }}</div>
                    <div><strong>Consent Type:</strong> {{ application.consentType }}</div>
                    <div v-if="application.clientSecret" class="flex items-center gap-2">
                      <strong>Client Secret:</strong>
                      <span class="font-mono select-all">
                          {{ showSecret ? application.clientSecret : maskedSecret }}
                      </span>
                      <Button
                          :icon="showSecret ? 'pi pi-eye-slash' : 'pi pi-eye'"
                          size="small"
                          text
                          rounded
                          aria-label="Toggle secret visibility"
                          @click="toggleSecret"
                      />
                    </div>
                  </div>
                  <div class="flex justify-end mt-4">
                    <Button label="Edit" icon="pi pi-pencil" @click="editingGeneral = true" />
                  </div>
                </div>

                <div v-else>
                  <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                      <label class="font-medium">Name</label>
                      <InputText v-model="generalForm.displayName" class="w-full" />
                    </div>
                    <div>
                      <label class="font-medium">Client ID</label>
                      <InputText v-model="generalForm.clientId" class="w-full" />
                    </div>
                    <div>
                      <label class="font-medium">Client Secret</label>
                      <InputText v-model="generalForm.clientSecret" class="w-full" />
                    </div>
                    <div>
                      <label class="font-medium">Application Type</label>
                      <InputText v-model="application.applicationType" class="w-full" disabled />
                    </div>
                    <div>
                      <label class="font-medium">Client Type</label>
                      <InputText v-model="application.clientType" class="w-full" disabled />
                    </div>
                    <div>
                      <label class="font-medium">Consent Type</label>
                      <InputText v-model="application.consentType" class="w-full" disabled />
                    </div>
                  </div>

                  <div class="flex justify-end gap-2 mt-6">
                    <Button label="Cancel" icon="pi pi-times" severity="secondary" @click="cancelGeneralEdit" />
                    <Button label="Save Changes" icon="pi pi-save" :disabled="!dirtyGeneral" @click="saveGeneral" />
                  </div>
                </div>
              </div>
            </TabPanel>

            <!-- Redirect URIs -->
            <TabPanel value="1">
              <div class="flex flex-col gap-6">
                <!-- Redirect URIs -->
                <div>
                  <h3 class="font-semibold mb-3">Redirect URIs</h3>
                  <div class="flex justify-between items-center mb-2">
                    <div>
                      <InputText
                          v-model="newRedirectUri"
                          placeholder="Enter redirect URI"
                          class="flex-grow"
                      />
                      <Button
                          class="ml-2"
                          label="Add"
                          icon="pi pi-plus"
                          :disabled="!newRedirectUri"
                          @click="addRedirectUri"
                      />
                    </div>
                    <Button
                        v-if="redirectUris.some(u => u.isNew || u.isToDelete)"
                        label="Save Changes"
                        icon="pi pi-save"
                        severity="success"
                        class="ml-2"
                        @click="saveRedirectUris"
                    />
                  </div>

                  <DataTable :value="redirectUris" tableStyle="min-width: 40rem">
                    <Column header="URI">
                      <template #body="{ data }">
            <span
                :class="{
                'text-green-600 font-medium': data.isNew,
                'line-through text-gray-400': data.isToDelete
              }"
            >
              {{ data.value }}
            </span>
                      </template>
                    </Column>

                    <Column header="Actions" bodyStyle="text-align:center;width:6rem">
                      <template #body="{ data }">
                        <Button
                            icon="pi pi-trash"
                            severity="danger"
                            rounded
                            @click="toggleRedirectUriDelete(data)"
                        />
                      </template>
                    </Column>
                  </DataTable>
                </div>

                <!-- Post-Logout Redirect URIs -->
                <div>
                  <h3 class="font-semibold mb-3">Post-Logout Redirect URIs</h3>
                  <div class="flex justify-between items-center mb-2">
                    <div>
                      <InputText
                          v-model="newPostLogoutRedirectUri"
                          placeholder="Enter post-logout redirect URI"
                          class="flex-grow"
                      />
                      <Button
                          class="ml-2"
                          label="Add"
                          icon="pi pi-plus"
                          :disabled="!newPostLogoutRedirectUri"
                          @click="addPostLogoutRedirectUri"
                      />
                    </div>
                    <Button
                        v-if="postLogoutRedirectUris.some(u => u.isNew || u.isToDelete)"
                        label="Save Changes"
                        icon="pi pi-save"
                        severity="success"
                        class="ml-2"
                        @click="savePostLogoutRedirectUris"
                    />
                  </div>

                  <DataTable :value="postLogoutRedirectUris" tableStyle="min-width: 40rem">
                    <Column header="URI">
                      <template #body="{ data }">
            <span
                :class="{
                'text-green-600 font-medium': data.isNew,
                'line-through text-gray-400': data.isToDelete
              }"
            >
              {{ data.value }}
            </span>
                      </template>
                    </Column>

                    <Column header="Actions" bodyStyle="text-align:center;width:6rem">
                      <template #body="{ data }">
                        <Button
                            icon="pi pi-trash"
                            severity="danger"
                            rounded
                            @click="togglePostLogoutRedirectUriDelete(data)"
                        />
                      </template>
                    </Column>
                  </DataTable>
                </div>
              </div>
            </TabPanel>

            <!-- Permissions -->
            <TabPanel value="2">
              <div class="flex flex-col gap-4 h-full">
                <h3 class="font-semibold mb-2">Client Permissions</h3>

                <DataTable :value="permissions" tableStyle="min-width: 40rem">
                  <template #header>
                    <div class="flex justify-between items-center">
                      <div>
                        <AutoComplete
                            v-model="selectedPermission"
                            :suggestions="filteredPermissions"
                            optionLabel="label"
                            placeholder="Search permissions..."
                            @complete="searchPermissions"
                            dropdown
                            class="flex-grow"
                        />
                        <Button
                            label="Add"
                            class="ml-2"
                            icon="pi pi-plus"
                            :disabled="!selectedPermission"
                            @click="addPermission"
                        />
                      </div>
                      <Button
                          v-if="permissions.some(p => p.isNew || p.isToDelete)"
                          label="Save Changes"
                          icon="pi pi-save"
                          severity="success"
                          @click="savePermissionChanges"
                      />
                    </div>
                  </template>

                  <Column header="Name">
                    <template #body="{ data }">
                      <span :class="{
                        'text-green-600 font-medium': data.isNew,
                        'line-through text-gray-400': data.isToDelete
                      }">
                        {{ data.value }}
                      </span>
                    </template>
                  </Column>

                  <Column header="Actions" bodyStyle="text-align:center;width:6rem">
                    <template #body="{ data }">
                      <Button
                          icon="pi pi-trash"
                          severity="danger"
                          rounded
                          @click="togglePermissionDelete(data)"
                      />
                    </template>
                  </Column>
                </DataTable>
              </div>
            </TabPanel>
          </TabPanels>
        </Tabs>
      </div>
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import {ref, onMounted, watch, computed} from "vue";
import { useRoute, useRouter } from "vue-router";
import { showError, showSuccess } from "../../../utils/toast";
import {getApplication, updateApplication} from "../../../services/ApplicationService";
import { AllOpenIdValues } from "../../../utils/ClientPermissionLists";
import type {ApplicationDetailDto, ApplicationUpdateRequest} from "../../../model/ApplicationModel.ts";

const route = useRoute();
const router = useRouter();

const visible = ref(true);
const loading = ref(true);
const application = ref<ApplicationDetailDto | null>(null);

const permissions = ref<{ value: string; isNew?: boolean; isToDelete?: boolean }[]>([]);
const selectedPermission = ref<{ label: string; value: string } | null>(null);
const allPermissions = ref<{ label: string; value: string }[]>([]);
const filteredPermissions = ref<{ label: string; value: string }[]>([]);

const activeTab = ref<string>((route.query.tab as string) ?? "0");

const editingGeneral = ref(false);
const generalForm = ref({ displayName: "", clientId: "", clientSecret: "" });
const dirtyGeneral = ref(false);

const showSecret = ref(false)

const maskedSecret = computed(() => {
  if (application.value && !application.value.clientSecret) return ''
  const secret = application.value?.clientSecret??''
  return secret.length > 8
      ? secret.slice(0, 4) + '••••••••' + secret.slice(-2)
      : '••••••••'
})

const toggleSecret = () => {
  showSecret.value = !showSecret.value
}

async function fetchApplication(id: string) {
  return await getApplication(id);
}

const loadApplication = async () => {
  loading.value = true;
  application.value = await fetchApplication(route.params.id as string);
  resetPermissionList(application.value?.permissions || []);
  redirectUris.value = (application.value?.redirectUris as string[] || []).map(u => ({ value: u }));
  postLogoutRedirectUris.value = (application.value?.postLogoutRedirectUris as string[] || []).map(u => ({ value: u }));
  loading.value = false;
  resetGeneralForm();
};

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

function resetPermissionList(permissionList: string[]) {
  permissions.value = permissionList.map(p => ({ value: p }));
}

function searchPermissions(event: any) {
  const query = event.query.toLowerCase();
  filteredPermissions.value = allPermissions.value.filter(
      p =>
          p.label.toLowerCase().includes(query) &&
          !permissions.value.some(ap => ap.value === p.value)
  );
}

function addPermission() {
  if (selectedPermission.value) {
    const newValue = selectedPermission.value.value;
    if (!permissions.value.some(p => p.value === newValue)) {
      permissions.value.push({ value: newValue, isNew: true });
    }
    selectedPermission.value = null;
    filteredPermissions.value = [];
  }
}

function togglePermissionDelete(perm: { value: string; isNew?: boolean; isToDelete?: boolean }) {
  if (perm.isNew) {
    permissions.value = permissions.value.filter(p => p.value !== perm.value);
  } else {
    perm.isToDelete = !perm.isToDelete;
  }
}

const savePermissionChanges = async () => {
  const dto: ApplicationUpdateRequest = {
    permissions: permissions.value.filter(p => !p.isToDelete).map(p => p.value),
  };

  await updateApplication(route.params.id as string, dto).then(
      res => {
        application.value = res;
        showSuccess("Application permissions updated.");
        resetPermissionList(application.value?.permissions || []);
      }
  ).catch(err => {
    console.error("Failed to update application permissions", err);
    showError(err.data || "Failed to update application permissions");
  });
};

function resetGeneralForm() {
  generalForm.value = {
    displayName: application.value?.displayName ?? "",
    clientId: application.value?.clientId ?? "",
    clientSecret: application.value?.clientSecret ?? ""
  };
  editingGeneral.value = false;
  dirtyGeneral.value = false;
}

watch(generalForm, (newVal, _) => {
  dirtyGeneral.value =
      newVal.displayName !== (application.value?.displayName ?? "") ||
      newVal.clientId !== (application.value?.clientId ?? "") ||
      newVal.clientSecret !== (application.value?.clientSecret ?? "");
}, { deep: true });

function cancelGeneralEdit() {
  resetGeneralForm();
}

const saveGeneral = async () => {
  const dto: ApplicationUpdateRequest = {
    displayName: generalForm.value.displayName,
    clientId: generalForm.value.clientId,
  };

  await updateApplication(route.params.id as string, dto).then(res => {
    application.value = res;
    showSuccess("Application details updated.");
    resetGeneralForm();
  }).catch(err => {
    console.error("Failed to update application", err);
    showError(err.data || "Failed to update application");
  });
};


function closeDialog() {
  const { tab, ...rest } = route.query;
  router.push({ path: "/applications", query: rest });
}

onMounted(() => {
  allPermissions.value = AllOpenIdValues.map(v => ({ label: v, value: v }));
  loadApplication();
});

const redirectUris = ref<{ value: string; isNew?: boolean; isToDelete?: boolean }[]>([]);
const postLogoutRedirectUris = ref<{ value: string; isNew?: boolean; isToDelete?: boolean }[]>([]);

const newRedirectUri = ref("");
const newPostLogoutRedirectUri = ref("");

// === Redirect URIs ===
function addRedirectUri() {
  const uri = newRedirectUri.value.trim();
  if (uri && !redirectUris.value.some(u => u.value === uri)) {
    redirectUris.value.push({ value: uri, isNew: true });
  }
  newRedirectUri.value = "";
}

function toggleRedirectUriDelete(uri: { value: string; isNew?: boolean; isToDelete?: boolean }) {
  if (uri.isNew) {
    redirectUris.value = redirectUris.value.filter(u => u.value !== uri.value);
  } else {
    uri.isToDelete = !uri.isToDelete;
  }
}
const saveRedirectUris = async () => {
  const dto: ApplicationUpdateRequest = {
    redirectUris: redirectUris.value.filter(u => !u.isToDelete).map(u => u.value),
  };

  await updateApplication(route.params.id as string, dto).then(
      res => {
        application.value = res;
        showSuccess("Redirect URIs updated.");
        redirectUris.value = (application.value?.redirectUris as string[] || []).map(u => ({ value: u }));
      }
    ).catch(err => {
      console.error("Failed to update redirect URIs", err);
      showError(err.data || "Failed to update redirect URIs");
    });
};

// === Post-Logout Redirect URIs ===
function addPostLogoutRedirectUri() {
  const uri = newPostLogoutRedirectUri.value.trim();
  if (uri && !postLogoutRedirectUris.value.some(u => u.value === uri)) {
    postLogoutRedirectUris.value.push({ value: uri, isNew: true });
  }
  newPostLogoutRedirectUri.value = "";
}

function togglePostLogoutRedirectUriDelete(uri: { value: string; isNew?: boolean; isToDelete?: boolean }) {
  if (uri.isNew) {
    postLogoutRedirectUris.value = postLogoutRedirectUris.value.filter(u => u.value !== uri.value);
  } else {
    uri.isToDelete = !uri.isToDelete;
  }
}

const savePostLogoutRedirectUris = async () => {
  const dto: ApplicationUpdateRequest = {
    postLogoutRedirectUris: postLogoutRedirectUris.value.filter(u => !u.isToDelete).map(u => u.value),
  };

  await updateApplication(route.params.id as string, dto).then(
      res => {
        application.value = res;
        showSuccess("Post-logout Redirect URIs updated.");
        postLogoutRedirectUris.value = (application.value?.postLogoutRedirectUris as string[] || []).map(u => ({ value: u }));
      }
      ).catch(err => {
        console.error("Failed to update post-logout redirect URIs", err);
        showError(err.data || "Failed to update post-logout redirect URIs");
      });
  };

</script>
