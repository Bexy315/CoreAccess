<script setup lang="ts">
import { onMounted, ref, watch } from "vue";
import { useConfirm } from "primevue";
import { router } from "../router";
import { useRoute } from "vue-router";
import {getApplications} from "../services/ApplicationService.ts";
import type {ApplicationDto} from "../model/ApplicationModel.ts";

const route = useRoute();

const search = ref<string>(String(route.query.search || ""));
const page = ref<number>(Number(route.query.page || 1));
const pageSize = ref<number>(Number(route.query.pageSize || 10));

const applications = ref<ApplicationDto[]>([]);
const selectedApplications = ref<ApplicationDto[]>([]);
const rowsPerPageOptions = ref([5, 10, 20, 50]);
const totalRecords = ref(0);
const loading = ref(false);
const confirm = useConfirm();
const first = ref(0)

async function fetchApplications(opts: {
  page: number;
  pageSize: number;
  search?: string;
}) {
  return await getApplications({
    page: opts.page,
    pageSize: opts.pageSize,
    search: opts.search,
  });
}

async function deleteApplication(id: string) {
  console.log("Deleting application:", id);
}

const loadApplications = async (page = 0, pageSize = 10) => {
  loading.value = true;
  try {
    const response = await fetchApplications({
      page: page,
      pageSize: pageSize,
      search: search.value || undefined,
    });
    applications.value = response.items;
    totalRecords.value = response.totalCount;
  } catch (err) {
    console.error("Failed to load applications", err);
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
      loadApplications();
    }
);

onMounted(async () => {
  await loadApplications(page.value, pageSize.value);
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

 function onPageChange(event: any) {
  page.value = event.page + 1;
  pageSize.value = event.rows;
  updateQuery({});
}

const confirmDelete = () => {
  if (selectedApplications.value.length !== 1) {
    console.warn("Please select exactly one application to delete.");
    return;
  }

  if(selectedApplications.value[0].clientId === "coreaccess") {
    console.warn("The coreaccess default application cannot be deleted.");
    return;
  }

  confirm.require({
    message: "Do you want to delete this application?",
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
        await deleteApplication(selectedApplications.value[0].id);
        selectedApplications.value = [];
        await loadApplications((page.value - 1), pageSize.value);
      } catch (err) {
        console.error("Error deleting application", err);
      }
    },
    reject: () => {
    },
  });
};

function openDetailsDialog(app: ApplicationDto) {
  router.push({
    path: `/applications/${app.id}`,
    query: route.query,
  });
}

function createNewApplication() {
  console.log("New application creation triggered");
}
</script>

<template>
  <div class="p-4">
    <h1 class="text-2xl font-bold mb-4">Application Management</h1>
    <p class="mb-4">Here you can manage your Applications.</p>

    <DataTable
        :value="applications"
        :lazy="true"
        v-model:selection="selectedApplications"
        :totalRecords="totalRecords"
        paginator
        paginatorTemplate="RowsPerPageDropdown FirstPageLink PrevPageLink CurrentPageReport NextPageLink LastPageLink"
        currentPageReportTemplate="{first} to {last} of {totalRecords}"
        :first="first"
        :last="first + pageSize - 1"
        :rows="pageSize"
        :rowsPerPageOptions="rowsPerPageOptions"
        :loading="loading"
        stripedRows
        @page="onPageChange"
        responsiveLayout="scroll"
        removableSort
    >
      <template #header>
        <Toolbar class="!bg-white">
          <template #start>
            <div class="flex gap-2">
              <Button
                  label="New"
                  icon="pi pi-plus"
                  @click="createNewApplication"
                  variant="text"
                  severity="secondary"
              />

              <Button
                  v-if="selectedApplications.length === 1 && selectedApplications[0].clientId !== 'coreaccess'"
                  label="Delete"
                  icon="pi pi-trash"
                  severity="danger"
                  @click="confirmDelete"
              />
            </div>
          </template>

          <template #end>
            <div class="flex gap-2 items-center">
              <span class="p-input-icon-left">
                <InputText
                    v-model="search"
                    placeholder="Search applications..."
                    @input="onSearchChange"
                />
              </span>
            </div>
          </template>
        </Toolbar>
      </template>

      <Column selectionMode="multiple" headerStyle="width: 3rem"></Column>
      <Column field="clientId" header="Client ID" />
      <Column field="displayName" header="Name" />
      <Column field="clientType" header="Client Type" />
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

    <router-view />
  </div>
  <ConfirmDialog></ConfirmDialog>
</template>

<style scoped></style>
