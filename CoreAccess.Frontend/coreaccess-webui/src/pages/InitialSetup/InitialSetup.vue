<script setup lang="ts">


const submit = () => {
  // Handle form submission logic here
  console.log("Form submitted");
};

</script>

<template>
  <h1 class="text-2xl font-bold text-primary mt-4 mb-4 text-center">CoreAccess Setup</h1>
  <div class="card flex justify-center mt-20">
    <Stepper value="1">
      <StepList>
        <Step value="1">General Settings</Step>
        <Step value="2">JWT Settings</Step>
        <Step value="3">Create Admin User</Step>
        <Step value="4">Confirm</Step>
      </StepList>
      <StepPanels>
        <StepPanel v-slot="{ activateCallback }" value="1">
          <div class="flex flex-col h-120">
            <form class="flex flex-col gap-4 w-full max-w-md mx-auto">
              <div class="form-group">
                <label for="baseurl" class="block text-sm font-medium text-gray-700">Base URL</label>
                <input type="text" id="baseurl" class="form-control border rounded px-3 py-2 w-full" placeholder="Enter Base URL" />
              </div>
              <div class="form-group">
                <label for="port" class="block text-sm font-medium text-gray-700">Port</label>
                <input type="number" id="port" class="form-control border rounded px-3 py-2 w-full" placeholder="Enter port number" />
              </div>
            </form>
          </div>
          <div class="flex pt-6 justify-end">
            <Button label="Next" icon="pi pi-arrow-right" iconPos="right" @click="activateCallback('2')" />
          </div>
        </StepPanel>
        <StepPanel v-slot="{ activateCallback }" value="2">
          <div class="flex flex-col h-120">
            <div class="border-2 border-dashed border-surface-200 dark:border-surface-700 rounded bg-surface-50 dark:bg-surface-950 flex-auto flex justify-center items-center font-medium">Content II</div>
          </div>
          <div class="flex pt-6 justify-between">
            <Button label="Back" severity="secondary" icon="pi pi-arrow-left" @click="activateCallback('1')" />
            <Button label="Next" icon="pi pi-arrow-right" iconPos="right" @click="activateCallback('3')" />
          </div>
        </StepPanel>
        <StepPanel v-slot="{ activateCallback }" value="3">
          <div class="flex flex-col h-120">
            <div class="border-2 border-dashed border-surface-200 dark:border-surface-700 rounded bg-surface-50 dark:bg-surface-950 flex-auto flex justify-center items-center font-medium">Content III</div>
          </div>
          <div class="flex pt-6 justify-between">
            <Button label="Back" severity="secondary" icon="pi pi-arrow-left" @click="activateCallback('2')" />
            <Button label="Next" icon="pi pi-arrow-right" iconPos="right" @click="activateCallback('4')" />
          </div>
        </StepPanel>
        <StepPanel v-slot="{ activateCallback }" value="4">
          <div class="flex flex-col h-120">
            <div class="border-2 border-dashed border-surface-200 dark:border-surface-700 rounded bg-surface-50 dark:bg-surface-950 flex-auto flex justify-center items-center font-medium">Content IV</div>
          </div>
          <div class="flex pt-6 justify-between">
            <Button label="Back" severity="secondary" icon="pi pi-arrow-left" @click="activateCallback('2')" />
            <Button label="Finish" icon="pi pi-check" severity="success" iconPos="right" @click="submit" />
          </div>
        </StepPanel>
      </StepPanels>
    </Stepper>
  </div>
</template>

<style scoped>

</style>