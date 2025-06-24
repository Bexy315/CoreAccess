<script setup lang="ts">
defineProps<{
  general: {
    baseUri: string
    systemLogLevel: string
    disableRegistration: string
  }
  jwt: {
    jwtSecret: string
    issuer: string
    audience: string
    expiresIn: string
  }
  admin: {
    username: string
    password: string
    email: string
    firstName: string
    lastName: string
    phone: string
    address: string
    city: string
    state: string
    zip: string
    country: string
    status: number
  }
}>()

const isMissing = (val: unknown) => {
  return !val || (typeof val === 'string' && val.trim() === '')
}
</script>

<template>
  <div class="space-y-6">
    <!-- General Settings -->
    <Card>
      <template #title>General Settings</template>
      <template #content>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <span class="font-medium">Base URI*:</span>
            <div :class="isMissing(general.baseUri) ? 'text-red-500' : ''">
              {{ general.baseUri || 'Missing value' }}
            </div>
          </div>

          <div>
            <span class="font-medium">System Log Level*:</span>
            <div :class="isMissing(general.systemLogLevel) ? 'text-red-500' : ''">
              {{ general.systemLogLevel || 'Missing value' }}
            </div>
          </div>

          <div>
            <span class="font-medium">Disable Registration: </span>
            <Tag :severity="general.disableRegistration === 'true' ? 'danger' : 'success'" class="mt-1">
              {{ general.disableRegistration === 'true' ? 'Yes' : 'No' }}
            </Tag>
          </div>
        </div>
      </template>
    </Card>

    <Divider />

    <!-- JWT Settings -->
    <Card>
      <template #title>JWT Settings</template>
      <template #content>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <span class="font-medium">JWT Secret*:</span>
            <div :class="isMissing(jwt.jwtSecret) ? 'text-red-500' : ''">
              {{ jwt.jwtSecret || 'Missing value' }}
            </div>
          </div>

          <div>
            <span class="font-medium">Issuer*:</span>
            <div :class="isMissing(jwt.issuer) ? 'text-red-500' : ''">
              {{ jwt.issuer || 'Missing value' }}
            </div>
          </div>

          <div>
            <span class="font-medium">Audience*:</span>
            <div :class="isMissing(jwt.audience) ? 'text-red-500' : ''">
              {{ jwt.audience || 'Missing value' }}
            </div>
          </div>

          <div>
            <span class="font-medium">Expires In (seconds)*:</span>
            <div :class="isMissing(jwt.expiresIn) ? 'text-red-500' : ''">
              {{ jwt.expiresIn || 'Missing value' }}
            </div>
          </div>
        </div>
      </template>
    </Card>

    <Divider />

    <!-- Admin User -->
    <Card>
      <template #title>Admin User</template>
      <template #content>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <span class="font-medium">Username*:</span>
            <div :class="isMissing(admin.username) ? 'text-red-500' : ''">
              {{ admin.username || 'Missing value' }}
            </div>
          </div>

          <div>
            <span class="font-medium">Password*:</span>
            <div :class="isMissing(admin.password) ? 'text-red-500' : ''">
              {{ admin.password ? '••••••••' : 'Missing value' }}
            </div>
          </div>

          <Accordion value="1">
            <AccordionPanel value="0">
              <AccordionHeader class="text-black">Optional Metadata</AccordionHeader>
              <AccordionContent>
              <div>
                <span class="font-medium">Email: </span>
                <div :class="isMissing(admin.email) ? 'text-gray-400 italic' : ''">
                  {{ admin.email || 'Not provided' }}
                </div>
              </div>

              <div>
                <span class="font-medium">First Name: </span>
                <div :class="isMissing(admin.firstName) ? 'text-gray-400 italic' : ''">
                  {{ admin.firstName || 'Not provided' }}
                </div>
              </div>

              <div>
                <span class="font-medium">Last Name: </span>
                <div :class="isMissing(admin.lastName) ? 'text-gray-400 italic' : ''">
                  {{ admin.lastName || 'Not provided' }}
                </div>
              </div>

              <div>
                <span class="font-medium">Phone: </span>
                <div :class="isMissing(admin.phone) ? 'text-gray-400 italic' : ''">
                  {{ admin.phone || 'Not provided' }}
                </div>
              </div>

              <div>
                <span class="font-medium">Address: </span>
                <div :class="isMissing(admin.address) ? 'text-gray-400 italic' : ''">
                  {{ admin.address || 'Not provided' }}
                </div>
              </div>

              <div>
                <span class="font-medium">City: </span>
                <div :class="isMissing(admin.city) ? 'text-gray-400 italic' : ''">
                  {{ admin.city || 'Not provided' }}
                </div>
              </div>

              <div>
                <span class="font-medium">State: </span>
                <div :class="isMissing(admin.state) ? 'text-gray-400 italic' : ''">
                  {{ admin.state || 'Not provided' }}
                </div>
              </div>

              <div>
                <span class="font-medium">ZIP: </span>
                <div :class="isMissing(admin.zip) ? 'text-gray-400 italic' : ''">
                  {{ admin.zip || 'Not provided' }}
                </div>
              </div>

              <div>
                <span class="font-medium">Country: </span>
                <div :class="isMissing(admin.country) ? 'text-gray-400 italic' : ''">
                  {{ admin.country || 'Not provided' }}
                </div>
              </div>
              </AccordionContent>
            </AccordionPanel>
          </Accordion>
        </div>
      </template>
    </Card>
  </div>
</template>
