import { ref } from 'vue'
import { coreAuth } from '@coreaccess/client'
import {router} from "../router";

export const isAuthenticated = ref(false)
export const currentUser = ref<any>(null)

coreAuth.configure({
    baseUrl: import.meta.env.VITE_API_BASE_URL || '/api'
})

coreAuth.onAuthChange((val : boolean) => {
    isAuthenticated.value = val
})
coreAuth.onLogin(() => {
    currentUser.value = coreAuth.getCurrentUser();

    const redirect = router.currentRoute.value.query.redirect;
    if (typeof redirect === 'string') {
        router.push(redirect);
    } else if (Array.isArray(redirect)) {
        router.push(redirect.join('')); // Convert array to string
    } else if (typeof redirect === 'object' && redirect !== null) {
        router.push(redirect);
    } else {
        router.push({ name: 'Dashboard' });
    }
});
coreAuth.onLogout(() => {
    currentUser.value = null
    if(router.currentRoute.value.name !== 'Login') {
        router.push({name: 'Login', query: {redirect: router.currentRoute.value.fullPath}})
    }
})

export async function login(credentials: { username: string; password: string }) {
    await coreAuth.login(credentials)
}

export function logout() {
    coreAuth.logout()
}

export function getUser() {
    return currentUser.value
}

export function restoreAuth() {
    if (coreAuth.isLoggedIn()) {
        isAuthenticated.value = true
        currentUser.value = coreAuth.getCurrentUser()
    }
}
