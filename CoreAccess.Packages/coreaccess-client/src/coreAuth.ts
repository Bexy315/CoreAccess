import type { CoreAccessConfig } from './types';
import {
    LocalStorageTokenStorage,
    type TokenStorage
} from './core/tokenStorage';
import httpClient from './core/httpClient';

type AuthChangeCallback = (isAuthenticated: boolean) => void;
type VoidCallback = () => void;

class CoreAuth {
    private isAuthenticated = false;
    private user: any = null;
    private config?: CoreAccessConfig;

    private tokenStorage: TokenStorage = new LocalStorageTokenStorage();

    private loginListeners = new Set<VoidCallback>();
    private logoutListeners = new Set<VoidCallback>();
    private authChangeListeners = new Set<AuthChangeCallback>();

    configure(config: CoreAccessConfig) {
        this.config = config;
        httpClient.defaults.baseURL = config.baseUrl;

        this.restoreFromStorage();
    }

    restoreFromStorage() {
        const access = this.tokenStorage.getAccessToken();

        if (access) {
            this.isAuthenticated = true;
            this.user = localStorage.getItem('coreaccess_userId')?? '';

            this.notifyLogin();
        } else {
            this.isAuthenticated = false;
            this.user = null;

            this.notifyLogout();
        }
    }

    onLogin(callback: VoidCallback) {
        this.loginListeners.add(callback);
    }

    onLogout(callback: VoidCallback) {
        this.logoutListeners.add(callback);
    }

    onAuthChange(callback: AuthChangeCallback) {
        this.authChangeListeners.add(callback);
    }

    private notifyLogin() {
        this.loginListeners.forEach(cb => cb());
        this.authChangeListeners.forEach(cb => cb(true));
    }

    private notifyLogout() {
        this.logoutListeners.forEach(cb => cb());
        this.authChangeListeners.forEach(cb => cb(false));
    }

    async login(credentials: { username: string; password: string }) {
        try {
            const response = await httpClient.post('/auth/login', {
                username: credentials.username,
                password: credentials.password
            });

            const data = response.data;

            if (!data?.accessToken || !data?.refreshToken) {
                throw new Error('Invalid response: tokens missing from server');
            }

            this.tokenStorage.setTokens(data.accessToken, data.refreshToken);
            this.user = response.data.userId;
            localStorage.setItem('coreaccess_userId', this.user);
            this.isAuthenticated = true;

            this.notifyLogin();
        } catch (error: any) {
            this.tokenStorage.clearTokens();
            this.user = null;
            localStorage.removeItem('coreaccess_userId');
            this.isAuthenticated = false;

            this.notifyLogout();

            const msg = error.response?.data?.message || error.message || 'Login failed';
            throw new Error(msg);
        }
    }

    logout() {
        this.tokenStorage.clearTokens();

        this.isAuthenticated = false;
        this.user = null;

        this.notifyLogout();
    }

    isLoggedIn(): boolean {
        return this.isAuthenticated;
    }

    getCurrentUser(): any | null {
        return this.user;
    }

    getAccessToken(): string | null {
        return this.tokenStorage.getAccessToken();
    }

    refreshAccessToken(): Promise<string | null> {
        return new Promise((resolve, reject) => {
            httpClient.post('/auth/refresh-token', {
                refreshToken: this.tokenStorage.getRefreshToken()
            })
                .then((response: { data: { accessToken: string; refreshToken: string; userId: string } }) => {
                    const data = response.data;
                    if (data?.accessToken) {
                        this.tokenStorage.setTokens(data.accessToken, data.refreshToken);
                        this.user = data.userId;
                        resolve(data.accessToken);
                    } else {
                        reject(new Error('Invalid response: access token missing'));
                    }
                })
                .catch((error: unknown) => {
                    this.tokenStorage.clearTokens();
                    this.isAuthenticated = false;
                    this.user = null;
                    this.notifyLogout();
                    reject(error);
                });
        });
    }
}

export const coreAuth = new CoreAuth();