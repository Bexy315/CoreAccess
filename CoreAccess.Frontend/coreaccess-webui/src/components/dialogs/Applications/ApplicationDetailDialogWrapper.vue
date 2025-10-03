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
            <Tab value="1">Secrets</Tab>
            <Tab value="2">Settings</Tab>
          </TabList>
          <TabPanels>
            <!-- General -->
            <TabPanel value="0">
              <div class="flex flex-col gap-6">
                <!-- View Mode -->
                <div v-if="!editingGeneral">
                  <div class="grid grid-cols-2 gap-4">
                    <div><strong>ID:</strong> {{ application.id }}</div>
                    <div><strong>Name:</strong> {{ application.displayName }}</div>
                    <div><strong>Client ID:</strong> {{ application.clientId }}</div>
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
                      <InputText v-model="generalForm.displayName" class="w-full" />
                    </div>
                    <div>
                      <label class="font-medium">Client ID</label>
                      <InputText v-model="generalForm.clientId" class="w-full" />
                    </div>
                  </div>

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

            <!-- Secrets -->
            <TabPanel value="1">
              <div>
                <h3 class="font-semibold mb-3">Secrets</h3>
                <p class="text-gray-500">Secret management coming soon...</p>
              </div>
            </TabPanel>

            <!-- Settings -->
            <TabPanel value="2">
              <p class="m-0">
                Settings section (placeholder).
              </p>
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
import { showError, showSuccess } from "../../../utils/toast.ts";
import {getApplication} from "../../../services/ApplicationService.ts";

async function fetchApplication(id: string) {
  return await getApplication(id);
}
async function updateApplication(id: string, data: any) {
  return { ...data, id };
}

const route = useRoute();
const router = useRouter();

const visible = ref(true);
const loading = ref(true);
const application = ref<any | null>(null);

const activeTab = ref<string>((route.query.tab as string) ?? "0");

const editingGeneral = ref(false);
const generalForm = ref({
  displayName: "",
  clientId: "",
  description: ""
});
const dirtyGeneral = ref(false);

const loadApplication = async () => {
  loading.value = true;
  application.value = await fetchApplication(route.params.id as string);
  loading.value = false;
  resetGeneralForm();
};

onMounted(loadApplication);

watch(() => route.params.id, loadApplication);

watch(activeTab, (val) => {
  if (route.query.tab === val) return;
  router.replace({
    path: route.path,
    query: { ...route.query, tab: val }
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

const resetGeneralForm = () => {
  generalForm.value = {
    displayName: application.value?.displayName ?? "",
    clientId: application.value?.clientId ?? "",
    description: application.value?.description ?? ""
  };
  editingGeneral.value = false;
  dirtyGeneral.value = false;
};

watch(
    generalForm,
    () => {
      dirtyGeneral.value = true;
    },
    { deep: true }
);

const saveGeneral = async () => {
  try {
    await updateApplication(application.value.id, generalForm.value);
    application.value = { ...application.value, ...generalForm.value };
    showSuccess("Application details updated successfully.");
    resetGeneralForm();
  } catch (e: any) {
    showError(e.message, "Failed to update application details.");
  }
};

const cancelGeneralEdit = () => {
  resetGeneralForm();
};

const closeDialog = () => {
  const { tab, ...rest } = route.query;
  router.push({
    path: "/applications",
    query: rest
  });
};
</script>
