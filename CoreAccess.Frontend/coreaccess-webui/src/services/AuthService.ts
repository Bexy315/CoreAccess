import { setupCoreAccess, coreAuth } from '@coreaccess/client';

setupCoreAccess({
    baseUrl: 'https://auth.myapp.com/api',
    onLogout: () => console.log('User logged out'),
});

export function login(username: string, password: string) {
    return coreAuth.login({ username, password });
}

export function logout() {
    return coreAuth.logout();
}